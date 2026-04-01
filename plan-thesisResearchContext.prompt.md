# Thesis Research Context & Plan

## Overview

**Title:** Penerapan Hierarchical Reinforcement Learning pada Non-Playable Character Musuh di Game 3D Action Hack and Slash  
**Author:** Rava Radithya Razan (10122108)  
**Institution:** Universitas Komputer Indonesia, Program Studi Teknik Informatika  
**Game:** "Code Crusader: Anti-Virus Assault"  
**Year:** 2026  

This research **continues and improves upon** the previous thesis by **Zaky** (scored 85-95), which implemented standard PPO-based RL for enemy NPCs. The current research addresses the shortcomings found in the prior implementation.

---

## Problem Statement

The previous research applied **Proximal Policy Optimization (PPO)** to NPC enemies in the game. Despite using RL, survey data from 106 players revealed:

| Issue | Percentage |
|---|---|
| Players found enemy behavior confusing | **84%** |
| Technical incidents: navigation failures | **70%** |
| Enemies not attacking within attack range | **60%** |

**Root Cause:** Standard (flat) PPO struggles with:
- **Long-term credit assignment** — agents cannot connect actions to distant future rewards
- **Imprecise value evaluation** — the single critic cannot evaluate high-level strategy and low-level execution separately
- **Navigation + attack coordination failures** — the agent tries to learn everything in one flat policy

---

## Proposed Solution: Hierarchical Critic Assignment (HCA)

The research applies **Hierarchical Critic Assignment (HCA)** based on the work by Cao & Lin (2020), which introduces:

### Architecture
- **Manager-level critic (global):** Evaluates high-level strategic decisions (navigation goals, engagement decisions)
- **Worker-level critic (local):** Evaluates low-level action execution (movement, attacking, dodging)
- **Shared actor:** Both levels share the action policy, but receive different evaluative signals

### How HCA Works
1. Each agent receives value information from **both local and global critics**
2. The updated value function uses a **softmax over multiple critics**:
   ```
   V̂(s, θ) = max ∪ᵢ₌₁ᵐ V̂ⁱ(s, θ)
   ```
3. By selecting the maximum value from hierarchical critics, the advantage function is minimized, leading to more stable policy updates
4. The manager observes broader environmental context, the worker observes local state

### Expected Benefits
- Faster convergence compared to flat PPO
- More precise credit assignment across hierarchical levels
- Better navigation-attack coordination

---

## Previous Research (Zaky's Thesis) — Key Design Decisions

### Environment
- **Normal Enemy Arena:** 9m × 9.5m arena with obstacles (Digi-Chest, Destructible Box, Wall)
- **Agent types:** Creep, Humanoid, Bull (all share same RL state/action/reward)

### States
| State | Description |
|---|---|
| Idle | Default state before any action |
| Pathfinding | Navigating/patrolling the arena |
| Position of Agent on Arena | Detecting player based on position |
| Attacking | Engaging player within attack range |
| Death | HP ≤ 0, drops loot |

### Actions
| Action | Description |
|---|---|
| Idle | Standing still |
| Patroling | Moving through patrol points |
| Detecting | Detecting player presence |
| Chasing | Pursuing detected player |
| Attacking | Attacking player in range |
| Dead | Agent dies |

### Reward Policy (from Zaky's Thesis - Table 3.30)
| # | Policy | Reward Value |
|---|---|---|
| 1 | Agent stuck/not moving for 50 steps or 2s | Episode skipped |
| 2 | Agent not moving at all | −0.010 (per step) |
| 3 | Agent approaches wall/obstacle | −0.20 (per step) |
| 4 | Agent successfully patrols | +0.005 (per step) |
| 5 | Agent completes one patrol rotation | +0.5 |
| 6 | Agent fails to patrol | −0.01 (per step) |
| 7 | Agent approaches player | +0.01 |
| 8 | Agent fails to approach player | −0.05 (per step) |
| 9 | Agent detects player during patrol | +0.5 |
| 10 | Agent chases after detecting | +0.010 (per step) |
| 11 | Agent successfully catches player | +0.5 |
| 12 | Agent doesn't chase player | −0.05 (per step) |
| 13 | Agent attacks after chasing | +0.5 |
| 14 | Agent doesn't attack immediately | −0.01 (per step) |
| 15 | Agent's attack misses | −0.1 |
| 16 | Agent gets hit by player | −0.5 |
| 17 | Agent wins against player | +1 |
| 18 | Agent loses/dies | −1 |

### Hyperparameters (Zaky's thesis)
Configured based on Unity ML-Agents documentation and reference research on "Comparative Study of SAC and PPO in Multi-Agent RL Using Unity ML-Agents" (2023).

---

## Current Implementation Status

### Training Configurations

**PPO Baseline** (`config/ppo/NormalEnemyCC.yaml` — with curriculum, `NormalEnemyCC_NoCurriculum.yaml` — without):
```yaml
behaviors:
  NormalEnemy:
    trainer_type: ppo
    max_steps: 5000000
    time_horizon: 512
    summary_freq: 10000
    threaded: false
    hyperparameters:
      batch_size: 2048
      buffer_size: 20480
      learning_rate: 0.0001
      beta: 0.01
      epsilon: 0.15
      lambd: 0.95
      num_epoch: 4
      learning_rate_schedule: constant
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 3
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
```

**HCA Experiment** (`config/hca/NormalEnemyHCA.yaml` — with curriculum, `NormalEnemyHCA_NoCurriculum.yaml` — without):
```yaml
behaviors:
  NormalEnemy:
    trainer_type: hca
    max_steps: 5000000
    time_horizon: 512
    summary_freq: 10000
    threaded: false
    hyperparameters:
      batch_size: 2048
      buffer_size: 20480
      learning_rate: 0.0001
      beta: 0.01
      epsilon: 0.15
      lambd: 0.95
      num_epoch: 4
      learning_rate_schedule: constant
      # --- HCA-Specific ---
      manager_hidden_units: 128
      manager_num_layers: 2
      manager_learning_rate: 0.0003
      hca_value_method: softmax
      manager_obs_index: -1
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 3
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
```

**Shared Hyperparameters (both PPO & HCA):**
```yaml
    time_horizon: 512
    summary_freq: 10000
    threaded: false
    hyperparameters:
      batch_size: 2048
      buffer_size: 20480
      learning_rate: 0.0001
      beta: 0.01
      epsilon: 0.15
      lambd: 0.95
      num_epoch: 4
      learning_rate_schedule: constant
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 3
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
```

**Training Commands:**
```bash
# HCA with curriculum
mlagents-learn config/hca/NormalEnemyHCA.yaml --run-id=HCA-Curriculum

# HCA without curriculum
mlagents-learn config/hca/NormalEnemyHCA_NoCurriculum.yaml --run-id=HCA-NoCurriculum

# PPO with curriculum
mlagents-learn config/ppo/NormalEnemyCC.yaml --run-id=PPO-Curriculum

# PPO without curriculum
mlagents-learn config/ppo/NormalEnemyCC_NoCurriculum.yaml --run-id=PPO-NoCurriculum

# Compare in TensorBoard
tensorboard --logdir=results
```

### Past Training Results
| Run | Mean Reward | Std Reward | Steps | Duration |
|---|---|---|---|---|
| NormalEnemyRun1 | 2.97 | -25.47 | 10,000,000 | 4.28 hr |
| NormalEnemyRun2 | 49.32 | 50 | 12,730,000 | 4.10 hr |
| NormalEnemyRunTest2 | 33.88 | 30.88 | 15,000,000 | 13.08 hr |
| NormalEnemyRunTest | 44.86 | 39.98 | 5,570,000 | 12.72 hr |
| rava-train-test | **103.41** | 109.12 | **500,000** | 1.75 hr |
| **HCA-experiment** | **134.79** (smoothed) | 104.23 | **1,500,000** | 1.345 hr |

**Key Observation:** HCA achieves ~30% higher smoothed reward (134.79 vs 103.41) than the best PPO run, demonstrating the benefit of hierarchical dual-critic value estimation.

### Agent Architecture (`NormalEnemyAgents.cs`)
- **Observations (24 vector):**
  - Health (1): normalized HP
  - Player-related (4): availability, direction (local x/z), distance
  - Velocity (2): local x/z velocity
  - Obstacle detection (8): 4 cardinal + 4 diagonal raycasts
  - State flags (3): knocked back, fleeing, should flee
  - Patrol target (3): direction + distance to patrol point
  - Enemy stats (3): normalized maxHP, attack, speed (enables type-conditioned behavior)
  - Plus: RayPerceptionSensor3D for visual detection

- **Actions:**
  - Continuous (3): forward, right, rotation
  - Discrete (1): attack (0/1)

- **Movement:** Rigidbody-based with `AddForce()`, continuous collision detection, velocity clamping — agents will NOT clip through walls

- **NavMeshAgent:** Explicitly disabled in training (`DisableConflictingComponents()`)

### Scene Setup
- **Training Scene:** `Training Normal Enemy` — multiple arenas (Easy/Medium/Hard/Ultra) with NavMeshSurface present but unused
- **Deployment Scene:** `Reinforcement Learning Stage` — the actual game level
- **Currently active arena:** Hard difficulty (1 active, others disabled)

### Training Config File Structure
```
config/
├── hca/
│   ├── NormalEnemyHCA.yaml              ← HCA with curriculum learning
│   └── NormalEnemyHCA_NoCurriculum.yaml ← HCA without curriculum (static player)
├── ppo/
│   ├── NormalEnemyCC.yaml               ← PPO with curriculum learning
│   └── NormalEnemyCC_NoCurriculum.yaml  ← PPO without curriculum (static player)
```

### Training Scripts
| Script | Location | Purpose |
|---|---|---|
| `RL_TrainingManager.cs` | `Assets/Script/RL Scripts/Training/` | Episode management, arena resets |
| `RL_TrainingEnemySpawner.cs` | `Assets/Script/RL Scripts/Training/` | Spawns Creep/Humanoid/Bull agent prefabs |
| `RL_TrainingPlayerSpawner.cs` | `Assets/Script/RL Scripts/Training/` | Spawns training player targets |
| `RL_TrainingPlayer.cs` | `Assets/Script/RL Scripts/Training/` | Destruction/lifecycle tracking for player |
| `RL_CurriculumPlayerController.cs` | `Assets/Script/RL Scripts/Training/` | Curriculum-aware player (4 difficulty stages) |

---

## Known Issues Identified

1. **Agents stop respawning** during training after some time — likely related to episode management in `RL_TrainingManager`
2. ~~**No wall/obstacle collision punishment**~~ — ✅ FIXED: collision punishment implemented in `OnCollisionEnter`/`OnCollisionStay`
3. ~~**Chase reward given even when stuck**~~ — ✅ FIXED: chase reward only given when distance to player actually decreases (`ProcessChaseRewards`)
4. **Persistent allocator leak warning** — Unity memory leak in persistent allocations
5. **Training/deployment scene mismatch** — different environments may cause policy transfer issues
6. ~~**No convergence/early stopping**~~ — ✅ FIXED: convergence detection implemented in `trainer.py` / `rl_trainer.py`
7. ~~**Kinematic Rigidbody velocity error**~~ — ✅ FIXED: `RL_CurriculumPlayerController` uses `MovePosition` for kinematic bodies

---

## Convergence/Early Stopping Implementation

Added to the ML-Agents trainer source code (3 files modified):

### `settings.py` — New TrainerSettings fields:
```python
convergence_threshold: Optional[float] = None   # std-dev threshold
convergence_window: int = 10                      # summary checkpoints to average
convergence_min_steps: int = 100000               # min steps before checking
convergence_patience: int = 3                      # consecutive checks before stop
```

### `trainer.py` — Convergence detection in `Trainer` base class:
- `should_still_train` now checks `_converged` flag
- `check_convergence()` method tracks reward std-dev over rolling window

### `rl_trainer.py` — Hook in `_write_summary()`:
- Feeds mean reward into convergence checker every `summary_freq` steps

---

## Research Methodology: GDLC (Game Development Life Cycle)

1. **Initiation** — Identify problems via player survey data
2. **Pre-Production** — Prepare dev environment, assets, design documents
3. **RL Design** — Design environment boundaries, observations, rewards, hyperparameters, policy update logic
4. **Production** — Implement and train agents using Unity ML-Agents
5. **Testing** — Evaluate via:
   - Technical evaluation (convergence, reward metrics)
   - Expert validation (game development professionals)
   - Player experience evaluation using **GEQ (Game Experience Questionnaire)**

---

## Evaluation Metrics

| Metric | Description |
|---|---|
| **Cumulative Reward** | Total reward per episode |
| **Average Reward** | Mean reward across episodes |
| **Convergence Rate** | Speed of policy stabilization |
| **Success Rate** | Percentage of successful episodes |
| **GEQ Scores** | Player experience (immersion, competence, flow, challenge) |

---

## Key Bibliography References

| Reference | Relevance |
|---|---|
| Cao & Lin (2020) — "Reinforcement Learning from Hierarchical Critics" | **Primary algorithm (RLHC/HCA)** — introduces multi-level cooperative critics for competition tasks |
| Cao & Lin (2019) — "Hierarchical Critic Assignment for MARL" | **Framework paper** — HCA framework with softmax-based critic assignment, tested on Unity tennis |
| Schulman et al. (2017) — "Proximal Policy Optimization Algorithms" | **Baseline algorithm (PPO)** — clipped surrogate objective, actor-critic architecture |
| Vezhnevets et al. (2017) — "Feudal Networks for HRL" | **Hierarchical RL foundation** — manager/worker modules for goal decomposition |
| Husniah et al. (2018) — "Gamification and GDLC" | **Methodology** — GDLC framework adapted for RL development |
| Yannakakis & Togelius (2018) — "AI and Games" | **Game AI foundation** — games as AI simulation environments |
| Lanham (2020) — "Hands-on RL for Games" | **Unity ML-Agents reference** — practical RL implementation in Unity |
| Sutton & Barto — "RL: An Introduction" | **RL fundamentals** — core theory, MDP, policy optimization |

---

## Next Steps (TODO)

### Phase 0: Design — States, Actions & Reward Policy for HCA
> **Priority: CRITICAL — must be designed before any code is written**

#### 0.1 Redesign States for Hierarchical Levels

The HCA architecture requires separating state representations into **worker-level (local)** and **manager-level (global)** observations.

**Worker-Level States (Local Critic):**
These are the agent's immediate, self-centered observations — what the agent can perceive locally.

| # | State | Observation | Size | Description |
|---|---|---|---|---|
| 1 | Self Health | `currentHP / maxHP` | 1 | Normalized health ratio |
| 2 | Player Available | `0 or 1` | 1 | Whether player is detected |
| 3 | Player Direction (local) | `localDir.x, localDir.z` | 2 | Direction to player in agent's local space |
| 4 | Player Distance | `dist / 50` | 1 | Normalized distance to player |
| 5 | Agent Velocity (local) | `localVel.x, localVel.z` | 2 | Agent's velocity in local space |
| 6 | Obstacle Distances | `8 directional raycasts` | 8 | Cardinal + diagonal obstacle proximity |
| 7 | Combat State Flags | `knockedBack, fleeing, shouldFlee` | 3 | Agent's current combat state |
| 8 | Patrol Target | `localPatrolDir.x, localPatrolDir.z, patrolDist` | 3 | Direction/distance to patrol point |
| 9 | Enemy Stats | `maxHP/200, attack/10, speed/10` | 3 | Normalized agent type stats (enables type-conditioned behavior) |
| | **Worker Total** | | **24** | Extended from original 21 to include agent identity stats |

**Manager-Level States (Global Critic):**
These are broader, arena-wide observations that provide strategic context — what a "commander" would see.

| # | State | Observation | Size | Description |
|---|---|---|---|---|
| 1 | Agent World Position | `norm(pos.x), norm(pos.z)` | 2 | Agent position normalized to arena bounds |
| 2 | Player World Position | `norm(playerPos.x), norm(playerPos.z)` | 2 | Player position normalized to arena bounds |
| 3 | Relative Position (world) | `norm(relativeDir.x), norm(relativeDir.z)` | 2 | World-space direction agent→player |
| 4 | Player Distance (absolute) | `dist / arenaSize` | 1 | Distance normalized to arena diagonal |
| 5 | Agent Health Ratio | `currentHP / maxHP` | 1 | Same as worker but for global context |
| 6 | Agent Current Behavior | `isPatrolling, isChasing, isAttacking, isFleeing, isIdle` | 5 | One-hot or binary state flags |
| 7 | Arena Quadrant | `quadrant (0-3)` | 1 | Which quadrant of the arena agent is in |
| 8 | Time Pressure | `stepCount / maxStepPerEpisode` | 1 | Episode progress (encourages urgency) |
| 9 | Engagement Success | `recentHitRatio` | 1 | Rolling ratio of successful attacks |
| | **Manager Total** | | **16** | Broader strategic context |

> **Key Design Decision:** The manager critic receives a *different, broader* observation set than the worker. This follows the RLHC paper (Cao & Lin, 2020) where "the manager observations include additional variables... and information gained from the worker agent's observations." The worker actor and worker critic share observations (local view, 24-dim including enemy stats). The manager critic alone uses the global view (16-dim). Enemy stats (maxHP, attack, speed) are included in the worker observations so the shared policy can learn type-conditioned behavior, enabling a single neural network to handle Creep, Humanoid, and Bull agents with appropriate strategies.

#### 0.2 Actions (Unchanged — Shared Actor)

Per the HCA/RLHC architecture, the **actor (policy) is shared** between manager and worker levels. Both critics evaluate the *same* actions. The action space does NOT change:

| Type | Index | Action | Range | Description |
|---|---|---|---|---|
| Continuous | 0 | Forward/Backward | [-1, 1] | Movement along agent's forward axis |
| Continuous | 1 | Left/Right Strafe | [-1, 1] | Movement along agent's right axis |
| Continuous | 2 | Rotation | [-1, 1] | Yaw rotation |
| Discrete | 0 | Attack | {0, 1} | 0 = don't attack, 1 = attack |

> **Why actions don't change:** Unlike HAC (Hierarchical Actor-Critic by Levy et al.) which has separate actors at each level, HCA/RLHC uses a **single shared actor** with **multiple critics**. The manager doesn't output subgoals — it provides a *global value signal* to improve the worker's policy gradient.

#### 0.3 Reward Policy for HCA — Hierarchical Reward Design

The core innovation of HCA is that reward signals are evaluated by **two different critics with different perspectives**. Both critics evaluate the same reward, but from different observation viewpoints, allowing the advantage function to capture both local execution quality and global strategic value.

**Single Reward Signal (shared between both critics):**

The reward function remains unified. Both the worker critic and manager critic learn to predict the value of the *same* cumulative reward stream — but from different state representations. The combined advantage uses `max(V_worker, V_manager)` per the RLHC formula.

**Revised Reward Policy Table:**

| # | Policy | Reward | Type | Rationale for HCA |
|---|---|---|---|---|
| **Navigation Rewards** | | | | |
| 1 | Agent stuck/not moving ≥ 2s | −0.01/step | Per-step | Worker critic: detects stuck via velocity; Manager critic: detects positional stagnation |
| 2 | Agent collides with wall/obstacle/enemy | −0.02 × dt | Per-step | Worker critic: obstacle raycasts predict collision; Manager critic: sees agent near arena edges |
| 3 | Agent successfully patrols | +0.001 × dt | Per-step | Worker critic: learns patrol execution; Manager critic: sees arena coverage |
| 4 | Agent completes patrol rotation | +0.15 | One-shot | Manager critic: evaluates strategic exploration value |
| 5 | Agent fails to patrol (no valid points) | −0.001 × dt | Per-step | Small penalty to discourage aimless wandering |
| **Detection & Engagement Rewards** | | | | |
| 6 | Agent detects player (first sight) | +0.10 | One-shot | Manager critic: values detection as strategic milestone |
| 7 | Agent approaches player (distance decreasing) | +0.005 × dt | Per-step | Worker critic: rewards closing distance; Manager critic: evaluates engagement strategy |
| 8 | Agent fails to approach player (distance NOT decreasing while chasing) | −0.005 | One-shot (per interval) | Worker critic: penalizes ineffective movement; Manager critic: sees global path inefficiency |
| 9 | Agent sees player but doesn't chase | −0.005 × dt | Per-step | Manager critic: penalizes strategic inaction |
| 10 | Agent chases player (moving while chasing) | +0.005 × dt | Per-step | Worker critic: rewards active pursuit behavior |
| 11 | Agent initiates chase after detection | +0.20 | One-shot | Manager critic: rewards correct strategic decision (detect→chase transition) |
| **Combat Rewards** | | | | |
| 12 | Agent attacks player (within range) | +0.30 | One-shot | Worker critic: rewards attack execution; Manager critic: rewards engagement outcome |
| 13 | Agent doesn't attack when in range | −0.02 × dt | Per-step | Worker critic: penalizes hesitation in attack range |
| 14 | Agent's attack misses | −0.05 | One-shot | Worker critic: penalizes poor attack timing/positioning |
| 15 | Agent gets hit by player | −0.20 | One-shot | Both critics: negative outcome from combat |
| **Terminal Rewards** | | | | |
| 16 | Agent kills player | +1.0 | Terminal | Maximum reward — both critics learn this is the ultimate goal |
| 17 | Agent dies | −1.0 | Terminal | Maximum penalty — both critics learn to avoid this |

**Key Differences from Zaky's Reward Policy:**

| Change | Old (Zaky) | New (HCA) | Reason |
|---|---|---|---|
| Wall punishment | −0.20/step (too harsh) | −0.02 × dt (proportional) | Old value dominated training, causing agents to freeze |
| No-movement | −0.010/step (too harsh) | −0.01/step | Reduced to avoid penalty-dominated learning |
| Patrol step | +0.005/step | +0.001 × dt | Reduced to prevent patrol-only exploitation |
| Episode skip on stuck | Iteration skipped | Removed | HCA's manager critic handles this via global position tracking |
| Approach player | +0.01 (flat) | +0.005 × dt | Time-scaled for consistency |
| Chase step | +0.010/step | +0.005 × dt | Reduced to balance with other rewards |

**Why HCA Helps with These Rewards:**
1. **Worker critic** (local view): Learns precise movement, obstacle avoidance, attack timing — *"how to execute"*
2. **Manager critic** (global view): Learns when to patrol vs. chase, strategic positioning, engagement decisions — *"what to do"*
3. **Combined advantage** `max(V_worker, V_manager)`: The policy gets the *best* value estimate, preventing the single-critic problem where one flat critic must learn everything simultaneously

---

### Phase 1: Unity C# — Manager Observation Sensor

Create a new sensor component that provides the manager-level (global) observations. This runs alongside the existing observation pipeline.

#### Task 1.1: Create `ManagerObservationSensor.cs`
- New ISensor implementation that provides the 16-dimensional manager observation
- Attach to the agent prefab as a second sensor alongside the existing RayPerception + vector obs
- Observations: world positions, behavior flags, quadrant, time pressure, engagement metrics
- **File:** `Assets/Script/RL Scripts/Normal Enemy/ManagerObservationSensor.cs`

#### Task 1.2: Modify `NormalEnemyAgents.cs`
- Add `[SerializeField] private ManagerObservationSensor managerSensor;` reference
- In `Initialize()`: configure the manager sensor with arena bounds
- In `OnEpisodeBegin()`: reset manager sensor state
- Feed runtime data (behavior state, engagement stats) to the manager sensor each step

#### Task 1.3: Update `NormalEnemyRewards.cs`
- Adjust reward values to match the new HCA reward policy table (Phase 0.3)
- No structural changes — the reward signal itself remains unified

---

### Phase 2: Python ML-Agents — HCA Trainer (Separate from PPO)

> **Design Decision:** HCA is implemented as a **completely separate trainer type** alongside PPO, not as a modification of PPO. This ensures the PPO baseline remains untouched for comparative testing. The only shared modification is `settings.py` (to register the new trainer type).

**File Structure (PPO untouched, HCA is new):**
```
ml-agents/ml-agents/mlagents/trainers/
├── ppo/                          # ◄── UNTOUCHED — your PPO baseline
│   ├── __init__.py
│   ├── trainer.py                #     Standard PPO trainer
│   └── optimizer_torch.py        #     Standard PPO optimizer (single critic)
│
├── hca/                          # ◄── NEW — your thesis contribution
│   ├── __init__.py               #     Package init
│   ├── trainer.py                #     HCA trainer (inherits from PPO trainer)
│   └── optimizer_torch.py        #     HCA optimizer (dual hierarchical critics)
│
├── settings.py                   # ◄── MODIFIED — add TrainerType.HCA + HCASettings
├── trainer_controller.py         # ◄── MODIFIED — register HCA in trainer factory
└── ...                           #     Everything else untouched
```

**How to switch between PPO and HCA:**
```yaml
# config/ppo/NormalEnemyCC.yaml — PPO baseline (unchanged)
behaviors:
  NormalEnemy:
    trainer_type: ppo        # ◄── uses original PPO trainer
    ...

# config/hca/NormalEnemyHCA.yaml — HCA experiment (new)
behaviors:
  NormalEnemy:
    trainer_type: hca        # ◄── uses new HCA trainer
    ...
```

#### Task 2.1: Register HCA trainer type in `settings.py`
Minimal changes to the existing settings file — only adds new entries, does not modify PPO logic.

```python
# In TrainerType enum — ADD one line:
class TrainerType(Enum):
    PPO = "ppo"
    SAC = "sac"
    HCA = "hca"    # ◄── NEW

# New HCA hyperparameter class — ADD after PPOSettings:
@attr.s(auto_attribs=True)
class HCAHyperparamSettings(OnPolicyHyperparamSettings):
    """HCA-specific hyperparameters (extends PPO's on-policy settings)."""
    beta: float = 5.0e-3
    epsilon: float = 0.2
    lambd: float = 0.95
    num_epoch: int = 3
    shared_critic: bool = False
    learning_rate_schedule: ScheduleType = ScheduleType.LINEAR
    beta_schedule: ScheduleType = ScheduleType.LINEAR
    epsilon_schedule: ScheduleType = ScheduleType.LINEAR
    # --- HCA-specific fields ---
    manager_hidden_units: int = 128       # Manager critic network width
    manager_num_layers: int = 2           # Manager critic network depth
    manager_learning_rate: float = 0.0003 # Separate LR for manager critic
    hca_value_method: str = "max"         # "max" (RLHC) or "softmax" (HCA)
    manager_obs_size: int = 16            # Size of manager observation vector
```

#### Task 2.2: Create `trainers/hca/__init__.py`
Empty package initializer.

#### Task 2.3: Create `trainers/hca/optimizer_torch.py` — Dual-Critic Optimizer
This is the **core algorithmic contribution**. Key differences from PPO's optimizer:

```python
class TorchHCAOptimizer(TorchOptimizer):
    """
    HCA optimizer with hierarchical critics (Cao & Lin, 2020).
    
    Differences from TorchPPOOptimizer:
    1. TWO ValueNetworks instead of one:
       - _worker_critic: uses worker observations (local, agent-centric)
       - _manager_critic: uses manager observations (global, arena-wide)
    2. Combined value estimation:
       V_combined = max(V_worker, V_manager)  # RLHC Eq. 16
    3. Separate value losses for each critic
    4. Shared actor (policy) — unchanged from PPO
    """
    
    def __init__(self, policy, trainer_settings):
        super().__init__(policy, trainer_settings)
        
        # Worker critic — same as PPO's standard critic (local obs)
        self._worker_critic = ValueNetwork(
            stream_names, worker_obs_specs, network_settings
        )
        
        # Manager critic — NEW, uses global observations
        manager_network_settings = NetworkSettings(
            hidden_units=hca_settings.manager_hidden_units,
            num_layers=hca_settings.manager_num_layers,
            normalize=network_settings.normalize,
        )
        self._manager_critic = ValueNetwork(
            stream_names, manager_obs_specs, manager_network_settings
        )
        
        # Separate optimizer includes both critics + shared actor
        params = (list(policy.actor.parameters()) 
                + list(self._worker_critic.parameters())
                + list(self._manager_critic.parameters()))
        self.optimizer = torch.optim.Adam(params, lr=learning_rate)
    
    def update(self, batch, num_sequences):
        # 1. Get worker values (from local observations)
        worker_values, _ = self._worker_critic.critic_pass(worker_obs)
        
        # 2. Get manager values (from global observations)  
        manager_values, _ = self._manager_critic.critic_pass(manager_obs)
        
        # 3. Hierarchical value combination (RLHC Eq. 16)
        combined_values = {}
        for name in self.stream_names:
            combined_values[name] = torch.max(
                worker_values[name], manager_values[name]
            )
        
        # 4. Compute advantages using combined values
        # (same PPO clipped objective, but with better value estimates)
        
        # 5. Value loss = worker_loss + manager_loss (both learn the same returns)
        worker_value_loss = trust_region_value_loss(worker_values, ...)
        manager_value_loss = trust_region_value_loss(manager_values, ...)
        value_loss = worker_value_loss + manager_value_loss
        
        # 6. Total loss (same structure as PPO)
        loss = policy_loss + 0.5 * value_loss - beta * entropy
        
        return update_stats  # includes worker/manager value losses separately
```

#### Task 2.4: Create `trainers/hca/trainer.py` — HCA Trainer
Inherits from PPO trainer, overrides only what's necessary:

```python
class HCATrainer(PPOTrainer):
    """
    HCA Trainer — inherits PPO's training loop but uses HCAOptimizer.
    
    Key overrides:
    1. create_optimizer() → returns TorchHCAOptimizer instead of TorchPPOOptimizer
    2. _process_trajectory() → separates worker/manager observations in buffer
    3. Additional TensorBoard stats for worker/manager value estimates
    """
    
    def create_optimizer(self):
        return TorchHCAOptimizer(self.policy, self.trainer_settings)
    
    def _process_trajectory(self, trajectory):
        # Split observations into worker (first sensor) and manager (second sensor)
        # Store both in the AgentBuffer for the optimizer to access
        super()._process_trajectory(trajectory)
```

#### Task 2.5: Register HCA in `trainer_controller.py` (or factory)
Add HCA to the trainer factory mapping:
```python
# In the trainer creation logic:
if trainer_type == TrainerType.HCA:
    from mlagents.trainers.hca.trainer import HCATrainer
    return HCATrainer(...)
```

#### Task 2.6: Create `config/hca/NormalEnemyHCA.yaml`
```yaml
behaviors:
  NormalEnemy:
    trainer_type: hca                    # ◄── Uses HCA trainer, not PPO
    max_steps: 1500000
    time_horizon: 512
    summary_freq: 10000
    threaded: false
    hyperparameters:
      batch_size: 2048
      buffer_size: 20480
      learning_rate: 0.0001             # Worker/actor learning rate
      beta: 0.01
      epsilon: 0.15
      lambd: 0.95
      num_epoch: 4
      learning_rate_schedule: constant
      # --- HCA-specific ---
      manager_hidden_units: 128         # Manager critic network width
      manager_num_layers: 2             # Manager critic depth
      manager_learning_rate: 0.0003     # Manager critic learning rate
      hca_value_method: max             # RLHC: max(V_worker, V_manager)
      manager_obs_size: 16              # Manager observation vector size
    network_settings:
      normalize: true
      hidden_units: 256                 # Worker critic/actor network width
      num_layers: 3                     # Worker network depth
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
```

#### Task 2.7: Keep PPO config unchanged for baseline
```yaml
# config/ppo/NormalEnemyCC.yaml — NO CHANGES
# This file stays exactly as-is for baseline comparison
```


---

### Phase 2.5: Curriculum Learning — Progressive Player Difficulty

> **Rationale:** The current training player only spawns at random positions and doesn't fight back meaningfully. This means the agent never learns defensive behavior, dodge timing, or how to fight a reactive opponent. Deploying such a model in the real game (where players attack, dodge, and reposition) creates a **distribution shift** that degrades agent behavior. Curriculum learning solves this by gradually increasing player difficulty, allowing the agent to first master navigation/basic combat before facing a fully reactive opponent.

**ML-Agents Built-in Curriculum Support:**  
ML-Agents natively supports curriculum learning via `environment_parameters` in the YAML config. The trainer sends a float parameter (`player_difficulty`) to Unity, which reads it via `Academy.Instance.EnvironmentParameters.GetWithDefault()` and adjusts player behavior accordingly.

#### Curriculum Stages

| Stage | `player_difficulty` | Player Behavior | Progression Threshold |
|---|---|---|---|
| **1 — Static** | 0.0 | Random spawn, no movement, auto-attacks only when agent is very close | Smoothed reward ≥ 100 |
| **2 — Passive Mobile** | 1.0 | Moves randomly within arena, auto-attacks at normal range | Smoothed reward ≥ 80 |
| **3 — Defensive** | 2.0 | Retreats from agent when too close, attacks, respawns on death | Smoothed reward ≥ 60 |
| **4 — Aggressive** | 3.0 | Actively approaches agent, attacks frequently, respawns on death | Terminal (no progression) |

#### Task 2.5.1: Create `RL_CurriculumPlayerController.cs`
- Reads `player_difficulty` from `Academy.Instance.EnvironmentParameters`
- **Stage 1 (0.0):** No movement, reduced attack range (attack only if agent is within 1.5m)
- **Stage 2 (1.0):** Random wander movement within arena, normal attack range
- **Stage 3 (2.0):** Kiting behavior — retreats when agent gets within a flee radius, attacks at range
- **Stage 4 (3.0):** Aggressive — moves toward nearest agent, attacks at full range, higher attack frequency
- Respawns at random position on death (stages 3-4)
- **File:** `Assets/Script/RL Scripts/Training/RL_CurriculumPlayerController.cs`

#### Task 2.5.2: Modify `NormalEnemyAgents.cs`
- In `OnEpisodeBegin()` or `OnActionReceived()`: read `player_difficulty` parameter
- Pass the value to the player spawner / player controller so spawned players adjust behavior
- No observation space changes — the curriculum changes the environment, not the agent's sensors

#### Task 2.5.3: Update YAML configs with curriculum settings
```yaml
# Added to both HCA and PPO configs:
environment_parameters:
  player_difficulty:
    curriculum:
      - name: Stage1_Static
        completion_criteria:
          measure: reward
          behavior: NormalEnemy
          min_lesson_length: 100
          threshold: 100
          signal_smoothing: true
        value: 0.0
      - name: Stage2_PassiveMobile
        completion_criteria:
          measure: reward
          behavior: NormalEnemy
          min_lesson_length: 100
          threshold: 80
          signal_smoothing: true
        value: 1.0
      - name: Stage3_Defensive
        completion_criteria:
          measure: reward
          behavior: NormalEnemy
          min_lesson_length: 100
          threshold: 60
          signal_smoothing: true
        value: 2.0
      - name: Stage4_Aggressive
        value: 3.0
```

#### Task 2.5.4: Training Plan with Curriculum
- **Without curriculum:** PPO baseline + HCA (static player only) — shows HCA architectural improvement
- **With curriculum:** HCA + curriculum — shows full solution with progressive difficulty
- Compare all three in TensorBoard for thesis results

---

### Phase 3: Training & Validation

#### Task 3.1: Fix Known Bugs First
- [ ] Fix agent respawn bug (agents stop respawning during training)
- [ ] Verify wall/obstacle collision penalties are applied correctly (already implemented in `OnCollisionEnter`/`OnCollisionStay`)
- [ ] Verify chase reward only given when distance actually decreases (already fixed in `ProcessChaseRewards`)

#### Task 3.2: Baseline PPO Training
- Train standard PPO with the **new reward policy** (Phase 0.3) for comparison
- Record: cumulative reward, convergence rate, episode length
- Target: 1.5M steps, checkpoint every 250K

#### Task 3.3: HCA Training
- Train HCA with the same reward policy
- Compare against PPO baseline
- Record same metrics + worker/manager value estimates separately

#### Task 3.4: Comparative Analysis
- TensorBoard comparison: PPO vs HCA
- Metrics to compare:
  - **Convergence speed** (steps to reach stable reward)
  - **Final reward** (mean reward over last 100 episodes)
  - **Value estimate stability** (how quickly value estimates converge)
  - **Episode length** (shorter = more decisive behavior)

---

### Phase 4: Deployment & Testing

#### Task 4.1: Export ONNX Model
- Export best HCA model as `.onnx`
- Place in `Assets/ML-Agents/` or `Assets/Model/`

#### Task 4.2: Deploy in Game Scene
- Attach trained model to NPC prefabs in `Reinforcement Learning Stage` scene
- Set `NormalEnemyAgent.TrainingActive = false`
- Set `BehaviorParameters` to inference mode with the `.onnx` model

#### Task 4.3: Environment Compatibility Check
- Verify observation spaces match between training and deployment scenes
- Verify all sensor components produce consistent data
- Test with different arena difficulties (Easy/Medium/Hard/Ultra)

---

### Phase 5: Evaluation

#### Task 5.1: Technical Evaluation
- Navigation success rate (% of episodes without navigation failures)
- Attack success rate (% of attacks that connect when in range)
- Convergence comparison: PPO vs HCA steps-to-convergence

#### Task 5.2: Expert Validation
- Game development expert review of agent behavior
- Qualitative assessment of navigation and combat quality

#### Task 5.3: GEQ Player Testing
- Conduct Game Experience Questionnaire with players
- Compare scores against Zaky's thesis baseline (84% confusion rate)
- Measure: immersion, competence, flow, challenge
- Target: significant reduction in "confusing enemy behavior" reports

---

## Curriculum Learning Diagram

```
 ┌──────────────────────────────────────────────────────────────────────────────────────────┐
 │                        CURRICULUM LEARNING — TRAINING STAGES                             │
 │                                                                                          │
 │  Controlled by: RL_CurriculumPlayerController.cs                                        │
 │  YAML param:    environment_parameters → difficulty (0 → 3)                             │
 │  Promotion:     Based on cumulative reward threshold per stage                          │
 └──────────────────────────────────────────────────────────────────────────────────────────┘

       STAGE 0 (Easy)              STAGE 1 (Medium)           STAGE 2 (Hard)            STAGE 3 (Ultra)
      difficulty = 0               difficulty = 1             difficulty = 2            difficulty = 3
 ┌─────────────────────┐     ┌─────────────────────┐    ┌─────────────────────┐   ┌─────────────────────┐
 │                     │     │                     │    │                     │   │                     │
 │   ┌─────────────┐   │     │   ┌─────────────┐   │    │   ┌─────────────┐   │   │   ┌─────────────┐   │
 │   │   STATIC    │   │     │   │  PASSIVE    │   │    │   │ DEFENSIVE   │   │   │   │ AGGRESSIVE  │   │
 │   │   PLAYER    │   │     │   │  MOBILE     │   │    │   │   PLAYER    │   │   │   │   PLAYER    │   │
 │   │             │   │     │   │   PLAYER    │   │    │   │             │   │   │   │             │   │
 │   │  • Stands   │   │     │   │  • Walks    │   │    │   │  • Moves    │   │   │   │  • Chases   │   │
 │   │    still    │   │     │   │    around   │   │    │   │    away     │   │   │   │    enemies  │   │
 │   │  • No AI    │   │     │   │  • Random   │   │    │   │  • Evades   │   │   │   │  • Attacks  │   │
 │   │  • Easy     │   │     │   │    wander   │   │    │   │    when     │   │   │   │    back     │   │
 │   │    target   │   │     │   │  • Moves    │   │    │   │    close    │   │   │   │  • Counter- │   │
 │   │             │   │     │   │    slowly   │   │    │   │  • Kites    │   │   │   │    attacks  │   │
 │   └─────────────┘   │     │   └─────────────┘   │    │   └─────────────┘   │   │   └─────────────┘   │
 │                     │     │                     │    │                     │   │                     │
 │  Agent learns:      │     │  Agent learns:      │    │  Agent learns:      │   │  Agent learns:      │
 │  • Basic movement   │     │  • Tracking a       │    │  • Pursuit of       │   │  • Combat timing    │
 │  • Approach target  │     │    moving target    │    │    evasive target   │   │  • Dodging attacks   │
 │  • Collision avoid  │     │  • Predict motion   │    │  • Group tactics    │   │  • Group assault     │
 │  • Spatial aware    │     │  • Path planning    │    │  • Flanking/cutoff  │   │  • Surround + kill   │
 │                     │     │                     │    │                     │   │                     │
 └──────────┬──────────┘     └──────────┬──────────┘    └──────────┬──────────┘   └─────────────────────┘
            │                          │                          │
            │  reward ≥ 100            │  reward ≥ 80             │  reward ≥ 60          TERMINAL
            │  ─────────────▶          │  ─────────────▶          │  ─────────────▶       (final policy)
            │  PROMOTE                 │  PROMOTE                 │  PROMOTE
            ▼                          ▼                          ▼

 ┌──────────────────────────────────────────────────────────────────────────────────────────┐
 │                         YAML CURRICULUM CONFIGURATION                                    │
 │                                                                                          │
 │  environment_parameters:                                                                │
 │    difficulty:                                                                           │
 │      curriculum:                                                                        │
 │        - name: Stage0_Static        ← Start here                                       │
 │          completion_criteria:                                                           │
 │            measure: reward                                                              │
 │            behavior: NormalEnemy                                                         │
 │            min_lesson_length: 100                                                       │
 │            threshold: 100.0                                                             │
 │          value: 0.0                                                                     │
 │                                                                                          │
 │        - name: Stage1_Passive       ← After avg reward ≥ 100                           │
 │          completion_criteria:                                                           │
 │            threshold: 80.0                                                              │
 │          value: 1.0                                                                     │
 │                                                                                          │
 │        - name: Stage2_Defensive     ← After avg reward ≥ 80                            │
 │          completion_criteria:                                                           │
 │            threshold: 60.0                                                              │
 │          value: 2.0                                                                     │
 │                                                                                          │
 │        - name: Stage3_Aggressive    ← Final stage                                      │
 │          value: 3.0                                                                     │
 │                                                                                          │
 └──────────────────────────────────────────────────────────────────────────────────────────┘

 ┌──────────────────────────────────────────────────────────────────────────────────────────┐
 │                     UNITY SIDE — RL_CurriculumPlayerController                           │
 │                                                                                          │
 │  OnEpisodeBegin() reads Academy.Instance.EnvironmentParameters.GetWithDefault(           │
 │      "difficulty", 0f)                                                                   │
 │                                                                                          │
 │  difficulty → (int) → selects player behavior:                                          │
 │                                                                                          │
 │      switch (currentDifficulty)                                                         │
 │      {                                                                                   │
 │          case 0: ExecuteStaticBehavior();     // Stand still                             │
 │          case 1: ExecutePassiveBehavior();    // Random wander                           │
 │          case 2: ExecuteDefensiveBehavior();  // Evade when enemies near                │
 │          case 3: ExecuteAggressiveBehavior(); // Attack enemies                          │
 │      }                                                                                   │
 │                                                                                          │
 └──────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Architecture Diagram — HCA in ML-Agents

```
                    ┌─────────────────────────┐
                    │   Unity Environment      │
                    │   (NormalEnemyAgent)      │
                    └─────────┬────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
     ┌────────────┐   ┌──────────────┐  ┌──────────┐
     │ Worker Obs │   │ Manager Obs  │  │  Reward   │
     │ (24-dim)   │   │ (16-dim)     │  │  Signal   │
     │ local view │   │ global view  │  │ (unified) │
     └─────┬──────┘   └──────┬───────┘  └─────┬────┘
           │                 │                 │
           ▼                 ▼                 │
     ┌──────────┐     ┌──────────────┐         │
     │  Worker  │     │   Manager    │         │
     │  Critic  │     │   Critic     │         │
     │  V_w(s)  │     │   V_m(s)     │         │
     └────┬─────┘     └──────┬───────┘         │
          │                  │                 │
          └──────┬───────────┘                 │
                 ▼                             │
         ┌───────────────┐                     │
         │ max(V_w, V_m) │ ◄── RLHC Eq.16     │
         │ = V_combined  │                     │
         └───────┬───────┘                     │
                 │                             │
                 ▼                             ▼
         ┌───────────────┐              ┌────────────┐
         │   Advantage   │◄─────────────│  Returns   │
         │   A = R - V   │              │  (from r)  │
         └───────┬───────┘              └────────────┘
                 │
                 ▼
         ┌───────────────┐
         │  Shared Actor  │
         │  π(a|s_worker) │
         │  PPO Clipped   │
         │  Policy Loss   │
         └───────────────┘
```

**Key insight:** The actor (policy) only sees worker observations and outputs actions. Both critics evaluate those actions but from different observation perspectives. The `max()` combination ensures the policy gets the most informative value signal — if the worker critic underestimates a state's value but the manager sees strategic potential, the manager's higher estimate is used.
