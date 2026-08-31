# STATISTICAL SIGNIFICANCE & HYPOTHESIS TESTING REPORT (50M CONVERGENCE)
Generated for Thesis Chapter 4: Results and Discussions

## Metric: Damage Dealt to Player (HP)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 80.35 ± 17.23 HP | 63.40 | 69.00 [28.0 - 122.0] |
| **HCA Softmax (50M)** | 50 | 76.68 ± 17.26 HP | 62.25 | 63.00 [20.5 - 115.0] |
| **HCA Max (50M)** | 50 | 84.16 ± 19.10 HP | 68.90 | 73.00 [19.0 - 150.5] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -0.295, p = 0.7683 (df=100.0) | U = 434, z = -5.80, p = 0.0000 | d = -0.058 (Negligible) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 0.291, p = 0.7714 (df=98.5) | U = 382, z = -6.15, p = 0.0000 | d = 0.058 (Negligible) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 149) = 0.166, p-val ≈ 0.4340

---

## Metric: Combat Duration / TTK (s)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 14.42 ± 2.82 s | 10.37 | 12.75 [6.5 - 23.4] |
| **HCA Softmax (50M)** | 50 | 13.06 ± 2.77 s | 9.99 | 10.89 [4.6 - 20.8] |
| **HCA Max (50M)** | 50 | 14.10 ± 3.04 s | 10.97 | 12.93 [3.3 - 24.0] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -0.675, p = 0.4998 (df=100.0) | U = 1244, z = -0.37, p = 0.7078 | d = -0.134 (Negligible) | Equivalent Performance Band |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -0.152, p = 0.8796 (df=99.1) | U = 1259, z = -0.27, p = 0.7837 | d = -0.030 (Negligible) | Equivalent Performance Band |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 149) = 0.234, p-val ≈ 0.4073

---

## Metric: Mean Encirclement Angle Span (E_E)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 114.37 ± 10.46 ° | 38.47 | 121.30 [90.2 - 147.8] |
| **HCA Softmax (50M)** | 50 | 113.81 ± 9.02 ° | 32.53 | 117.25 [100.3 - 133.3] |
| **HCA Max (50M)** | 50 | 113.35 ± 8.77 ° | 31.64 | 122.70 [98.0 - 132.8] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -0.080, p = 0.9365 (df=98.4) | U = 1261, z = -0.26, p = 0.7940 | d = -0.016 (Negligible) | Equivalent Performance Band |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -0.147, p = 0.8834 (df=97.7) | U = 1272, z = -0.19, p = 0.8513 | d = -0.029 (Negligible) | Equivalent Performance Band |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 149) = 0.011, p-val ≈ 0.4955

---

## Metric: Mean Inter-Agent Distance to Player
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 2.80 ± 0.27 m | 0.99 | 2.58 [2.2 - 3.2] |
| **HCA Softmax (50M)** | 50 | 2.60 ± 0.19 m | 0.69 | 2.36 [2.2 - 2.7] |
| **HCA Max (50M)** | 50 | 2.58 ± 0.21 m | 0.76 | 2.37 [2.1 - 2.7] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -1.174, p = 0.2405 (df=90.9) | U = 1280, z = -0.13, p = 0.8935 | d = -0.231 (Small) | Equivalent Performance Band |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -1.248, p = 0.2119 (df=95.4) | U = 1052, z = -1.66, p = 0.0969 | d = -0.246 (Small) | Equivalent Performance Band |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 149) = 1.091, p-val ≈ 0.1377

---
