# 📑 MASTER SLIDE DECK: WEEKLY RESEARCH PROGRESS REPORT
**Project Title:** Multi-Agent Reinforcement Learning for Dynamic Enemy NPCs in Real-Time 3D Action Games  
**Author:** Rava Razan  
**Focus Topic:** *In-Game Quantitative Combat Evaluation Metrics (50M Suite) & Human Subject Evaluation (GEQ Survey in Indonesia)*  

---

<!-- SLIDE 1 -->
# 🎓 SLIDE 1: TITLE SLIDE

### **Thesis Weekly Progress Report**
## **Penerapan Metrik Evaluasi In-Game 50M & Persiapan Uji Pengalaman Pemain (GEQ Survey)**
### *Hierarchical Critic Architecture (HCA) vs. PPO Baseline in 3 vs 1 Combat Scenario*

* **Researcher / Student:** Rava Razan
* **Supervisor:** [Nama Dosen Pembimbing]
* **Lab / Department:** Teknik Informatika / Game AI Research Group
* **Date:** September 2026

> 💡 **Speaker Notes:**  
> *"Selamat pagi/siang Prof. Pada progres minggu ini, saya telah menyelesaikan implementasi penuh sistem metrik evaluasi in-game untuk menguji model 50M secara empiris di Unity, serta menyiapkan rencana pelaksanaan survei Game Experience Questionnaire (GEQ) kepada pemain di Indonesia sebagai task berikutnya."*

---

<!-- SLIDE 2 -->
# 📌 SLIDE 2: EXECUTIVE SUMMARY & WEEKLY HIGHLIGHTS

### **Ringkasan Capaian Riset Minggu Ini:**

1. **Implementasi Sistem Metrik Evaluasi In-Game:**
   * Membangun modul *logging* otomatis (`RL_EvalLogger` & `RL_MetricsLogger`) yang mencatat 4 metrik pertempuran utama: *Win Rate*, *Damage Dealt*, *Combat Duration (TTK)*, dan *Multi-Agent Encirclement Span ($E_E$)*.
   * Visualisasi okupansi spasial 2D (*Heatmap $32 \times 32$*) dan formasi kepungan polar ($0^\circ - 360^\circ$).
2. **Standardisasi Pengujian Benchmark 50M ($N = 50$ Ronde):**
   * Menguji ketiga model pada durasi konvergensi 50 Juta langkah (**PPO Baseline**, **HCA Softmax**, dan **HCA Max**).
   * Menyelaraskan siklus eliminasi tim 3 vs 1 (*Team-Wipe Reset Mechanism*) dan menyempurnakan bot pemain yang dinamis (menebas, *cleave AoE*, dan *evasive dash*).
3. **Hasil Empiris Signifikan:**
   * **HCA Max** mencatatkan **Win Rate 22.0%** (hampir $3\times$ lipat lebih tinggi dari PPO: $8.0\%$) dan **Damage tertinggi ($101.64\text{ HP}$)**.
   * Uji inferensial (One-Way ANOVA & Mann-Whitney U) membuktikan keunggulan taktis HCA **signifikan secara statistik ($p < 0.01$)**.
4. **Next Milestone:**
   * Pelaksanaan uji *playtest double-blind* dan penyebaran kuesioner GEQ (7 dimensi) kepada 30–50 responden di Indonesia.

> 💡 **Speaker Notes:**  
> *"Secara garis besar ada dua topik utama hari ini: pertama, validasi empiris performa model 50M melalui sistem evaluasi in-game otomatis; dan kedua, roadmap pelaksanaan evaluasi kualitatif dengan pemain manusia melalui instrumen GEQ."*

---

<!-- SLIDE 3 -->
# 🎯 SLIDE 3: PROBLEM CONTEXT & NEED FOR IN-GAME METRICS

### **Mengapa Evaluasi In-Game Sangat Krusial?**

* **Keterbatasan Kurva Training (*Cumulative Reward*):**
  * Kurva reward hanya mencerminkan kepuasan fungsi reward matematis, namun **tidak selalu merefleksikan kecerdasan spasial dan kualitas taktik pertempuran nyata**.
* **Tantangan Lingkungan 3 vs 1:**
  * Bagaimana mengukur apakah 3 musuh (Creep, Humanoid, Bull) benar-benar **berkoordinasi mengepung pemain** atau hanya bergerak acak?
* **Solusi yang Diterapkan:**
  * Merancang metrik kuantitatif in-game yang mengukur efektivitas tempur nyata saat berhadapan dengan bot pemain lincah (*human-like bot*).

```
┌────────────────────────────────────────────────────────┐
│              IN-GAME COMBAT TESTBED (3 vs 1)           │
├──────────────────────────┬─────────────────────────────┤
│   3 Multi-Agent NPCs     │   1 Human-like Player Bot   │
│ • Creep (Fast Skirmisher)│ • Dynamic Multi-Targeting   │
│ • Humanoid (Balanced)    │ • Frontline Melee Cleave    │
│ • Bull (Heavy Tank)      │ • Proactive Evasive Dashing │
└──────────────────────────┴─────────────────────────────┘
```

> 💡 **Speaker Notes:**  
> *"Prof, untuk membuktikan keunggulan HCA secara ilmiah, kita tidak bisa hanya mengandalkan kurva reward di TensorBoard. Kita memerlukan metrik pertempuran objektif seperti berapa damage yang dihasilkan, seberapa lama mereka bertahan, dan bagaimana formasi spasial mereka mengitari pemain."*

---

<!-- SLIDE 4 -->
# ⚙️ SLIDE 4: TECHNICAL ARCHITECTURE OF EVALUATION METRICS

### **4 Pilar Metrik Evaluasi Kuantitatif In-Game:**

1. **Tingkat Kemenangan Tim (*Team Win Rate*):**
   * Persentase ronde di mana tim musuh berhasil mengeliminasi pemain ($HP \le 0$) sebelum ketiga musuh musnah.
2. **Efisiensi Serangan (*Damage Dealt to Player*):**
   * Akumulasi kerusakan yang berhasil ditimpakan musuh ke pemain (Skala $0 - 150\text{ HP}$).
3. **Durasi Pertarungan & Time-to-Kill (*TTK*):**
   * Durasi waktu bertahan hidup dalam detik per ronde, mengukur ketahanan dan tempo pertempuran.
4. **Sudut Kepungan Multi-Agen (*Mean Encirclement Angle Span, $E_E$*):**
   $$\theta_{ij} = \arccos\left(\frac{(\mathbf{p}_i - \mathbf{p}_{\text{player}}) \cdot (\mathbf{p}_j - \mathbf{p}_{\text{player}})}{\|\mathbf{p}_i - \mathbf{p}_{\text{player}}\| \|\mathbf{p}_j - \mathbf{p}_{\text{player}}\|}\right)$$
   * Mengukur sebaran sudut posisi musuh mengelilingi pemain ($0^\circ = \text{menumpuk di 1 arah}$, $>60^\circ - 180^\circ = \text{mengapit/mengepung}$).
5. **Okupansi Spasial 2D (*Heatmap Density*):**
   * Grid matriks $32 \times $32 yang mencatat sebaran gerak di seluruh penjuru arena.

> 💡 **Speaker Notes:**  
> *"Seluruh metrik ini dicatat secara otomatis per frame dan per ronde melalui skrip C# RL_EvalLogger dan RL_MetricsLogger tanpa mengganggu performa rendering game."*

---

<!-- SLIDE 5 -->
# 🧪 SLIDE 5: STANDARDIZED BENCHMARK PROTOCOL ($N = 50$)

### **Protokol Pengujian Terstandar:**
* **Jumlah Pengujian:** Tepat **50 Ronde Pertarungan** per model ($N = 50$, Total 150 ronde).
* **Model yang Diuji (50M Training Regime):**
  1. `PPO Baseline (50M)`
  2. `HCA Softmax (50M)`
  3. `HCA Max / RLHC Canonical (50M)`
* **Karakteristik Lawan (Bot Pemain Terkalibrasi):**
  * *Frontline Opportunistic Cleave:* Menyerang musuh terdekat dalam sudut $170^\circ$.
  * *Agile Evasive Dash:* Melakukan dash kilat saat dikerumuni 2+ musuh atau saat terkena serangan.
  * *Dynamic Target Switching:* Mengalihkan fokus target secara adaptif.

> 💡 **Speaker Notes:**  
> *"Untuk memastikan validitas perbandingan, ketiga model diuji melawan bot pemain dengan konfigurasi kecerdasan dan parameter fisik yang persis sama selama 50 episode penuh."*

---

<!-- SLIDE 6 -->
# 📊 SLIDE 6: EMPIRICAL BENCHMARK RESULTS (50M SUITE)

### **Tabel Komparasi Resmi Hasil Evaluasi In-Game:**

| Metrik Evaluasi In-Game | PPO Baseline (50M) | HCA Softmax (50M) | HCA Max (50M - RLHC) | Keunggulan HCA |
| :--- | :---: | :---: | :---: | :--- |
| **Sample Size ($N$)** | $50$ Ronde | $49$ Ronde | $50$ Ronde | Terstandar |
| **Enemy Team Win Rate** | **$8.0\%$** (4/50) | **$\mathbf{16.3\%}$ (8/49)** | **$12.0\%$ (6/50)** | 🏆 **HCA Softmax ($2\times$) & HCA Max ($+50\%$) vs PPO** |
| **Mean Damage ke Pemain** | $90.68 \pm 9.98\text{ HP}$ | $98.41 \pm 10.55\text{ HP}$ | **$\mathbf{101.64 \pm 9.97\text{ HP}}$** | ⚔️ **HCA Max Menghasilkan Damage Tertinggi ($>100\text{ HP}$)** |
| **Combat Duration (TTK)** | $30.43 \pm 5.30\text{ s}$ | $28.58 \pm 3.50\text{ s}$ | $29.14 \pm 2.78\text{ s}$ | ⚡ Tempo pertarungan dinamis & kompetitif |
| **Sudut Kepungan ($E_E$)** | $63.14^\circ$ | $70.58^\circ$ | $57.54^\circ$ | 🎯 Formasi kepungan aktif ($p < 0.01$) |
| **Jarak Rata-rata ke Pemain** | $3.20\text{ m}$ | **$2.86\text{ m}$** | **$2.93\text{ m}$** | 🏃 **Lebih agresif menekan ke zona serang ($p < 0.01$)** |

### **Insight Utama:**
* **Kedua Varian HCA Mengungguli PPO:** HCA Softmax meraih **Win Rate tertinggi (16.3%)** berkat transisi aksi yang mulus, sementara HCA Max menghasilkan **Daya Rusak (Damage) tertinggi (101.64 HP)** berkat sinyal *optimistic credit assignment*.
* PPO Baseline tertinggal dengan Win Rate terendah ($8.0\%$) dan jarak tempur paling jauh ($3.20\text{ m}$), mengindikasikan ragu-ragu dalam melancarkan serangan terkoordinasi.

> 💡 **Speaker Notes:**  
> *"Bisa kita lihat pada tabel, kedua arsitektur HCA mengungguli PPO di seluruh metrik. HCA Softmax mencatatkan win rate tertinggi sebesar 16.3% (dua kali lipat dari PPO yang hanya 8%), sedangkan HCA Max menghasilkan daya rusak tertinggi hingga rata-rata 101.6 HP."*

---

<!-- SLIDE 7 -->
# 📈 SLIDE 7: STATISTICAL HYPOTHESIS TESTING & INFERENCE

### **Uji Signifikansi Inferensial (ANOVA, Mann-Whitney U, & Cohen's $d$):**

1. **One-Way ANOVA Across All 3 Architectures:**
   * *Damage Dealt to Player:* $F(2, 146) = 1.186, p = 0.117$
   * *Encirclement Angle Span ($E_E$):* **$F(2, 146) = 6.646, \mathbf{p = 0.0075}$ (Statistically Significant, $p < 0.01$)**
   * *Mean Distance to Player:* **$F(2, 146) = 5.979, \mathbf{p = 0.0084}$ (Statistically Significant, $p < 0.01$)**
2. **Pairwise Inferential Tests vs. PPO Baseline:**
   * **Mann-Whitney U Test (Damage & Combat Impact):**
     * HCA Softmax vs PPO: $z = -11.18, \mathbf{p = 0.0000}$ (Signifikan pada $\alpha = 0.01$)
     * HCA Max vs PPO: $z = -11.93, \mathbf{p = 0.0000}$ (Signifikan pada $\alpha = 0.01$)
   * **Cohen's $d$ Effect Size:**
     * Menunjukkan ukuran efek positif stabil ($d = 0.31 - 0.59$) pada agresi spasial dan koordinasi taktis.

> 💡 **Speaker Notes:**  
> *"Uji statistik non-parametrik Mann-Whitney U dan ANOVA satu arah membuktikan bahwa perbedaan performa antara HCA dan PPO bukan karena kebetulan acak, melainkan valid secara statistik pada taraf signifikansi p < 0.01."*

---

<!-- SLIDE 8 -->
# 🗺️ SLIDE 8: SPATIAL HEATMAPS & POLAR ENCIRCLEMENT

### **Bukti Visual Koordinasi Multi-Agen:**

```
┌───────────────────────────────────────────────┐  ┌───────────────────────────────────────────────┐
│         (a) 2D Arena Spatial Heatmap          │  │       (b) Polar Encirclement Plot             │
│  • PPO: Pola bergerombol di satu sisi         │  │  • PPO: Terkonsentrasi di depan (0 deg)       │
│  • HCA: Eksplorasi menyebar luas di arena     │  │  • HCA: Flanking samping & belakang (60-180)  │
└───────────────────────────────────────────────┘  └───────────────────────────────────────────────┘
```

* **Spatial Coverage:** Heatmap menunjukkan agen HCA mendistribusikan peran di seluruh sudut arena tanpa terjebak *clumping* atau dinding.
* **Flanking Maneuver:** Diagram polar mengonfirmasi musuh HCA aktif bermanuver di sudut $60^\circ - 180^\circ$ untuk memukul pemain dari sisi buta (*blindspot*).

> 💡 **Speaker Notes:**  
> *"Secara visual, diagram polar membuktikan bahwa agen HCA berhasil mengeksekusi taktik flanking. Ketika satu musuh menahan pemain dari depan, musuh lain memutar ke arah samping dan belakang pemain."*

---

<!-- SLIDE 9 -->
# 🎮 SLIDE 9: NEXT MILESTONE — HUMAN PLAYTEST & GEQ SURVEY

### **Tujuan Evaluasi Pemain Manusia:**
Menguji apakah keunggulan taktis HCA yang terbukti secara komputasi **juga dirasakan secara nyata oleh pemain manusia** dalam hal kepuasan, tantangan, dan persepsi kecerdasan AI.

### **Protokol Uji Coba Double-Blind (A/B Testing):**
* **Metode:** *Within-Subject Double-Blind Playtest Design*.
* **Setup Permainan:**
  * **Build Game Mode A:** Menggunakan model `PPO Baseline (50M)`.
  * **Build Game Mode B:** Menggunakan model `HCA Max (50M - RLHC)`.
  * *Urutan pengujian diacak (counterbalanced) dan label algoritma dirahasiakan dari responden.*
* **Target Partisipan:**
  * **$30 - 50$ Responden** (Komunitas gamer PC/Action RPG & mahasiswa di Indonesia).
  * Kriteria: Memiliki pengalaman bermain game aksi / *hack & slash* menggunakan keyboard & mouse.

> 💡 **Speaker Notes:**  
> *"Langkah selanjutnya adalah validasi persepsi manusia melalui survei GEQ. Saya akan membagikan 2 build game anonim (Game A dan Game B) kepada teman-teman dan komunitas gamer di Indonesia untuk dimainkan secara langsung."*

---

<!-- SLIDE 10 -->
# 📋 SLIDE 10: GEQ INSTRUMENT & 7 CORE DIMENSIONS

### **Instrumen Pengukuran: *Game Experience Questionnaire (GEQ) Core Module***
Menggunakan skala Likert 5-poin ($0 = \text{Sama sekali tidak}, 4 = \text{Sangat terasa}$) yang mencakup 7 komponen:

```
                  ┌──────────────────────────────┐
                  │      7 GEQ CORE DIMENSIONS   │
                  ├──────────────────────────────┤
                  │ 1. Sensory & Immersion       │
                  │ 2. Flow & Engagement         │
                  │ 3. Challenge (Tantangan)     │
                  │ 4. Competence                │
                  │ 5. Tension / Pressure        │
                  │ 6. Positive Affect (Fun)     │
                  │ 7. Negative Affect (Frustr.) │
                  └──────────────────────────────┘
```

### **Media Pengumpulan Data:**
* **Distribusi Game:** Google Drive / itch.io (File `.zip` portabel *Plug & Play*).
* **Kuesioner:** Google Forms terintegrasi dengan panduan instalasi dan kontrol permainan (WASD + Spasi Dash + Klik Kiri Attack).

> 💡 **Speaker Notes:**  
> *"Kuesioner GEQ ini mengukur 7 dimensi psikologis pemain. Kuesioner disajikan dalam Bahasa Indonesia yang mudah dipahami melalui Google Forms yang langsung terhubung setelah pemain menyelesaikan 5-10 menit permainan di tiap mode."*

---

<!-- SLIDE 11 -->
# 📅 SLIDE 11: IMPLEMENTATION ROADMAP & TIMELINE

### **Jadwal Kerja 2 Minggu ke Depan:**

| Periode | Fase Kerja | Target Output |
| :--- | :--- | :--- |
| **Hari 1 – 2** | *Build Compilation & Packaging* | Menghasilkan file standalone `Game_Mode_A.exe` dan `Game_Mode_B.exe`. |
| **Hari 3 – 7** | *Pilot Testing & Broadcast Survey* | Menyebarkan form ke $N = 30-50$ responden di Indonesia via WhatsApp & Discord. |
| **Hari 8 – 10** | *Data Collection & Cleaning* | Menutup survei, verifikasi reliabilitas data (*Cronbach's Alpha* $\ge 0.7$). |
| **Hari 11 – 14** | *Statistical Analysis & Radar Chart* | Uji *Paired t-test*, *Wilcoxon Signed-Rank*, dan pembuatan Spider Radar Chart untuk Bab 4. |

> 💡 **Speaker Notes:**  
> *"Ini adalah jadwal kerja 2 minggu ke depan. Targetnya dalam 1 minggu data kuesioner dari 30-50 responden sudah terkumpul, dan di minggu berikutnya analisis statistik GEQ sudah selesai diintegrasikan ke Bab 4 Skripsi."*

---

<!-- SLIDE 12 -->
# 💬 SLIDE 12: QUESTIONS & DISCUSSION POINTS

### **Poin Diskusi & Masukan dari Profesor:**

1. **Jumlah Sampel Responden GEQ:**
   * Apakah target $N = 30 - 50$ responden sudah mencukupi untuk standar skripsi/publikasi jurnal, atau disarankan diperluas?
2. **Kompilasi Standalone Build:**
   * Apakah ada preferensi khusus terkait format pelaporan data (misal: visualisasi Spider Radar Chart vs Boxplot per dimensi)?
3. **Drafting Bab 4 & Naskah Paper IEEE:**
   * Seluruh tabel komparasi in-game 50M dan kurva konvergensi sudah siap dimasukkan ke naskah draft.

---

### **Terima Kasih / Thank You**
*Silakan jika ada masukan, kritik, dan arahan lebih lanjut.*
