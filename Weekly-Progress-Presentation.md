# Weekly Progress Report — Thesis Presentation
**Title:** Penerapan Hierarchical Reinforcement Learning pada NPC Musuh di Game 3D Action Hack and Slash  
**Student:** Rava Radithya Razan (10122108)  
**Game:** "Code Crusader: Anti-Virus Assault"  
**Date:** March 5, 2026

---

## Slide 1 — Research Recap

- Continuing Zaky's thesis (PPO-based enemy NPC RL, scored 85–95)
- Problem: 84% players found enemy behavior confusing, 70% navigation failures, 60% attack failures
- **Solution:** Implement **Hierarchical Critic Assignment (HCA)** (Cao & Lin, 2020) to improve agent behavior through dual-level value estimation

---

## Slide 2 — What Has Been Done ✅

### A. HCA Architecture — Fully Designed & Implemented

1. **Hierarchical Observation Design (Completed)**
   - **Worker-level observations (24-dim, local):** agent HP, player direction/distance, velocity, obstacle raycasts (8 directions), combat state flags, patrol target, **enemy stats (maxHP, attack, speed)**
   - **Manager-level observations (16-dim, global):** world positions (agent & player), arena quadrant, behavior flags (idle/patrol/chase/attack/flee), time pressure, engagement success ratio

2. **Enemy Stat Observations — Added (Completed)**
   - Added 3 normalized stat observations to worker vector (21→24): `maxHP/200`, `attack/10`, `speed/10`
   - Enables a **single neural network** to learn type-conditioned behavior for Creep, Humanoid, and Bull agents
   - All existing `.onnx` models invalidated — requires full retraining

3. **HCA Reward Policy — Redesigned (Completed)**
   - Revised all 17 reward signals from Zaky's flat policy
   - Reduced overly harsh penalties (wall: −0.20 → −0.02×dt, no-movement: −0.010 → −0.01/step)
   - Added time-scaled (×dt) rewards for consistency
   - Added collision punishment for walls/obstacles/other agents
   - Fixed chase reward: now only given when distance to player *actually* decreases

4. **Unity C# Scripts — Created (Completed)**
   | Script | Purpose |
   |---|---|
   | `ManagerObservationSensor.cs` | ISensor implementation providing 16-dim global observations |
   | `ManagerObservationSensorComponent.cs` | SensorComponent to attach manager sensor to agent |
   | `NormalEnemyAgents.cs` | Main agent script (24-dim worker obs with enemy stats) |
   | `NormalEnemyRewards.cs` | Reward logic (updated to HCA policy) |
   | `NormalEnemyActions.cs` | Action handling (shared actor, unchanged action space) |
   | `NormalEnemyStates.cs` | State management |
   | `RL_CurriculumPlayerController.cs` | **NEW** — Curriculum-aware player with 4 difficulty stages |

5. **Python ML-Agents HCA Trainer — Created (Completed)**
   - Created **separate** `trainers/hca/` package (PPO baseline untouched)
   - `optimizer_torch.py` — Dual-critic optimizer (worker + manager ValueNetworks, combined via `max(V_w, V_m)`)
   - `trainer.py` — HCA trainer inheriting from PPO trainer
   - Registered `TrainerType.HCA` in `settings.py`
   - Registered HCA in trainer factory (`trainer_controller.py`)

6. **Training Configurations — Created (Completed)**
   | Config File | Trainer | Curriculum |
   |---|---|---|
   | `config/ppo/NormalEnemyCC.yaml` | PPO | ✅ With curriculum |
   | `config/ppo/NormalEnemyCC_NoCurriculum.yaml` | PPO | ❌ Without curriculum |
   | `config/hca/NormalEnemyHCA.yaml` | HCA | ✅ With curriculum |
   | `config/hca/NormalEnemyHCA_NoCurriculum.yaml` | HCA | ❌ Without curriculum |

7. **Curriculum Learning — Implemented (Completed)**
   - `RL_CurriculumPlayerController.cs` reads `player_difficulty` from ML-Agents Academy
   - 4 progressive stages: Static → Passive Mobile → Defensive → Aggressive
   - Automatic progression based on smoothed reward thresholds
   - Uses ML-Agents built-in `environment_parameters` curriculum support

8. **Convergence / Early Stopping — Implemented (Completed)**
   - Modified ML-Agents trainer source: `settings.py`, `trainer.py`, `rl_trainer.py`
   - Parameters: convergence_threshold, convergence_window, convergence_min_steps, convergence_patience

---

## Slide 3 — Training Results Summary

### Previous PPO Baseline Runs (Zaky's Design)

| Run | Mean Reward | Steps | Duration |
|---|---|---|---|
| NormalEnemyRun1 | 2.97 | 10,000,000 | 4.28 hr |
| NormalEnemyRun2 | 49.32 | 12,730,000 | 4.10 hr |
| NormalEnemyRunTest2 | 33.88 | 15,000,000 | 13.08 hr |
| NormalEnemyRunTest | 44.86 | 5,570,000 | 12.72 hr |
| rava-train-test | **103.41** | 500,000 | 1.75 hr |

### Latest HCA Training Run (New Architecture)

| Run | Smoothed Reward | Raw Value | Steps | Duration |
|---|---|---|---|---|
| **HCA-experiment** | **134.79** | **104.23** | **1,500,000** | **1.345 hr** |

### Key Comparison
| Metric | Best PPO (rava-train-test) | HCA-experiment |
|---|---|---|
| Mean Reward | 103.41 | **134.79** (smoothed) |
| Steps Used | 500,000 | 1,500,000 |
| Training Time | 1.75 hr | 1.345 hr |

**Key Finding:** HCA achieves **~30% higher smoothed reward** (134.79 vs 103.41) with the hierarchical dual-critic architecture, demonstrating improved value estimation from separating local and global observations.

> **Note:** Results above were obtained with 21-dim worker observations. Next training runs will use 24-dim (with enemy stats) and curriculum learning — requires full retraining.

---

## Slide 4 — Challenges & Obstacles Encountered 🚧

1. **Agent Respawn Bug**
   - During training, agents stop respawning after some time while training continues
   - Related to episode management in `RL_TrainingManager` / `RL_TrainingEnemySpawner`
   - Partially addressed but still observed

2. **Tensor Size Mismatch Errors**
   - `RuntimeError: The size of tensor a (16) must match the size of tensor b (21)` — caused by manager (16-dim) and worker (21-dim) observation tensors being processed on different paths
   - Required careful separation of observation flows in the HCA optimizer

3. **Device Mismatch (CUDA vs CPU)**
   - `RuntimeError: Expected all tensors to be on the same device, but found at least two devices, cuda:0 and cpu!`
   - Required explicit `.to(device)` calls on manager critic tensors

4. **ML-Agents Trainer Registration Issues**
   - `TrainerConfigError: Invalid trainer type hca was found` — HCA needed to be registered in multiple places (settings.py, trainer_controller.py, factory logic)
   - Multiple Unity console errors (999+) during initial integration

5. **Training/Deployment Scene Mismatch**
   - Training scene (`Training Normal Enemy`) differs from game scene (`Reinforcement Learning Stage`)
   - NavMesh disabled in training (Rigidbody-based movement), but present in deployment
   - Potential policy transfer issues being monitored

6. **Kinematic Rigidbody on Training Player**
   - `RL_CurriculumPlayerController` tried to set `linearVelocity` on kinematic Rigidbody
   - Fixed: added `isKinematic` check, using `MovePosition` for kinematic bodies

7. **Memory Leak Warning**
   - Unity persistent allocator leak detected during training sessions
   - Non-blocking but concerning for long training runs

---

## Slide 5 — HCA Architecture Diagram

```
     Unity Environment (NormalEnemyAgent)
              │
     ┌────────┼────────┐
     ▼        ▼        ▼
  Worker   Manager   Reward
  Obs(24)  Obs(16)   Signal
     │        │        │
     ▼        ▼        │
  Worker   Manager     │
  Critic   Critic      │
  V_w(s)   V_m(s)      │
     │        │        │
     └───┬────┘        │
         ▼             ▼
   max(V_w, V_m)    Returns
   = V_combined  ───► Advantage
         │               │
         ▼               │
    Shared Actor ◄───────┘
    π(a|s_worker)
    PPO Clipped Loss
```

**Key Insight:** Single shared actor, dual critics with different observation perspectives. Manager sees strategic context, worker sees local execution details. Enemy stat observations (maxHP, attack, speed) are in worker obs so the policy can differentiate Creep/Humanoid/Bull agents.

---

## Slide 6 — Curriculum Learning 📚

### Progressive Player Difficulty (4 Stages)

| Stage | Player Behavior | Progression |
|---|---|---|
| **1 — Static** | No movement, close-range attack only | Reward ≥ 100 |
| **2 — Passive Mobile** | Random wandering, normal attacks | Reward ≥ 80 |
| **3 — Defensive** | Retreats from agents, attacks at range | Reward ≥ 60 |
| **4 — Aggressive** | Chases agents, high attack frequency | Terminal |

- Implemented via `RL_CurriculumPlayerController.cs` + ML-Agents `environment_parameters`
- Separate YAML configs for with/without curriculum for controlled comparison

---

## Slide 7 — Future Plan 📋

### Immediate Next Steps
1. **Retrain with 24-dim Observations** — All models invalidated by enemy stat obs change
2. **Run 4-Way Comparison** — PPO vs HCA, with vs without curriculum
3. **Fix Agent Respawn Bug** — Debug `RL_TrainingManager` for consistent respawning

### Training Plan
| Run | Config | Purpose |
|---|---|---|
| `PPO-NoCurriculum` | `NormalEnemyCC_NoCurriculum.yaml` | PPO baseline (static player) |
| `HCA-NoCurriculum` | `NormalEnemyHCA_NoCurriculum.yaml` | HCA improvement (static player) |
| `PPO-Curriculum` | `NormalEnemyCC.yaml` | PPO + curriculum |
| `HCA-Curriculum` | `NormalEnemyHCA.yaml` | HCA + curriculum (full solution) |

### Comparative Analysis Phase
4. **TensorBoard Side-by-Side Comparison** — All 4 configs on:
   - Convergence speed (steps to stable reward)
   - Final mean reward (last 100 episodes)
   - Episode length (shorter = more decisive behavior)
   - Curriculum stage progression speed
5. **Worker vs Manager Value Analysis** — Visualize how each critic contributes

### Deployment & Testing Phase
6. **Export Best ONNX Model** — Export best trained model for inference
7. **Deploy in Game Scene** — Attach to NPC prefabs in `Reinforcement Learning Stage`
8. **Environment Compatibility Testing** — Verify observation consistency

### Evaluation Phase
9. **Technical Evaluation** — Navigation success rate, attack accuracy, convergence metrics
10. **Expert Validation** — Game development professional review
11. **GEQ Player Testing** — Game Experience Questionnaire survey
    - Target: significant reduction from Zaky's 84% "confusing behavior" rate
    - Metrics: immersion, competence, flow, challenge

---

## Slide 8 — Timeline Estimate

| Week | Activity |
|---|---|
| This Week | Retrain all 4 configs (PPO/HCA × curriculum/no-curriculum) |
| Week +1 | TensorBoard comparative analysis, fix remaining bugs |
| Week +2 | Export best model, deploy in game, compatibility testing |
| Week +3 | Behavior QA, expert validation |
| Week +4 | GEQ player testing |
| Week +5 | Write results chapter, finalize thesis |

---

## Summary

| Aspect | Status |
|---|---|
| HCA Architecture Design | ✅ Complete |
| Unity C# Implementation | ✅ Complete (24-dim worker obs) |
| Python HCA Trainer | ✅ Complete |
| Enemy Stat Observations (21→24) | ✅ Complete |
| Curriculum Learning | ✅ Complete (4 stages) |
| Convergence / Early Stopping | ✅ Complete |
| Training Configs (with/without curriculum) | ✅ Complete (4 YAML files) |
| Initial HCA Training (21-dim) | ✅ Complete (134.79 smoothed @ 1.5M steps) |
| Retraining with 24-dim + Curriculum | 🔄 Next |
| 4-Way Comparison (PPO/HCA × Curriculum) | 📋 Planned |
| Bug Fixes (respawn) | 🔄 In Progress |
| Deployment & Testing | 📋 Planned |
| Player Evaluation (GEQ) | 📋 Planned |

