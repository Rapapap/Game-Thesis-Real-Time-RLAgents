# STATISTICAL SIGNIFICANCE & HYPOTHESIS TESTING REPORT (50M CONVERGENCE)
Generated for Thesis Chapter 4: Results and Discussions

## Metric: Damage Dealt to Player (HP)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 90.68 ± 9.98 HP | 36.00 | 86.00 [68.5 - 113.5] |
| **HCA Softmax (50M)** | 49 | 98.41 ± 10.55 HP | 37.66 | 94.00 [72.0 - 134.0] |
| **HCA Max (50M)** | 50 | 101.64 ± 9.97 HP | 35.97 | 98.00 [74.0 - 133.5] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 1.043, p = 0.2968 (df=96.6) | U = -373, z = -11.18, p = 0.0000 | d = 0.210 (Small) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 1.523, p = 0.1278 (df=98.0) | U = -481, z = -11.93, p = 0.0000 | d = 0.305 (Small) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 1.186, p-val ≈ 0.1177

---

## Metric: Combat Duration / TTK (s)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 30.43 ± 5.30 s | 19.12 | 25.88 [19.1 - 34.1] |
| **HCA Softmax (50M)** | 49 | 28.58 ± 3.50 s | 12.50 | 22.82 [19.3 - 37.6] |
| **HCA Max (50M)** | 50 | 29.14 ± 2.78 s | 10.03 | 26.55 [22.0 - 36.5] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -0.573, p = 0.5667 (df=84.6) | U = 1222, z = -0.02, p = 0.9832 | d = -0.115 (Negligible) | Equivalent Performance Band |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -0.425, p = 0.6711 (df=74.1) | U = 1136, z = -0.79, p = 0.4319 | d = -0.085 (Negligible) | Equivalent Performance Band |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 0.217, p-val ≈ 0.4143

---

## Metric: Mean Encirclement Angle Span (E_E)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 63.14 ± 5.30 ° | 19.11 | 62.30 [51.4 - 76.0] |
| **HCA Softmax (50M)** | 49 | 70.58 ± 5.14 ° | 18.37 | 67.50 [58.6 - 82.1] |
| **HCA Max (50M)** | 50 | 57.54 ± 4.42 ° | 15.94 | 54.70 [49.1 - 66.1] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 1.976, p = 0.0481 (df=97.0) | U = 829, z = -2.77, p = 0.0056 | d = 0.397 (Small) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -1.592, p = 0.1114 (df=94.9) | U = 1198, z = -0.36, p = 0.7200 | d = -0.318 (Small) | Equivalent Performance Band |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 6.646, p-val ≈ 0.0075

---

## Metric: Mean Inter-Agent Distance to Player
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 3.20 ± 0.18 m | 0.66 | 3.12 [2.7 - 3.6] |
| **HCA Softmax (50M)** | 49 | 2.86 ± 0.13 m | 0.46 | 2.82 [2.5 - 3.1] |
| **HCA Max (50M)** | 50 | 2.93 ± 0.10 m | 0.37 | 2.87 [2.7 - 3.3] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -2.957, p = 0.0031 (df=87.1) | U = 1052, z = -1.21, p = 0.2260 | d = -0.592 (Medium) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -2.454, p = 0.0141 (df=76.7) | U = 854, z = -2.73, p = 0.0063 | d = -0.491 (Small) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 5.979, p-val ≈ 0.0084

---
