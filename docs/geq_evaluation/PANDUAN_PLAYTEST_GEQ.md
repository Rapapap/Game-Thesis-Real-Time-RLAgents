# PANDUAN PRAKTIS PLAYTEST & SURVEY GEQ (PILIHAN 1)

Panduan ini memandu Anda melakukan **Build 2 Versi Game** dan mendistribusikannya ke responden untuk pengujian *Game Experience Questionnaire (GEQ)*.

---

## 🛠️ LANGKAH 1: BUILD GAME MODE A (MODEL PPO BASELINE)

1. **Pasang Model PPO ke Prefab:**
   * Di Unity Project Window, buka folder: `Assets/Level/RL Scenes/Training/Art/Models/RL Agents/`
   * Buka ketiga prefab musuh:
     * `RL_Humanoid.prefab`
     * `RL_Creep.prefab`
     * `RL_Bull.prefab`
   * Pada komponen **`Behavior Parameters`**, pastikan:
     * **Model:** `NormalEnemy_PPO_50M` (`Assets/Models/NormalEnemy_PPO_50M.onnx`)
     * **Behavior Type:** `Inference Only` atau `Default`
2. **Lakukan Build di Unity:**
   * Klik menu bar atas: **File $\rightarrow$ Build Settings...**
   * Pastikan scene gameplay (misal: `Reinforcement Learning Stage` / Level Gameplay) tercentang di daftar **Scenes In Build**.
   * Klik tombol **Build**.
   * Buat folder baru bernama: `Builds/Game_Mode_A/` dan simpan file build sebagai `Game_Mode_A.exe`.

---

## 🛠️ LANGKAH 2: BUILD GAME MODE B (MODEL HCA MAX - RLHC)

1. **Pasang Model HCA Max ke Prefab:**
   * Buka kembali ketiga prefab musuh:
     * `RL_Humanoid.prefab`
     * `RL_Creep.prefab`
     * `RL_Bull.prefab`
   * Pada komponen **`Behavior Parameters`**, ganti modelnya menjadi:
     * **Model:** `NormalEnemy_HCA_Max_50M` (`Assets/Models/NormalEnemy_HCA_Max_50M.onnx`)
     * **Behavior Type:** `Inference Only` atau `Default`
2. **Lakukan Build di Unity:**
   * Klik menu: **File $\rightarrow$ Build Settings $\rightarrow$ Build**.
   * Buat folder baru bernama: `Builds/Game_Mode_B/` dan simpan file build sebagai `Game_Mode_B.exe`.

---

## 📦 LANGKAH 3: PENGEMASAN & DISTRIBUSI KE RESPONDEN

1. Buat folder bernama `Playtest_Skripsi_Rava/` yang berisi:
   * 📁 Folder `Game_Mode_A/` (berisi `Game_Mode_A.exe` dan folder data terkait)
   * 📁 Folder `Game_Mode_B/` (berisi `Game_Mode_B.exe` dan folder data terkait)
   * 📄 File teks `PETUNJUK_BERMAIN.txt`
2. Kompres folder tersebut menjadi `Playtest_Skripsi_Rava.zip` dan upload ke Google Drive Anda.

---

## 📝 CONTOH TEMPLATE TEKS INSTRUKSI UNTUK RESPONDEN (WA / EMAIL)

```
Halo teman-teman! 👋
Saya sedang melakukan penelitian tugas akhir (skripsi) mengenai pengembangan kecerdasan buatan (AI) musuh pada game 3D Action.

Saya memohon kesediaan teman-teman untuk mencoba memainkan game ini (±5–10 menit) dan memberikan penilaian melalui kuesioner.

🕹️ TAHAPAN PLAYTEST:
1. Download dan ekstrak file game berikut: [LINK GOOGLE DRIVE ZIP ANDA]
2. Buka folder "Game_Mode_A" dan jalankan "Game_Mode_A.exe".
   - Mainkan pertarungan selama 3–5 menit (coba serang, hindari, dan amati perilaku musuh).
3. Setelah selesai Mode A, buka link kuesioner dan isi Bagian A: [LINK GOOGLE FORM ANDA]
4. Buka folder "Game_Mode_B" dan jalankan "Game_Mode_B.exe".
   - Mainkan pertarungan selama 3–5 menit.
5. Lanjutkan mengisi kuesioner Bagian B hingga selesai.

Kontrol Permainan:
- Gerak: W, A, S, D
- Serang: Klik Kiri Mouse / Space
- Kamera: Gerakan Mouse

Terima kasih banyak atas bantuan dan waktu yang teman-teman luangkan! 🙏✨
```

---

## 📊 LANGKAH 4: PENGOLAHAN DATA DENGAN PYTHON SCRIPT

Setelah responden mengisi Google Form (minimal 20–30 responden):
1. Download data respon dari Google Forms sebagai format `.csv` (misalnya simpan di `docs/geq_evaluation/responses.csv`).
2. Jalankan skrip analisis:
   ```bash
   python scripts/calculate_geq_scores.py
   ```
3. Skrip akan otomatis:
   * Menghitung nilai mean, standard deviation, persentase, dan predikat resmi untuk 7 dimensi GEQ.
   * Menghasilkan grafik komparasi radar chart: `presentation_charts/journal_style/fig8_geq_radar_chart.png`.
   * Menghasilkan laporan evaluasi tabel lengkap di `docs/geq_evaluation/GEQ_EVALUATION_REPORT.md`.
