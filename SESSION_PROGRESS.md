# SESSION_PROGRESS.md
> Terakhir diperbarui: 2026-08-14 (JST)

Dokumen ini mencatat semua perubahan, keputusan, dan status pekerjaan dari sesi riset terakhir.
Gunakan file ini sebagai titik awal saat melanjutkan pekerjaan di perangkat atau sesi lain.

---

## 1. Status Training Terbaru (10 Juta Steps)

Tiga run selesai dan telah dibandingkan via TensorBoard:

| Run ID (di `results/`) | Algoritma | Cumulative Reward (Smoothed) | Entropy Akhir | Waktu Training |
|---|---|---|---|---|
| `PPO_NoCurriculum_v2` | PPO (No Curriculum) | **1.7995** | 1.3154 (Meluruh) | 4.416 jam |
| `HCA_Softmax_v2` | HCA Softmax | 1.7122 | 2.3631 (Stabil Tinggi) | **3.772 jam** |
| `HCA_Max_v2` | HCA Max | 1.6420 | 2.3754 (Stabil Tinggi) | 5.651 jam |

**Kesimpulan dari training:**
- **PPO** menghasilkan reward tertinggi secara raw, namun entropy-nya meluruh => perilaku musuh menjadi deterministik/monoton.
- **HCA Softmax** menjadi kandidat terbaik: reward kompetitif, entropy tetap tinggi (perilaku musuh lebih bervariasi/tidak terprediksi), dan waktu training paling efisien.
- **HCA Softmax** dipilih sebagai model utama untuk evaluasi gameplay.

---

## 2. Model Yang Digunakan untuk Evaluasi

File model hasil training telah disalin ke folder Unity Assets dan di-assign ke prefab musuh.

### Lokasi File Model
```
Assets/Models/
  - NormalEnemy_PPO_v2.onnx
  - NormalEnemy_HCA_Softmax_v2.onnx   <-- Model aktif saat ini
  - NormalEnemy_HCA_Max_v2.onnx
```

### Prefab yang Sudah Diassign Model
Ketiga prefab musuh berikut sudah di-set menggunakan `NormalEnemy_HCA_Softmax_v2.onnx`
via komponen `BehaviorParameters`:
- `Assets/Level/RL Scenes/Training/Art/Models/RL Agents/RL_Humanoid.prefab`
- `Assets/Level/RL Scenes/Training/Art/Models/RL Agents/RL_Creep.prefab`
- `Assets/Level/RL Scenes/Training/Art/Models/RL Agents/RL_Bull.prefab`

> **Catatan:** Untuk mengganti model (misal ke PPO), drag file `.onnx` yang diinginkan
> dari `Assets/Models/` ke slot `Model` di komponen `BehaviorParameters` masing-masing prefab.

---

## 3. Sistem Evaluasi Otomatis (RL_EvalLogger)

### Komponen Yang Aktif di Scene
- **Scene evaluasi:** `Assets/Level/Scenes/Game Stage/Reinforcement Learning Stage.unity`
- Komponen `RL_EvalLogger` sudah dipasang di GameObject `RL_TrainingManager` dalam scene tersebut.
- Konfigurasi saat ini:
  - `Run Label` = `"HCA_Softmax_v2"` (ganti sesuai model yang diuji)
  - `Target Episodes` = `50`
  - `Output Folder` = `EvalResults/`

### Cara Menjalankan Evaluasi
1. Buka scene `Reinforcement Learning Stage.unity` di Unity.
2. Tekan **Play** di Unity Editor ATAU build game dan jalankan `.exe`.
3. Mainkan game hingga 50 ronde selesai.
4. File CSV akan tersimpan otomatis di: `<root project>/EvalResults/eval_HCA_Softmax_v2_<timestamp>.csv`

### Metrik yang Dicatat
- `DamageDealt` -- Total damage yang diberikan musuh ke player per episode
- `EnemyWon` -- 1 jika musuh menang (player mati), 0 jika musuh kalah (musuh mati)
- Summary otomatis dicetak di Unity Console setelah 50 episode

---

## 4. Perubahan Kode Sesi Ini

Semua perubahan kode di bawah ini sudah tersimpan dan dikompilasi di Unity.

### 4.1 `TrainingActive` -- Sekarang Otomatis & Dinamis
**File:** `Assets/Script/RL Scripts/Normal Enemy/NormalEnemyAgents.cs`

**Perubahan:** `TrainingActive` bukan lagi field statis manual (`= false`).
Sekarang menjadi **property dinamis** yang mengecek status ML-Agents Communicator secara real-time:

```csharp
// Sebelum (manual):
public static bool TrainingActive = false;

// Sesudah (otomatis):
public static bool TrainingActive => Unity.MLAgents.Academy.IsInitialized
                                     && Unity.MLAgents.Academy.Instance.IsCommunicatorOn;
```

**Dampak:**
- Saat training (`mlagents-learn` aktif) => `TrainingActive = true` secara otomatis.
- Saat gameplay/build (tanpa `mlagents-learn`) => `TrainingActive = false` secara otomatis.
- **Tidak perlu mengubah kode atau setting apa pun saat berpindah antara mode training dan mode gameplay.**

---

### 4.2 Bug Fix -- Musuh Tidak Lagi Respawn Saat Gameplay

**Masalah:** Saat memainkan `Reinforcement Learning Stage` (mode gameplay), musuh terkadang tiba-tiba respawn, ter-heal HP ke 100%, atau teleportasi ke posisi acak.

**Files yang Diubah:**

**`NormalEnemyAgents.cs`**
```csharp
// OnEpisodeBegin -- reset hanya saat training:
// Sebelum: if (TrainingActive || enableEpisodeReset)
// Sesudah: if (TrainingActive)

// HandleEnemyDeath -- EndEpisode hanya saat training:
// Sebelum: if (TrainingActive || enableEpisodeReset)
// Sesudah: if (TrainingActive)
```

**`RL_TrainingManager.cs`**
```csharp
// Start() -- sesi training hanya diinisialisasi saat training:
// Sebelum: if (autoStartTraining)
// Sesudah: if (autoStartTraining && NormalEnemyAgent.TrainingActive)

// HandleEnemyDeath() -- guard baru ditambahkan:
public void HandleEnemyDeath()
{
    if (!NormalEnemyAgent.TrainingActive) return; // Guard baru
    if (--activeEnemiesCount <= 0)
        StartCoroutine(ResetEpisodeCoroutine());
}
```

---

### 4.3 Integrasi `RL_EvalEvents` ke Gameplay Scripts

Event bus evaluasi kini terhubung ke alur gameplay sungguhan.

**`RL_EnemyController.cs`** -- Catat Damage ke Player
```csharp
// Di TryDamagePlayer(), setelah damage berhasil diberikan ke rlPlayer dan player:
RL_EvalEvents.RaiseEnemyDealtDamage(dmg);
```

**`NormalEnemyAgents.cs`** -- Catat Hasil Episode saat Musuh Mati
```csharp
// Di HandleEnemyDeath():
RL_EvalEvents.RaiseEpisodeResult(false);  // false = musuh kalah
```

**`PlayerController.cs`** (Game Script) -- Catat Hasil Episode saat Player Mati
```csharp
// Di OnDeath():
RL_EvalEvents.RaiseEpisodeResult(true);  // true = musuh menang
```

**`RL_PlayerController.cs`** (Training Script) -- Catat Hasil Episode saat Player Mati
```csharp
// Di Die():
RL_EvalEvents.RaiseEpisodeResult(true);  // true = musuh menang
```

---

## 5. Build Settings

Scene `Reinforcement Learning Stage.unity` sudah diaktifkan di Build Settings (Build Index 8).

> Training scene (`Training Normal Enemy.unity`) **tidak** dimasukkan ke build game.

---

## 6. Langkah Selanjutnya

- [ ] **Bake Lighting** di scene `Reinforcement Learning Stage.unity` agar tidak gelap
  - Unity Editor -> Window -> Rendering -> Lighting -> Generate Lighting
- [ ] **Lakukan evaluasi 50 episode** dengan model `HCA_Softmax_v2` (human vs RL agent)
- [ ] **Lakukan evaluasi 50 episode** dengan model `PPO_v2` untuk perbandingan
  - Ganti `Run Label` di `RL_EvalLogger` -> `"PPO_v2"`
  - Ganti model di ketiga prefab musuh -> `NormalEnemy_PPO_v2.onnx`
- [ ] *(Opsional)* Evaluasi juga dengan model `HCA_Max_v2` untuk kelengkapan data
- [ ] **Analisis file CSV** dari folder `EvalResults/` untuk bab Pembahasan skripsi
  - Bandingkan Win-Rate dan Average Damage antara PPO vs HCA Softmax vs HCA Max
- [ ] **Build game** setelah evaluasi awal selesai
  - File -> Build Settings -> Build and Run
  - Output ke folder `Builds/`
