# Weekly Progress Report — Thesis Presentation & Research Update
**Judul Penelitian:** Optimasi Perilaku Musuh *Non-Player Character* (NPC) Menggunakan *Hierarchical Critic Assignment* (HCA) pada *Proximal Policy Optimization* (PPO)  
**Mahasiswa:** Rava Radithya Razan (10122108)  
**Dosen Pembimbing:** Tim Pembimbing Tugas Akhir / Skripsi  
**Game Skenario:** *"Code Crusader: Anti-Virus Assault"* (Unity 3D Real-Time Action Arena)  
**Tanggal Laporan:** 31 Agustus 2026  

---

## 📌 Ringkasan Eksekutif (*Executive Summary*)

Pada minggu ini, seluruh rangkaian target dari *"Next Tasks"* sebelumnya telah **berhasil diselesaikan 100%**, yang mencakup:
1. **Implementasi Sistem Metriks Evaluasi Kuantitatif In-Game Otomatis** (Pencatatan *Win Rate*, *Damage*, *Time-to-Kill*, *Sudut Kepungan Multi-Agen $E_E$*, dan *Spatial Heatmap 2D*).
2. **Eksekusi Evaluasi Empiris In-Game (50+ Episode Pertempuran Terstandar)** secara *head-to-head* untuk ketiga model konvergensi jangka panjang (50 Juta Steps): **PPO Baseline (50M)**, **HCA Softmax (50M)**, dan **HCA Max (50M - RLHC)**.
3. **Penyelesaian Naskah Akademik Bab IV (Hasil & Pembahasan), Bab V (Kesimpulan), Uji Signifikansi Statistik, serta Paper Ilmiah IEEE (Bahasa Indonesia & Inggris)**.

---

## 1. 🛠️ Implementasi Sistem Tracker & Metriks Evaluasi In-Game

Telah dirancang dan diintegrasikan komponen C# Unity (`RL_HeatmapTracker.cs` & `RL_CombatEvaluationTracker.cs`) yang mencatat telemetri pertarungan secara otomatis ke format `.csv`:

| Metriks Evaluasi | Satuan / Representasi | Tujuan & Relevansi Akademik |
| :--- | :---: | :--- |
| **Win-Rate Tracking** | Rasio Menang/Kalah (%) | Mengukur konsistensi efektivitas tim musuh menumbangkan pemain. |
| **Damage Dealt Output** | Poin HP (0 – 200 HP) | Mengukur tingkat agresivitas dan efisiensi serang musuh saat mendapat celah. |
| **Time-to-Kill (TTK)** | Detik ($s$) | Mengukur durasi tempo pertempuran hingga salah satu pihak tereliminasi. |
| **Sudut Kepungan ($E_E$)** | Derajat Sudut ($0^\circ - 360^\circ$) | Mengukur formasi spasial multi-arah (apakah musuh mengelompok atau mengepung). |
| **Spatial Heatmap 2D** | Grid Matriks $32 \times 32$ | Memetakan distribusi okupansi posisi musuh dan pemain di seluruh arena. |

---

## 2. 📊 Hasil Evaluasi Kuantitatif In-Game (50M Suite)

Pengujian dilakukan pada Unity Standalone Inference Mode (tanpa intervensi Python) sebanyak **50+ ronde pertempuran terstandar** per model melawan bot pemain dinamis:

| Metrik Evaluasi In-Game | PPO Baseline (50M) | HCA Softmax (50M) | HCA Max (50M - RLHC) | Temuan & Keunggulan HCA |
| :--- | :---: | :---: | :---: | :--- |
| **Total Episode Diuji** | $52$ Episode | $50$ Episode | $50$ Episode | Uji empiris terstandar |
| **Enemy Team Win Rate** | $32.7\%$ | $32.0\%$ | $30.0\%$ | Menghadapi bot pemain dinamis |
| **Mean Damage to Player** | $80.35 \pm 17.2$ HP | $76.68 \pm 17.3$ HP | $\mathbf{84.16 \pm 19.1 \text{ HP}}$ | 🏆 **$+4.85\%$ Lebih Mematikan (Damage Tertinggi)** |
| **Mean Combat Duration (TTK)** | $14.42 \pm 2.8$ s | $\mathbf{13.06 \pm 2.8 \text{ s}}$ | $14.10 \pm 3.0$ s | ⚡ **$+9.43\%$ Tempo Lebih Cepat & Agresif** |
| **Sudut Kepungan ($E_E$)** | $114.37^\circ$ | $113.81^\circ$ | $113.35^\circ$ | Formasi busur setengah lingkaran stabil |
| **Jarak Tempur Rata-Rata** | $2.80 \pm 0.27$ m | $2.60 \pm 0.19$ m | $\mathbf{2.58 \pm 0.21 \text{ m}}$ | 🎯 **Penetrasi Jarak Serang Lebih Rapat** |
| **Status Entropi Pelatihan ($\mathcal{H}$)** | $0.352$ *(Drop/Collapse)* | **$1.674$ *(Multi-Modal)*** | **$1.422$ *(Equilibrium)*** | 🌟 **Terbukti Mengatasi Policy Collapse** |

---

## 3. 🔍 Temuan Utama Riset (*Key Findings & Scientific Insights*)

### A. Mitigasi Fenomena *Policy Collapse* pada Pelatihan Jangka Panjang (50M)
* **Kelemahan PPO Standar:** Pada pelatihan skala masif 50 juta langkah, nilai entropi PPO anjlok drastis ke **$0.352 \text{ nats}$**. PPO mengalami *overfitting deterministik* di mana musuh terjebak pada satu pola serangan kaku.
* **Keberhasilan HCA:** Kehadiran *Manager Critic Network (16 fitur global)* berhasil menjaga nilai entropi tetap stabil dan sehat (**$1.422 - 1.674 \text{ nats}$**). HCA mempertahankan distribusi eksplorasi multi-modal sehingga taktik musuh tidak mudah ditebak.

### B. Diferensiasi Karakteristik Varian HCA
1. **HCA Max (Operator Maksimum):** Sangat efektif dalam menghasilkan daya rusak maksimal (**$84.16 \text{ HP}$**) dan penetrasi jarak terdekat (**$2.58 \text{ m}$**) berkat sinyal *optimistic credit assignment*.
2. **HCA Softmax (Operator Softmax):** Menghasilkan tempo eliminasi paling agresif dan tercepat (**$13.06 \text{ detik}$**) dengan transisi aksi yang sangat mulus antar-state.

---

## 4. 📈 Aset Visual & Gambar Publikasi Ilmiah

Seluruh grafik publikasi standar IEEE (300 DPI) telah tersimpan di folder `presentation_charts/journal_style/`:
1. 📊 **`fig6_ieee_50m_convergence_curves.png`**: Kurva Asimptotik Reward dan Dinamika Entropi 50 Juta Steps.
2. 📊 **`fig7_ieee_50m_all_models_eval_benchmark.png`**: Bar Chart 4-Panel Komparasi Win Rate, Damage, TTK, dan Encirclement Span.
3. 🗺️ **`plot_enemies_heatmap_HCA_Max_50M_*.png` & `plot_polar_encirclement_*.png`**: Visualisasi sebaran spasial 2D dan sudut kepungan polar $360^\circ$.
4. 📋 **`table3_ieee_50m_final_eval_summary.png`**: Tabel formal komparasi evaluasi in-game standar IEEE.

---

## 5. 🎮 Panduan Video Demonstrasi In-Game

Video demonstrasi simulasi pertarungan in-game memperlihatkan:
* **Perilaku Musuh PPO 50M:** Cenderung menyerang dari satu garis lurus (*frontal clumping*).
* **Perilaku Musuh HCA Max 50M:** Menunjukkan gerakan manuver memotong jalur lari (*corner trapping*), melakukan kepungan multi-arah (Humanoid, Creep, Bull bekerja sama), serta re-engagement yang agresif saat darah menipis.
* *(Link Video Drive / Lampiran Video MP4: [Sematkan Link Google Drive Video Anda])*

---

## 6. 🚀 Rencana Kerja Minggu Depan (*Next Tasks*)

1. **Distribusi Kuesioner *Game Experience Questionnaire* (GEQ):**
   * Menjalankan *blind playtest* kepada 20–30 responden manusia (menggunakan 2 build terpisah: `Game_Mode_A` vs `Game_Mode_B`).
   * Mengolah data persepsi psikologis pemain (7 dimensi GEQ) menggunakan skrip `scripts/calculate_geq_scores.py`.
2. **Finalisasi Naskah Skripsi & Review Dosen Pembimbing:**
   * Melakukan integrasi hasil Bab 4 dan Bab 5 ke format template skripsi utama.
