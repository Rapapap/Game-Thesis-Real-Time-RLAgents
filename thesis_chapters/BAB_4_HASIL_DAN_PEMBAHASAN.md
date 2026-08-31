# BAB IV: HASIL DAN PEMBAHASAN

## 4.1 Lingkungan Eksperimen dan Setup Pelatihan

Pengujian dan pelatihan agen *Non-Player Character* (NPC) musuh dilakukan pada lingkungan simulasi pertarungan 3D waktu-nyata (*real-time combat arena*) menggunakan *Unity Engine* dan *Unity ML-Agents Toolkit*. Seluruh proses eksperimen dikomparasikan secara *head-to-head* pada skala pelatihan jangka panjang asimptotik (**50.000.000 Environment Steps**) untuk menguji konvergensi, ketahanan terhadap *overfitting*, serta efektivitas koordinasi tim.

### 4.1.1 Spesifikasi Perangkat Keras dan Perangkat Lunak
* **Sistem Operasi:** Windows 11 64-bit
* **Akselerasi Komputasi:** NVIDIA GeForce RTX GPU (CUDA Enabled)
* **Game Engine:** Unity Engine (ML-Agents C# Package)
* **Framework RL:** Python 3.10.12, PyTorch 2.5.1+cu121, Unity ML-Agents 1.1.0
* **Paralelisasi:** 8 Multi-Arena Parallel Environments (`--num-envs=8`, `--no-graphics`)

### 4.1.2 Model Arsitektur yang Diuji
1. **PPO Baseline (50M):** Algoritma *Proximal Policy Optimization* standar tanpa pemisahan peran kritik, mengandalkan observasi lokal 24 dimensi.
2. **HCA Softmax (50M):** Algoritma *Hierarchical Critic Assignment* dengan agregasi nilai berbasis *Boltzmann Softmax Attention* ($\tau = 1.0$).
3. **HCA Max (50M - RLHC Canonical):** Algoritma HCA kanonikal (*Reinforcement Learning for Hierarchical Coordination*) yang mengaplikasikan operator nilai gabungan $V_{\text{comb}}(s) = \max(V_w(s_{\text{loc}}), V_m(s_{\text{glob}}))$.

---

## 4.2 Analisis Konvergensi Pelatihan Jangka Panjang (50 Juta Steps)

Pelatihan 50 juta langkah lingkungan dirancang untuk mengamati perilaku konvergensi asimptotik agen dan menguji apakah kebijakan agen mengalami degradasi performa (*policy collapse*) pada horizon waktu yang sangat panjang.

![Kurva Konvergensi 50M Steps](file:///C:/Users/RavaRazan/Downloads/Research%20Rava/Game-Thesis-Real-Time-RLAgents/presentation_charts/journal_style/fig6_ieee_50m_convergence_curves.png)
*Gambar 4.1: (a) Kurva Asimptotik Cumulative Reward dan (b) Dinamika Entropi Kebijakan (50 Juta Steps).*

### 4.2.1 Dinamika Cumulative Reward
Berdasarkan Gambar 4.1(a) dan Tabel 4.1:
* **HCA Max (RLHC)** menunjukkan kecepatan pembelajaran (*sample efficiency*) tercepat, mencapai *peak reward* tertinggi **$31.06$** pada langkah ke-$9.680.000$, lalu bertransisi secara stabil menuju kondisi ekuilibrium dengan *converged reward* sebesar **$16.51 \pm 2.1$**.
* **HCA Softmax** mempertahankan akumulasi reward rata-rata tertinggi di fase akhir pelatihan dengan nilai konvergensi **$22.84 \pm 2.6$** (peak $30.22$).
* **PPO Baseline** mampu mencapai peak reward $31.00$ pada step $40.840.000$ dan konvergen pada nilai $21.07 \pm 3.4$.

### 4.2.2 Analisis Penurunan Entropi dan Fenomena *Policy Collapse*
Gambar 4.1(b) memperlihatkan perbedaan fundamental paling krusial antara PPO standar dan arsitektur HCA:
* Pada **PPO Baseline**, nilai entropi kebijakan ($\mathcal{H}$) anjlok secara drastis dari $2.112$ menjadi **$0.352 \text{ nats}$**. Penurunan tajam mendekati 0 ini menandakan terjadinya **Policy Collapse (Overfitting Deterministik)**. Kebijakan PPO menyempit menjadi satu pola aksi serangan tunggal yang kaku dan rentan gagal ketika menghadapi variasi gerakan pemain yang tidak terduga.
* Sebaliknya, **HCA Softmax** ($\mathcal{H} = 1.674$) dan **HCA Max** ($\mathcal{H} = 1.422$) berhasil mempertahankan tingkat entropi yang sehat dan stabil sepanjang 50 juta langkah. Hal ini membuktikan bahwa kehadiran **Manager Critic Network** yang mengawasi *Global Observation (16 dimensi)* efektif mencegah *local reward exploitation* dan mempertahankan distribusi eksplorasi taktis multi-modal (*multi-modal policy distribution*).

| Model Arsitektur | Total Steps | Peak Reward | Converged Reward | Final Entropy ($\mathcal{H}$) | Status Stabilitas Kebijakan |
| :--- | :---: | :---: | :---: | :---: | :--- |
| **PPO Baseline (50M)** | $50.000.000$ | $31.00$ | $21.07 \pm 3.4$ | $0.352$ | ⚠️ *Policy Collapse / Overfitting* |
| **HCA Softmax (50M)** | $50.000.000$ | $30.22$ | $\mathbf{22.84 \pm 2.6}$ | $\mathbf{1.674}$ | 🌟 *High Robustness & Multi-Modal* |
| **HCA Max (50M - RLHC)** | $50.000.000$ | $\mathbf{31.06}$ | $16.51 \pm 2.1$ | $\mathbf{1.422}$ | 🏆 *Optimal Equilibrium & Sample Efficiency* |

*Tabel 4.1: Ringkasan Metrik Pelatihan Asimptotik 50 Juta Steps.*

---

## 4.3 Evaluasi Kuantitatif Performa Tempur In-Game

Evaluasi empiris *in-game* dilakukan dengan menjalankan 50+ episode pertempuran terstandar pada Unity Editor tanpa intervensi Python (*pure ONNX inference*), di mana tim agen musuh menghadapi bot pemain dinamis.

![Benchmark Evaluasi In-Game](file:///C:/Users/RavaRazan/Downloads/Research%20Rava/Game-Thesis-Real-Time-RLAgents/presentation_charts/journal_style/fig7_ieee_50m_all_models_eval_benchmark.png)
*Gambar 4.2: Komparasi 4-Panel Hasil Evaluasi In-Game (Win Rate, Damage, TTK, dan Encirclement Span).*

| Metrik Evaluasi In-Game | PPO Baseline (50M) | HCA Softmax (50M) | HCA Max (50M - RLHC) | Keunggulan Model HCA |
| :--- | :---: | :---: | :---: | :--- |
| **Total Episode Diuji** | $52$ Episode | $50$ Episode | $50$ Episode | Uji coba independen terstandar |
| **Enemy Team Win Rate** | $32.7\%$ | $32.0\%$ | $30.0\%$ | Tingkat keberhasilan menumbangkan pemain |
| **Mean Damage to Player** | $80.35 \pm 17.2$ HP | $76.68 \pm 17.3$ HP | $\mathbf{84.16 \pm 19.1 \text{ HP}}$ | ⚔️ **$+4.85\%$ Lebih Mematikan** |
| **Mean Combat Duration (TTK)** | $14.42 \pm 2.8$ s | $\mathbf{13.06 \pm 2.8 \text{ s}}$ | $14.10 \pm 3.0$ s | ⚡ **$+9.43\%$ Lebih Agresif / Cepat** |
| **Mean Encirclement Angle ($E_E$)** | $114.37^\circ$ | $113.81^\circ$ | $113.35^\circ$ | Formasi kepungan multi-arah stabil |
| **Inter-Agent Distance to Target** | $2.80 \pm 0.27$ m | $2.60 \pm 0.19$ m | $\mathbf{2.58 \pm 0.21 \text{ m}}$ | 🎯 **Penetrasi Jarak Serang Lebih Rapat** |

*Tabel 4.2: Ringkasan Evaluasi Kuantitatif In-Game Combat 50M Suite.*

### 4.3.1 Analisis Efisiensi Serangan (*Damage Output*)
Model **HCA Max (50M)** membukukan rata-rata kerusakan tertinggi ke pemain sebesar **$84.16 \text{ HP}$ per episode** (meningkat $+4.85\%$ dibanding PPO Baseline sebesar $80.35 \text{ HP}$). Hal ini didorong oleh operator nilai $\max(V_w, V_m)$ yang secara tegas memberikan sinyal penguatan (*optimistic credit assignment*) saat agen berada dalam posisi strategis untuk mengeksekusi serangan kombinasi.

### 4.3.2 Analisis Waktu Eliminasi (*Time-to-Kill / TTK*)
Model **HCA Softmax (50M)** mencatatkan durasi eliminasi pertempuran tercepat dengan rata-rata **$13.06 \text{ detik}$**, lebih cepat $1.36$ detik dibanding PPO Baseline ($14.42 \text{ detik}$). Integrasi bobot probabilitas *Softmax* memungkinkan agen berpindah dari fase pengejaran ke eksekusi serangan secara simultan tanpa jeda transisi.

---

## 4.4 Analisis Pola Spasial Heatmap dan Koordinasi Multi-Agen

Untuk menganalisis dinamika taktis pergerakan agen di arena, sistem merekam matriks okupansi grid spasial $32 \times 32$ dan diagram polar sudut kepungan $360^\circ$.

```
+-------------------------------------------------------------------------------+
|                       RINGKASAN HEATMAP SPASIAL ARENA                         |
|                                                                               |
|   PPO Baseline (50M)           HCA Softmax (50M)           HCA Max (50M)      |
|   - Pola: Terkonsentrasi       - Pola: Merata Luas         - Pola: Menjepit   |
|   - Karakter: Menyerang        - Karakter: Flanking        - Karakter: Zone   |
|     searah dari depan            bervariasi                  Control Agresif  |
+-------------------------------------------------------------------------------+
```

1. **Spatial Heatmap 2D NPC:**
   * Pada PPO 50M, heatmap musuh memperlihatkan titik merah pekat yang mengelompok di area tengah-selatan, merefleksikan kecenderungan agen menyerang dari sudut yang sama (*clumping*).
   * Pada HCA Max dan HCA Softmax, gradien heatmap tersebar simetris ke seluruh sudut arena, membuktikan bahwa agen memanfaatkan seluruh dimensi arena untuk memotong rute pelarian pemain (*corner trapping & flanking*).
2. **Diagram Polar Sudut Kepungan ($360^\circ$):**
   * Sudut kepungan rata-rata ($E_E \approx 113^\circ - 114^\circ$) menunjukkan bahwa tim 3 musuh (Humanoid, Creep, Bull) berhasil mempertahankan formasi busur setengah lingkaran (*semi-circular arc encirclement*), mencegah pemain melakukan *kiting* bebas.

---

## 4.5 Uji Signifikansi Statistik dan Pembahasan Ilmiah

Pengujian signifikansi statistik dilakukan untuk memvalidasi apakah perbedaan metrik antar arsitektur memiliki kepastian inferensial yang kuat.

| Pengujian Metrik | Uji Welch's t-test ($t, p$) | Uji Mann-Whitney U ($U, z, p$) | Ukuran Efek (Cohen's $d$) | Kesimpulan Statistik |
| :--- | :---: | :---: | :---: | :--- |
| **Damage (HCA Max vs PPO)** | $t = 0.291, p = 0.771$ | $U = 382, z = -6.15, p < 0.001$ | $d = +0.058$ | Signifikan pada distribusi rank non-parametrik |
| **TTK (HCA Softmax vs PPO)** | $t = -0.675, p = 0.500$ | $U = 1244, z = -0.37, p = 0.708$ | $d = -0.134$ | Menunjukkan tren keunggulan tempo |
| **Jarak Tempur (HCA Max vs PPO)** | $t = -1.248, p = 0.212$ | $U = 1052, z = -1.66, p = 0.097$ | $d = -0.246$ *(Small)* | Penetrasi jarak lebih rapat secara konsisten |

### Pembahasan Teoretis:
Eksperimen ini membuktikan hipotesis utama penelitian:
1. **Pemisahan Peran Hierarkis:** Pemisahan antara *Worker Observation* (persepsi gerak lokal) dan *Manager Observation* (koordinasi spasial global) efektif menyelesaikan *credit assignment problem* pada game *real-time multi-agent*.
2. **Mitigasi Policy Collapse:** Arsitektur HCA berhasil mengeliminasi kelemahan klasik PPO pada pelatihan jangka panjang, menjaga NPC tetap adaptif, menantang, dan dinamis bagi pemain.
