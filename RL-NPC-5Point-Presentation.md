# RL NPC 5-Point Presentation (Block Diagram)

**Title:** Gambaran RL NPC Musuh `NormalEnemy`  
**Project:** Code Crusader: Anti-Virus Assault  
**Date:** April 7, 2026

---

## Slide 1 — Gambaran Alur Arsitektur HCA pada Game Ini

```text
+--------------------------------------------------+
| Unity Agent: NormalEnemyAgent                    |
| CollectObservations()                            |
+--------------------------+-----------------------+
                           |
          +----------------+----------------+
          |                                 |
          v                                 v
+---------------------------+   +-------------------------------+
| Worker Observation (24)   |   | Manager Observation (16)      |
| Local state NPC           |   | Global arena context          |
+-------------+-------------+   +---------------+---------------+
              |                                 |
              +---------------+-----------------+
                              |
                              v
                  +-----------------------------+
                  | Shared Actor (Policy pi)    |
                  | Output: action NPC          |
                  | (move, rotate, attack)      |
                  +-------------+---------------+
                                |
                                v
                    +--------------------------+
                    | Unity Env + Reward       |
                    | (NormalEnemyRewards.cs)  |
                    +-------------+------------+
                                  |
                 +----------------+----------------+
                 |                                 |
                 v                                 v
      +-------------------------+       +-------------------------+
      | Worker Critic V_worker  |       | Manager Critic V_manager|
      +-------------+-----------+       +-------------+-----------+
                    |                                 |
                    +---------------+-----------------+
                                    |
                                    v
                     +-------------------------------+
                     | HCA Value Combine             |
                     | method: softmax / max         |
                     | (sesuai config HCA)           |
                     +---------------+---------------+
                                     |
                                     v
                     +-------------------------------+
                     | Advantage + Policy Update     |
                     | HCATrainer / HCAOptimizer     |
                     +---------------+---------------+
                                     |
                                     v
                     +-------------------------------+
                     | Model ONNX -> Inference Scene |
                     +-------------------------------+
```

### Penjelasan
- Alur HCA dimulai dari satu agent Unity (`NormalEnemyAgent`) yang menghasilkan dua jenis observasi: lokal untuk worker dan global untuk manager.
- Keduanya dievaluasi oleh critic yang berbeda, tetapi aksi tetap diproduksi oleh **satu shared actor**.
- Nilai dari worker dan manager digabungkan (`softmax` atau `max` sesuai konfigurasi HCA), lalu dipakai untuk menghitung advantage.
- Hasil update policy digunakan untuk menghasilkan model `.onnx` yang dipakai saat inference di scene game.

---

## Slide 2 — Gambaran Letak Local Manager dan Global Manager

```text
+---------------------------------------------------------+
|                NormalEnemyAgent (Unity)                 |
+--------------------------+------------------------------+
                           |
          +----------------+----------------+
          |                                 |
          v                                 v
+---------------------------+   +-------------------------------+
| LOCAL PATH (WORKER)       |   | GLOBAL PATH (MANAGER)         |
| CollectObservations()     |   | ManagerObservationSensor.cs   |
| (24 fitur lokal)          |   | (16 fitur global)             |
+-------------+-------------+   +---------------+---------------+
              |                                 |
              v                                 v
+---------------------------+   +-------------------------------+
| Actor + Worker Critic     |   | Manager Critic                 |
+-------------+-------------+   +---------------+---------------+
              |                                 |
              +----------------+----------------+
                               |
                               v
                    +--------------------------+
                    | Policy Update (HCA/PPO)  |
                    +--------------------------+
```

### Penjelasan
- **Local/Worker path** berada di `CollectObservations()` pada `NormalEnemyAgent` dan fokus pada kondisi sekitar NPC secara langsung.
- **Global/Manager path** berasal dari `ManagerObservationSensor` dan memberi konteks arena secara lebih strategis.
- Arsitektur ini memisahkan sudut pandang evaluasi nilai tanpa memecah policy actor.
- Dampaknya, agent tetap punya aksi sederhana tetapi kualitas evaluasi keputusan menjadi lebih kaya.

---

## Slide 3 — Observation dari Local dan Global Manager

```text
+-----------------------------------------------------------+
| OBSERVATION SPACE: NormalEnemy                            |
+------------------------------+----------------------------+
| LOCAL / WORKER (24)          | GLOBAL / MANAGER (16)      |
+------------------------------+----------------------------+
| [1] Health                   | [2] Agent world position   |
| [4] Player local info        | [2] Player world position  |
| [2] Local velocity           | [2] Relative dir (world)   |
| [8] Obstacle raycasts        | [1] Global distance        |
| [3] Combat state flags       | [1] Health ratio           |
| [3] Patrol target info       | [5] Behavior one-hot       |
| [3] Enemy stats (HP/ATK/SPD) | [1] Arena quadrant         |
|                              | [1] Time pressure          |
|                              | [1] Engagement ratio       |
+------------------------------+----------------------------+
```

```text
Worker Obs (24) ---------------------> Actor + Worker Critic
Manager Obs (16) --------------------> Manager Critic
```

### Penjelasan
- Worker observation (24) berisi data eksekusi mikro: posisi relatif lokal, obstacle raycast, state combat, dan stat NPC.
- Manager observation (16) berisi data makro: posisi dunia, progres episode, state perilaku, dan engagement ratio.
- Pemisahan ini membuat worker belajar **cara bertindak**, sedangkan manager menilai **konteks strategi**.
- Karena behavior tetap `NormalEnemy`, sinkronisasi dimensi observasi C# dan trainer wajib dijaga.

---

## Slide 4 — Action dari NPC

```text
+----------------------------------------------+
| ACTION SPACE (Behavior: NormalEnemy)         |
+--------------------------+-------------------+
| Continuous Actions (3)   | Discrete Actions  |
+--------------------------+-------------------+
| [0] Forward/Backward     | [0] Attack 0/1    |
| [1] Strafe Left/Right    |                   |
| [2] Rotation (Yaw)       |                   |
+--------------------------+-------------------+
```

```text
Output Policy
    |
    v
+------------------------------+
| OnActionReceived()           |
| -> ProcessActions()          |
+--------------+---------------+
               |
               +--> Movement (Rigidbody)
               |
               +--> Attack check (range/cooldown)
                        |
                        v
                  RL_EnemyController.AgentAttack()
```

### Penjelasan
- Ruang aksi tetap kecil: 3 continuous (gerak/rotasi) + 1 discrete (attack), sehingga stabil untuk training.
- Policy mengeluarkan aksi setiap decision step dan diproses melalui `OnActionReceived()`.
- Eksekusi movement memakai Rigidbody, sementara serangan divalidasi oleh jarak dan cooldown sebelum `AgentAttack()` dipanggil.
- Dengan desain ini, HCA meningkatkan evaluasi nilai tanpa mengubah action space dasar.

---

## Slide 5 — Policy dan Reward Table

```text
+----------------------------------------------------------+
| POLICY CONFIGURATION                                     |
+----------------------+-----------------------------------+
| PPO Baseline         | config/ppo/NormalEnemyCC.yaml    |
| trainer_type: ppo    |                                   |
+----------------------+-----------------------------------+
| HCA Experiment       | config/hca/NormalEnemyHCA.yaml   |
| trainer_type: hca    | manager_hidden_units: 128        |
|                      | manager_num_layers: 2            |
|                      | manager_learning_rate: 0.0003    |
|                      | hca_value_method: softmax        |
|                      | manager_obs_index: -1            |
+----------------------+-----------------------------------+
```

```text
+----------------------------------------------------------+
| REWARD BLOCK (NormalEnemyRewards.cs)                     |
+----------------+----------------------+-------------------+
| Terminal +     | Kill player          | +1.00             |
| Terminal -     | Agent mati           | -1.00             |
| Major +        | Detect/Patrol/Chase  | +0.10/+0.15/+0.20|
| Major +        | Attack valid         | +0.30             |
| Major -        | Kena hit player      | -0.20             |
| Step +/-       | Movement/Obstacle/   | time-scaled (*dt) |
|                | Approach/Attack miss |                   |
+----------------+----------------------+-------------------+
```

### Penjelasan
- PPO dipakai sebagai baseline, sedangkan HCA menambahkan konfigurasi khusus manager critic pada behavior yang sama (`NormalEnemy`).
- Reward tetap terpusat di `NormalEnemyRewards.cs` agar tuning konsisten dan tidak tersebar di banyak script.
- Struktur reward menggabungkan terminal reward, event reward, dan step-based reward (time-scaled) untuk membentuk perilaku bertahap.
- Inti perbandingan: PPO memakai satu jalur nilai, HCA memakai dua jalur nilai yang digabung untuk update policy yang lebih stabil.