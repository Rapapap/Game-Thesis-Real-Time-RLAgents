# AGENTS.md

## Scope and priorities
- This repo is a Unity game project with an RL training stack for enemy NPCs; prioritize changes in `Assets/Script/RL Scripts/` and `config/` before touching gameplay scripts.
- The training behavior name is `NormalEnemy`; keep Unity `BehaviorParameters` and YAML behavior keys aligned (see `Assets/Level/RL Scenes/Training/Art/Models/RL Agents/RL_Humanoid.prefab`, `config/ppo/NormalEnemyCC.yaml`, `config/hca/NormalEnemyHCA.yaml`).
- Prefer the active RL scripts under `Assets/Script/RL Scripts/**`; treat top-level backups like `Assets/Script/NormalEnemyAgents Before Update.cs` and `Assets/Script/RL_EnemyController Before Update.cs` as historical snapshots.
- Active agent implementation is now in `Assets/Script/RL Scripts/Normal Enemy/NormalEnemyAgents.cs` (not the root level backup).

## Architecture map (what talks to what)
- `NormalEnemyAgent` (`Assets/Script/RL Scripts/Normal Enemy/NormalEnemyAgents.cs`) is the ML-Agents entrypoint: observations, action processing, rewards, episode lifecycle.
- `RL_EnemyController` (`Assets/Script/RL Scripts/Controller/RL_EnemyController.cs`) owns combat/HP/animation/death and calls back into the agent (`HandleDamage()`, `HandleEnemyDeath()`).
- `RL_TrainingManager` (`Assets/Script/RL Scripts/Training/RL_TrainingManager.cs`) orchestrates episode resets and counts active agents.
- `RL_TrainingEnemySpawner` and `RL_TrainingPlayerSpawner` manage multi-arena spawning; the player spawner configures curriculum bounds on spawned targets.
- `RL_CurriculumPlayerController` reads `Academy.Instance.EnvironmentParameters.GetWithDefault("player_difficulty", 0f)` and changes target behavior by stage.
- HCA adds a second sensor path: `ManagerObservationSensorComponent` -> `ManagerObservationSensor` (16 global features), while worker observations stay in `NormalEnemyAgent` (24 local features).
- `ManagerObservationSensorComponent` and `ManagerObservationSensor` (in `Assets/Script/RL Scripts/Normal Enemy/`) provide global state information for HCA critic networks.
- Observation Redesign & Fallback Mechanism:
  - Default Redesigned Mode (`useFallbackLegacyObservations = false`): Index [7] = normalized center distance (`distFromCenter`), Index [13] = continuous boundary proximity (`borderProximity`). Aligned with Cao & Lin (2020) RLHC paper.
  - Fallback Legacy Mode (`useFallbackLegacyObservations = true`): Toggleable via Inspector on `ManagerObservationSensorComponent`. Index [7] = health ratio, Index [13] = ordinal quadrant (`quadrant / 3f`).
  - HCA Value Combination: Default `hca_value_method: max` in `config/hca/NormalEnemyHCA.yaml` (RLHC Eq. 16). Toggleable to `softmax` in YAML if comparing combination methods.

## Training/inference workflow (project-specific)
- Environment/package baseline: Unity package `com.unity.ml-agents` is pinned in `Packages/manifest.json`; Python trainer entrypoint is `mlagents-learn` (also documented in `ml-agents/ml-agents/README.md`).
- Use the repo cheat sheet in `MLAgents COnda Script.txt` for canonical run patterns (`--resume`, `--force`, `--torch-device=cuda`).
- Typical runs:
  - PPO curriculum: `mlagents-learn config/ppo/NormalEnemyCC.yaml --run-id=PPO_Curriculum_v1`
  - HCA curriculum: `mlagents-learn config/hca/NormalEnemyHCA.yaml --run-id=HCA_Curriculum_v1`
  - Compare logs: `tensorboard --logdir=results`
- Build scenes include `Assets/Level/Scenes/Game Stage/Reinforcement Learning Stage.unity` (`ProjectSettings/EditorBuildSettings.asset`).

## Conventions that matter here
- `NormalEnemyAgent.TrainingActive` gates reset/death semantics across files; keep this contract intact when changing lifecycle logic (`NormalEnemyAgents.cs`, `RL_EnemyController.cs`, `RL_TrainingEnemySpawner.cs`).
- Agent reset path expects deactivated agents to be reactivated and `EndEpisode()`-driven; avoid destroy/deactivate behavior during training unless all reset callsites are updated.
- Reward tuning is centralized in `NormalEnemyRewards.cs`; use helper methods instead of scattering raw `AddReward()` calls. (`AttackMissedPunishment` is set to `-0.10f` to prevent combat policy collapse).
- Spawner logic depends on patrol point ordering by name (`A->B->C->D`) and per-arena parent transforms; preserve these assumptions when editing spawn code.
- `RL_EnemyController` supports both `RL_PlayerController` and `PlayerController`; keep dual-path damage handling to avoid training/runtime regressions.
- HCA training uses `ManagerObservationSensorComponent` for 16 global features; when modifying observations, ensure both worker (24 local) and manager (16 global) sensor paths remain synchronized.
- Enemy stat observations (health/attack/speed) are normalized in `NormalEnemyAgent.CollectObservations()` to enable policy differentiation across enemy types.
- Performance-critical components cache references (`RL_TrainingPlayerSpawner`, `RL_TrainingManager`) in `Initialize()` to avoid `FindFirstObjectByType` calls every episode.
- Collision tracking uses layered counters (`obstacleCollisionCount`) with enter/stay/exit callbacks for accurate obstacle punishment.

## Integration points and external code boundaries
- HCA trainer implementation lives in vendored Python source: `ml-agents/ml-agents/mlagents/trainers/hca/{trainer.py,optimizer_torch.py}`.
- HCA config expects manager observation sensor index `manager_obs_index: -1`; if sensor order changes in prefabs, update YAML accordingly.
- Unity prefabs (`Assets/Level/RL Scenes/Training/Art/Models/RL Agents/*.prefab`) define action/observation sizes and decision cadence (`DecisionPeriod: 5`); keep code and prefab settings synchronized.

## Safe-edit checklist for agents
- Confirm behavior name, observation dimensions, and action spec stay consistent across C# + prefab + YAML.
- If changing episode/death flow, test both training mode (`TrainingActive=true`) and game mode (`TrainingActive=false`).
- If editing spawners, verify bounds/collision checks still produce non-zero spawns per arena.
- To compare redesigned vs legacy observations, toggle `Use Fallback Legacy Observations` in `ManagerObservationSensorComponent` Inspector rather than altering vector sizes.
- No dedicated Unity test suite was found under `Assets/**/*Test*.cs`; validate changes by focused play-mode training smoke runs.
