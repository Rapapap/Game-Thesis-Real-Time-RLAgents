# Unity ML-Agents Toolkit
# ## ML-Agent Learning (HCA - Hierarchical Critic Assignment)
# Based on: Cao & Lin (2020) - "Reinforcement Learning from Hierarchical Critics"
#
# This trainer inherits from PPOTrainer and overrides only what is needed:
#   1. create_optimizer() → returns TorchHCAOptimizer (dual critics) instead of TorchPPOOptimizer
#   2. _process_trajectory() → updates normalization for both worker and manager critics
#   3. get_trainer_name() → returns "hca"
#
# Everything else (training loop, GAE computation, policy creation) is inherited from PPO.

from typing import cast, Type, Union, Dict, Any

import numpy as np

from mlagents_envs.base_env import BehaviorSpec
from mlagents_envs.logging_util import get_logger
from mlagents.trainers.buffer import BufferKey, RewardSignalUtil
from mlagents.trainers.trainer.on_policy_trainer import OnPolicyTrainer
from mlagents.trainers.policy.policy import Policy
from mlagents.trainers.trainer.trainer_utils import get_gae
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer
from mlagents.trainers.policy.torch_policy import TorchPolicy
from mlagents.trainers.hca.optimizer_torch import TorchHCAOptimizer, HCASettings
from mlagents.trainers.trajectory import Trajectory
from mlagents.trainers.behavior_id_utils import BehaviorIdentifiers
from mlagents.trainers.settings import TrainerSettings

from mlagents.trainers.torch_entities.networks import SimpleActor, SharedActorCritic

logger = get_logger(__name__)

TRAINER_NAME = "hca"


class HCATrainer(OnPolicyTrainer):
    """
    HCA Trainer — Hierarchical Critic Assignment.

    Inherits PPO's training loop but uses dual hierarchical critics
    (worker + manager) to provide better value estimation.

    The only differences from PPOTrainer:
    - create_optimizer() returns TorchHCAOptimizer
    - _process_trajectory() also updates manager critic normalization
    - Reports separate worker/manager value estimates to TensorBoard
    """

    def __init__(
        self,
        behavior_name: str,
        reward_buff_cap: int,
        trainer_settings: TrainerSettings,
        training: bool,
        load: bool,
        seed: int,
        artifact_path: str,
    ):
        super().__init__(
            behavior_name,
            reward_buff_cap,
            trainer_settings,
            training,
            load,
            seed,
            artifact_path,
        )
        self.hyperparameters: HCASettings = cast(
            HCASettings, self.trainer_settings.hyperparameters
        )
        self.seed = seed
        self.shared_critic = self.hyperparameters.shared_critic
        self.policy: TorchPolicy = None  # type: ignore

    def _process_trajectory(self, trajectory: Trajectory) -> None:
        """
        Takes a trajectory and processes it, putting it into the update buffer.
        Same as PPO but also updates manager critic normalization.
        """
        super()._process_trajectory(trajectory)
        agent_id = trajectory.agent_id

        agent_buffer_trajectory = trajectory.to_agentbuffer()
        self._warn_if_group_reward(agent_buffer_trajectory)

        # Update normalization for actor + BOTH critics (index-aware)
        if self.is_training:
            self.policy.actor.update_normalization(agent_buffer_trajectory)
            # Use index-aware methods that read from correct buffer indices
            self.optimizer.update_worker_normalization(agent_buffer_trajectory)
            self.optimizer.update_manager_normalization(agent_buffer_trajectory)

        # Get hierarchical value estimates (uses combined max(V_w, V_m))
        (
            value_estimates,
            value_next,
            value_memories,
        ) = self.optimizer.get_trajectory_value_estimates(
            agent_buffer_trajectory,
            trajectory.next_obs,
            trajectory.done_reached and not trajectory.interrupted,
        )
        if value_memories is not None:
            agent_buffer_trajectory[BufferKey.CRITIC_MEMORY].set(value_memories)

        for name, v in value_estimates.items():
            agent_buffer_trajectory[
                RewardSignalUtil.value_estimates_key(name)
            ].extend(v)
            self._stats_reporter.add_stat(
                f"Policy/{self.optimizer.reward_signals[name].name.capitalize()} Value Estimate",
                np.mean(v),
            )

        # Evaluate all reward functions
        self.collected_rewards["environment"][agent_id] += np.sum(
            agent_buffer_trajectory[BufferKey.ENVIRONMENT_REWARDS]
        )
        for name, reward_signal in self.optimizer.reward_signals.items():
            evaluate_result = (
                reward_signal.evaluate(agent_buffer_trajectory)
                * reward_signal.strength
            )
            agent_buffer_trajectory[RewardSignalUtil.rewards_key(name)].extend(
                evaluate_result
            )
            self.collected_rewards[name][agent_id] += np.sum(evaluate_result)

        # Compute GAE and returns (same as PPO)
        tmp_advantages = []
        tmp_returns = []
        for name in self.optimizer.reward_signals:
            bootstrap_value = value_next[name]

            local_rewards = agent_buffer_trajectory[
                RewardSignalUtil.rewards_key(name)
            ].get_batch()
            local_value_estimates = agent_buffer_trajectory[
                RewardSignalUtil.value_estimates_key(name)
            ].get_batch()

            local_advantage = get_gae(
                rewards=local_rewards,
                value_estimates=local_value_estimates,
                value_next=bootstrap_value,
                gamma=self.optimizer.reward_signals[name].gamma,
                lambd=self.hyperparameters.lambd,
            )
            local_return = local_advantage + local_value_estimates
            agent_buffer_trajectory[RewardSignalUtil.returns_key(name)].set(
                local_return
            )
            agent_buffer_trajectory[RewardSignalUtil.advantage_key(name)].set(
                local_advantage
            )
            tmp_advantages.append(local_advantage)
            tmp_returns.append(local_return)

        # Get global advantages
        global_advantages = list(
            np.mean(np.array(tmp_advantages, dtype=np.float32), axis=0)
        )
        global_returns = list(
            np.mean(np.array(tmp_returns, dtype=np.float32), axis=0)
        )
        agent_buffer_trajectory[BufferKey.ADVANTAGES].set(global_advantages)
        agent_buffer_trajectory[BufferKey.DISCOUNTED_RETURNS].set(global_returns)

        self._append_to_update_buffer(agent_buffer_trajectory)

        if trajectory.done_reached:
            self._update_end_episode_stats(agent_id, self.optimizer)

    def create_optimizer(self) -> TorchOptimizer:
        """Creates the HCA optimizer with dual hierarchical critics."""
        return TorchHCAOptimizer(  # type: ignore
            cast(TorchPolicy, self.policy), self.trainer_settings  # type: ignore
        )  # type: ignore

    def create_policy(
        self, parsed_behavior_id: BehaviorIdentifiers, behavior_spec: BehaviorSpec
    ) -> TorchPolicy:
        """
        Creates a policy with a PyTorch backend and HCA hyperparameters.
        The policy (actor) is the same as PPO — only the critics differ.
        """
        actor_cls: Union[Type[SimpleActor], Type[SharedActorCritic]] = SimpleActor
        actor_kwargs: Dict[str, Any] = {
            "conditional_sigma": False,
            "tanh_squash": False,
        }
        if self.shared_critic:
            reward_signal_configs = self.trainer_settings.reward_signals
            reward_signal_names = [
                key.value for key, _ in reward_signal_configs.items()
            ]
            actor_cls = SharedActorCritic
            actor_kwargs.update({"stream_names": reward_signal_names})

        policy = TorchPolicy(
            self.seed,
            behavior_spec,
            self.trainer_settings.network_settings,
            actor_cls,
            actor_kwargs,
        )
        return policy

    def get_policy(self, name_behavior_id: str) -> Policy:
        return self.policy

    @staticmethod
    def get_trainer_name() -> str:
        return TRAINER_NAME

