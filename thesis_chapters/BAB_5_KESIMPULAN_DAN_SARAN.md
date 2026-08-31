# BAB V: KESIMPULAN DAN SARAN

## 5.1 Kesimpulan

Berdasarkan seluruh rangkaian penelitian, perancangan arsitektur, pelatihan jangka panjang asimptotik (50 Juta Steps), serta evaluasi empiris *in-game* multi-ronde yang telah dilakukan, dapat ditarik beberapa kesimpulan utama sebagai berikut:

1. **Efektivitas Arsitektur HCA dalam Pelatihan Jangka Panjang:**
   * Algoritma *Hierarchical Critic Assignment* (HCA) terbukti secara konsisten mengatasi kelemahan mendasar PPO Baseline pada pelatihan skala besar (50.000.000 langkah).
   * PPO standar mengalami **Policy Collapse / Overfitting** yang ditandai dengan anjloknya entropi kebijakan ke nilai **$0.352 \text{ nats}$**. Sebaliknya, **HCA Softmax** ($\mathcal{H} = 1.674$) dan **HCA Max** ($\mathcal{H} = 1.422$) berhasil mempertahankan distribusi kebijakan yang sehat, mencegah agen terjebak dalam pola aksi deterministik monoton.

2. **Peningkatan Performa Tempur In-Game (Combat Efficiency):**
   * **HCA Max (RLHC Canonical)** membuktikan keunggulan agresivitas dan penetrasi serangan dengan mencatatkan rata-rata kerusakan tertinggi ke pemain sebesar **$84.16 \text{ HP}$ per episode** ($+4.85\%$ lebih tinggi dibanding PPO Baseline).
   * **HCA Softmax** mencatatkan efisiensi waktu eliminasi pertempuran (*Time-to-Kill / TTK*) tercepat dengan rata-rata **$13.06 \text{ detik}$** ($+9.43\%$ lebih cepat dibanding PPO Baseline $14.42 \text{ detik}$).

3. **Koordinasi Spasial dan Formasi Kepungan Multi-Agen:**
   * Analisis *Spatial Heatmap 2D* ($32 \times 32$) dan diagram polar $360^\circ$ menunjukkan bahwa agen dengan kritik manajer global mampu memanfaatkan seluruh dimensi arena secara merata (*arena coverage*), melakukan pemotongan jalur lari (*corner trapping*), serta membentuk sudut kepungan busur (*encirclement span* $\approx 113.4^\circ - 113.8^\circ$) secara konsisten.

4. **Kontribusi Terhadap Kecerdasan Buatan dalam Game Real-Time 3D:**
   * Integrasi *Hierarchical Critic Assignment* pada Unity ML-Agents berhasil membuktikan bahwa pemisahan representasi kritik lokal (*worker perception*) dan kritik global (*manager coordination*) adalah pendekatan yang kokoh dan efisien untuk melatih NPC multi-agen yang menantang, dinamis, dan tidak mudah dieksploitasi oleh pemain.

---

## 5.2 Saran

Untuk pengembangan dan penelitian lanjutan di masa mendatang, disarankan beberapa poin berikut:

1. **Eksplorasi Kurikulum Adaptif Otomatis (*Curriculum Learning & Domain Randomization*):**
   * Mengembangkan sistem kurikulum dinamis berbasis performa pemain secara *online* untuk menyesuaikan tingkat kesulitan (*Dynamic Difficulty Adjustment / DDA*) secara *real-time*.
2. **Peningkatan Skala Agen (*Scalability to Massive Multi-Agent*):**
   * Menguji arsitektur HCA pada skenario pertarungan dengan jumlah musuh yang lebih besar ($N > 10$ agen) dan tipe musuh dengan *archetype skills* yang lebih heterogen (misalnya: *caster*, *tanker*, dan *ranged sniper*).
3. **Pengujian Kualitatif Pengalaman Pemain (*Human Playtest & UX Study*):**
   * Melakukan studi survei kualitatif *Game User Experience* (GUX) dengan melibatkan pemain manusia dari berbagai tingkat keahlian (*novice, intermediate, pro*) untuk mengukur tingkat kepuasan (*fun*), ketegangan (*tension*), dan persepsi kecerdasan musuh (*perceived intelligence*).
4. **Integrasi Komunikasi Grafis / Graph Neural Networks (GNN):**
   * Menyelidiki penggunaan *Graph Attention Networks* (GAT) pada layer *Manager Critic* untuk memodelkan relasi topologi antar-agen secara dinamis saat berada di arena non-linear (bertingkat/labirin).
