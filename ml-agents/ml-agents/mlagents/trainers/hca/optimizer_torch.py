# Unity ML-Agents Toolkit
# ## HCA Optimizer (Hierarchical Critic Assignment)
# Implements dual hierarchical critics based on:
#   Cao & Lin (2020) - "Reinforcement Learning from Hierarchical Critics" (RLHC)
#   Cao & Lin (2019) - "Hierarchical Critic Assignment for Multi-agent RL" (HCA)
#
# Key difference from PPO optimizer:
#   PPO uses a SINGLE critic (ValueNetwork) to estimate state values.
#   HCA uses TWO critics — a worker critic (local observations) and a manager
#   critic (global observations) — and combines them via max(V_w, V_m) per RLHC Eq. 16.

from typing import Dict, cast, List, Optional, Tuple
import attr
import numpy as np

from mlagents.torch_utils import torch, default_device

from mlagents.trainers.buffer import AgentBuffer, BufferKey, RewardSignalUtil

from mlagents_envs.timers import timed
from mlagents.trainers.policy.torch_policy import TorchPolicy
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer
from mlagents.trainers.settings import (
    TrainerSettings,
    OnPolicyHyperparamSettings,
    ScheduleType,
    NetworkSettings,
)
from mlagents.trainers.torch_entities.networks import ValueNetwork
from mlagents.trainers.torch_entities.encoders import VectorInput
from mlagents.trainers.torch_entities.agent_action import AgentAction
from mlagents.trainers.torch_entities.action_log_probs import ActionLogProbs
from mlagents.trainers.torch_entities.utils import ModelUtils
from mlagents.trainers.trajectory import ObsUtil


@attr.s(auto_attribs=True)
class HCASettings(OnPolicyHyperparamSettings):
    """
    HCA-specific hyperparameters.
    Extends PPO's on-policy settings with hierarchical critic configuration.
    """

    beta: float = 5.0e-3
    epsilon: float = 0.2
    lambd: float = 0.95
    num_epoch: int = 3
    shared_critic: bool = False
    learning_rate_schedule: ScheduleType = ScheduleType.LINEAR
    beta_schedule: ScheduleType = ScheduleType.LINEAR
    epsilon_schedule: ScheduleType = ScheduleType.LINEAR

    # --- HCA-specific fields ---
    manager_hidden_units: int = 128
    manager_num_layers: int = 2
    manager_learning_rate: float = 3.0e-4
    hca_value_method: str = "max"  # "max" (RLHC Eq.16) or "softmax" (HCA paper)
    manager_obs_index: int = -1  # Index of the manager observation sensor (-1 = last)


class TorchHCAOptimizer(TorchOptimizer):
    """
    HCA Optimizer with hierarchical dual critics.

    Architecture (per RLHC paper):
        - Shared Actor: π(a|s_worker) — same as PPO, uses worker (local) observations
        - Worker Critic: V_w(s_worker) — evaluates local observations
        - Manager Critic: V_m(s_manager) — evaluates global observations
        - Combined Value: V = max(V_w, V_m) — RLHC Eq. 16

    The actor (policy) is NOT modified. Only the value estimation is hierarchical.
    """

    def __init__(self, policy: TorchPolicy, trainer_settings: TrainerSettings):
        super().__init__(policy, trainer_settings)

        reward_signal_configs = trainer_settings.reward_signals
        reward_signal_names = [key.value for key, _ in reward_signal_configs.items()]

        self.hyperparameters: HCASettings = cast(
            HCASettings, trainer_settings.hyperparameters
        )

        # Determine observation specs for worker and manager
        all_obs_specs = list(policy.behavior_spec.observation_specs)
        n_obs = len(all_obs_specs)

        # Manager observation index: by default, the last sensor
        manager_idx = self.hyperparameters.manager_obs_index
        if manager_idx < 0:
            manager_idx = n_obs + manager_idx  # Convert negative index

        # Split observation specs
        self.manager_obs_index = manager_idx
        self.worker_obs_indices = [i for i in range(n_obs) if i != manager_idx]
        self.all_obs_indices = list(range(n_obs))

        worker_obs_specs = [all_obs_specs[i] for i in self.worker_obs_indices]
        manager_obs_specs = [all_obs_specs[manager_idx]]

        # ---- Actor parameters (shared, same as PPO) ----
        actor_worker_params = list(self.policy.actor.parameters())

        # ---- Worker Critic (local observations) ----
        if self.hyperparameters.shared_critic:
            self._worker_critic = policy.actor
        else:
            self._worker_critic = ValueNetwork(
                reward_signal_names,
                worker_obs_specs,
                network_settings=trainer_settings.network_settings,
            )
            self._worker_critic.to(default_device())
            actor_worker_params += list(self._worker_critic.parameters())

        # ---- Manager Critic (global observations) — NEW for HCA ----
        manager_network_settings = NetworkSettings(
            normalize=trainer_settings.network_settings.normalize,
            hidden_units=self.hyperparameters.manager_hidden_units,
            num_layers=self.hyperparameters.manager_num_layers,
        )
        self._manager_critic = ValueNetwork(
            reward_signal_names,
            manager_obs_specs,
            network_settings=manager_network_settings,
        )
        self._manager_critic.to(default_device())
        manager_params = list(self._manager_critic.parameters())

        # ---- Learning rate decay (same schedule as PPO) ----
        self.decay_learning_rate = ModelUtils.DecayedValue(
            self.hyperparameters.learning_rate_schedule,
            self.hyperparameters.learning_rate,
            1e-10,
            self.trainer_settings.max_steps,
        )
        self.decay_manager_learning_rate = ModelUtils.DecayedValue(
            self.hyperparameters.learning_rate_schedule,
            self.hyperparameters.manager_learning_rate,
            1e-10,
            self.trainer_settings.max_steps,
        )
        self.decay_epsilon = ModelUtils.DecayedValue(
            self.hyperparameters.epsilon_schedule,
            self.hyperparameters.epsilon,
            0.1,
            self.trainer_settings.max_steps,
        )
        self.decay_beta = ModelUtils.DecayedValue(
            self.hyperparameters.beta_schedule,
            self.hyperparameters.beta,
            1e-5,
            self.trainer_settings.max_steps,
        )

        # Optimizer with separate parameter groups for actor/worker vs manager critic
        self.optimizer = torch.optim.Adam(
            [
                {"params": actor_worker_params, "lr": self.hyperparameters.learning_rate},
                {"params": manager_params, "lr": self.hyperparameters.manager_learning_rate},
            ]
        )

        self.stats_name_to_update_name = {
            "Losses/Value Loss": "value_loss",
            "Losses/Policy Loss": "policy_loss",
        }

        self.stream_names = list(self.reward_signals.keys())

    # ---- Properties ----

    @property
    def critic(self):
        """Returns the worker critic as the 'primary' critic for base class compatibility."""
        return self._worker_critic

    @property
    def manager_critic(self):
        """Returns the manager critic."""
        return self._manager_critic

    # ---- Normalization updates (index-aware) ----

    def update_worker_normalization(self, buffer: AgentBuffer) -> None:
        """
        Update normalization for the worker critic using only worker observations.
        Reads from the correct buffer indices (skipping the manager obs).
        """
        n_obs = len(self.policy.behavior_spec.observation_specs)
        all_obs = ObsUtil.from_buffer(buffer, n_obs)
        worker_obs_bufs = [all_obs[i] for i in self.worker_obs_indices]

        for obs_buf, enc in zip(worker_obs_bufs, self._worker_critic.network_body.observation_encoder.processors):
            if isinstance(enc, VectorInput):
                enc.update_normalization(torch.as_tensor(obs_buf.to_ndarray()))

    def update_manager_normalization(self, buffer: AgentBuffer) -> None:
        """
        Update normalization for the manager critic using only the manager observation.
        Reads from the correct buffer index (manager_obs_index), NOT index 0.
        """
        n_obs = len(self.policy.behavior_spec.observation_specs)
        all_obs = ObsUtil.from_buffer(buffer, n_obs)
        manager_obs_buf = all_obs[self.manager_obs_index]

        for enc in self._manager_critic.network_body.observation_encoder.processors:
            if isinstance(enc, VectorInput):
                enc.update_normalization(torch.as_tensor(manager_obs_buf.to_ndarray()))

    # ---- Helper: split observations ----

    def _split_obs(
        self, all_obs: List[torch.Tensor]
    ) -> Tuple[List[torch.Tensor], List[torch.Tensor]]:
        """
        Split a list of observation tensors into worker and manager observations.
        """
        worker_obs = [all_obs[i] for i in self.worker_obs_indices]
        manager_obs = [all_obs[self.manager_obs_index]]
        return worker_obs, manager_obs

    # ---- Hierarchical value combination (RLHC Eq. 16) ----

    def _combine_values(
        self,
        worker_values: Dict[str, torch.Tensor],
        manager_values: Dict[str, torch.Tensor],
    ) -> Dict[str, torch.Tensor]:
        """
        Combine worker and manager value estimates using the hierarchical
        value function from RLHC (Eq. 16):

            V̂(s, θ) = max ∪ᵢ₌₁ᵐ V̂ⁱ(s, θ)

        For the softmax variant (HCA paper), we use a weighted softmax instead.
        """
        combined = {}
        method = self.hyperparameters.hca_value_method

        for name in worker_values:
            w_val = worker_values[name]
            m_val = manager_values[name]

            if method == "max":
                # RLHC Eq. 16: element-wise max
                combined[name] = torch.max(w_val, m_val)
            elif method == "softmax":
                # HCA paper: softmax-weighted combination
                stacked = torch.stack([w_val, m_val], dim=-1)  # (..., 2)
                weights = torch.softmax(stacked, dim=-1)
                combined[name] = (stacked * weights).sum(dim=-1)
            else:
                # Default: mean
                combined[name] = (w_val + m_val) / 2.0

        return combined

    # ---- Trajectory value estimates (overrides base class behavior) ----

    def get_trajectory_value_estimates(
        self,
        batch: AgentBuffer,
        next_obs: List[np.ndarray],
        done: bool,
        agent_id: str = "",
    ) -> Tuple[Dict[str, np.ndarray], Dict[str, float], Optional[List]]:
        """
        Override: Get HIERARCHICAL value estimates for a trajectory.
        Uses both worker and manager critics and combines them via max/softmax.
        """
        n_obs = len(self.policy.behavior_spec.observation_specs)

        current_obs = [
            ModelUtils.list_to_tensor(obs) for obs in ObsUtil.from_buffer(batch, n_obs)
        ]
        next_obs_tensors = [ModelUtils.list_to_tensor(obs) for obs in next_obs]
        next_obs_tensors = [obs.unsqueeze(0) for obs in next_obs_tensors]

        with torch.no_grad():
            # Split observations
            worker_obs, manager_obs = self._split_obs(current_obs)
            worker_next_obs, manager_next_obs = self._split_obs(next_obs_tensors)

            # Worker critic pass
            worker_values, _ = self._worker_critic.critic_pass(
                worker_obs, sequence_length=batch.num_experiences
            )

            # Manager critic pass
            manager_values, _ = self._manager_critic.critic_pass(
                manager_obs, sequence_length=batch.num_experiences
            )

            # Combine: V = max(V_worker, V_manager) — RLHC Eq. 16
            combined_values = self._combine_values(worker_values, manager_values)

            # Next step values (for bootstrapping)
            worker_next_values, _ = self._worker_critic.critic_pass(
                worker_next_obs, sequence_length=1
            )
            manager_next_values, _ = self._manager_critic.critic_pass(
                manager_next_obs, sequence_length=1
            )
            combined_next_values = self._combine_values(
                worker_next_values, manager_next_values
            )

        # Convert to numpy
        value_estimates = {}
        next_value_estimate = {}
        for name in combined_values:
            value_estimates[name] = ModelUtils.to_numpy(combined_values[name])
            next_value_estimate[name] = ModelUtils.to_numpy(
                combined_next_values[name]
            )

        if done:
            for k in next_value_estimate:
                if not self.reward_signals[k].ignore_done:
                    next_value_estimate[k] = 0.0

        return value_estimates, next_value_estimate, None

    # ---- Update (PPO loss with dual value losses) ----

    @timed
    def update(self, batch: AgentBuffer, num_sequences: int) -> Dict[str, float]:
        """
        Performs HCA update on model.

        Same PPO clipped policy loss, but with:
        - Worker value loss (worker critic on worker obs vs returns)
        - Manager value loss (manager critic on manager obs vs returns)
        - Total value loss = worker_loss + manager_loss
        """
        # Get decayed parameters
        decay_lr = self.decay_learning_rate.get_value(self.policy.get_current_step())
        decay_manager_lr = self.decay_manager_learning_rate.get_value(
            self.policy.get_current_step()
        )
        decay_eps = self.decay_epsilon.get_value(self.policy.get_current_step())
        decay_bet = self.decay_beta.get_value(self.policy.get_current_step())

        returns = {}
        old_values = {}
        for name in self.reward_signals:
            old_values[name] = ModelUtils.list_to_tensor(
                batch[RewardSignalUtil.value_estimates_key(name)]
            )
            returns[name] = ModelUtils.list_to_tensor(
                batch[RewardSignalUtil.returns_key(name)]
            )

        n_obs = len(self.policy.behavior_spec.observation_specs)
        current_obs = ObsUtil.from_buffer(batch, n_obs)
        current_obs = [ModelUtils.list_to_tensor(obs) for obs in current_obs]

        # Split observations for worker and manager
        worker_obs, manager_obs = self._split_obs(current_obs)

        act_masks = ModelUtils.list_to_tensor(batch[BufferKey.ACTION_MASK])
        actions = AgentAction.from_buffer(batch)

        memories = [
            ModelUtils.list_to_tensor(batch[BufferKey.MEMORY][i])
            for i in range(
                0, len(batch[BufferKey.MEMORY]), self.policy.sequence_length
            )
        ]
        if len(memories) > 0:
            memories = torch.stack(memories).unsqueeze(0)

        # --- Actor forward pass (uses all obs, same as PPO) ---
        run_out = self.policy.actor.get_stats(
            current_obs,
            actions,
            masks=act_masks,
            memories=memories,
            sequence_length=self.policy.sequence_length,
        )

        log_probs = run_out["log_probs"]
        entropy = run_out["entropy"]

        # --- Worker Critic forward pass (local observations) ---
        worker_values, _ = self._worker_critic.critic_pass(
            worker_obs, sequence_length=self.policy.sequence_length
        )

        # --- Manager Critic forward pass (global observations) ---
        manager_values, _ = self._manager_critic.critic_pass(
            manager_obs, sequence_length=self.policy.sequence_length
        )

        # --- Compute losses ---
        old_log_probs = ActionLogProbs.from_buffer(batch).flatten()
        log_probs = log_probs.flatten()
        loss_masks = ModelUtils.list_to_tensor(
            batch[BufferKey.MASKS], dtype=torch.bool
        )

        # Worker value loss
        worker_value_loss = ModelUtils.trust_region_value_loss(
            worker_values, old_values, returns, decay_eps, loss_masks
        )

        # Manager value loss
        manager_value_loss = ModelUtils.trust_region_value_loss(
            manager_values, old_values, returns, decay_eps, loss_masks
        )

        # Combined value loss (both critics learn to predict the same returns)
        value_loss = worker_value_loss + manager_value_loss

        # Policy loss (standard PPO clipped — uses advantages computed from combined values)
        policy_loss = ModelUtils.trust_region_policy_loss(
            ModelUtils.list_to_tensor(batch[BufferKey.ADVANTAGES]),
            log_probs,
            old_log_probs,
            loss_masks,
            decay_eps,
        )

        # Total loss (value_loss is already 0.5 * (worker + manager))
        loss = (
            policy_loss
            + value_loss
            - decay_bet * ModelUtils.masked_mean(entropy, loss_masks)
        )

        # Optimize: update learning rates for each param group individually
        self.optimizer.param_groups[0]["lr"] = decay_lr
        self.optimizer.param_groups[1]["lr"] = decay_manager_lr
        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()

        update_stats = {
            "Losses/Policy Loss": torch.abs(policy_loss).item(),
            "Losses/Value Loss": value_loss.item(),
            "Losses/HCA Worker Value Loss": worker_value_loss.item(),
            "Losses/HCA Manager Value Loss": manager_value_loss.item(),
            "Policy/Learning Rate": decay_lr,
            "Policy/Manager Learning Rate": decay_manager_lr,
            "Policy/Epsilon": decay_eps,
            "Policy/Beta": decay_bet,
        }

        return update_stats

    def get_modules(self):
        modules = {
            "Optimizer:value_optimizer": self.optimizer,
            "Optimizer:worker_critic": self._worker_critic,
            "Optimizer:manager_critic": self._manager_critic,
        }
        for reward_provider in self.reward_signals.values():
            modules.update(reward_provider.get_modules())
        return modules

