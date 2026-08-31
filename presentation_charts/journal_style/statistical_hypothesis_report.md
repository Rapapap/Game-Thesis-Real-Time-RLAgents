# STATISTICAL SIGNIFICANCE & HYPOTHESIS TESTING REPORT (50M CONVERGENCE)
Generated for Thesis Chapter 4: Results and Discussions

## Metric: Damage Dealt to Player (HP)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 83.48 ± 17.36 HP | 62.63 | 74.00 [31.5 - 130.0] |
| **HCA Softmax (50M)** | 49 | 98.41 ± 10.55 HP | 37.66 | 94.00 [72.0 - 134.0] |
| **HCA Max (50M)** | 50 | 101.64 ± 9.97 HP | 35.97 | 98.00 [74.0 - 133.5] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 1.440, p = 0.1497 (df=80.6) | U = -7, z = -8.62, p = 0.0000 | d = 0.288 (Small) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 1.778, p = 0.0754 (df=78.2) | U = 167, z = -7.47, p = 0.0000 | d = 0.356 (Small) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 2.113, p-val ≈ 0.0173

---

## Metric: Combat Duration / TTK (s)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 14.99 ± 2.82 s | 10.17 | 13.62 [7.3 - 23.6] |
| **HCA Softmax (50M)** | 49 | 28.58 ± 3.50 s | 12.50 | 22.82 [19.3 - 37.6] |
| **HCA Max (50M)** | 50 | 29.14 ± 2.78 s | 10.03 | 26.55 [22.0 - 36.5] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 5.927, p = 0.0000 (df=92.4) | U = 470, z = -5.28, p = 0.0000 | d = 1.194 (Large) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 7.005, p = 0.0000 (df=98.0) | U = 432, z = -5.64, p = 0.0000 | d = 1.401 (Large) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 26.711, p-val ≈ 0.0019

---

## Metric: Mean Encirclement Angle Span (E_E)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 115.17 ± 9.44 ° | 34.06 | 121.30 [93.2 - 144.8] |
| **HCA Softmax (50M)** | 49 | 70.58 ± 5.14 ° | 18.37 | 67.50 [58.6 - 82.1] |
| **HCA Max (50M)** | 50 | 57.54 ± 4.42 ° | 15.94 | 54.70 [49.1 - 66.1] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -8.128, p = 0.0000 (df=75.6) | U = 595, z = -4.41, p = 0.0000 | d = -1.625 (Large) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -10.838, p = 0.0000 (df=69.5) | U = 259, z = -6.83, p = 0.0000 | d = -2.168 (Large) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 77.899, p-val ≈ 0.0006

---

## Metric: Mean Inter-Agent Distance to Player
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 50 | 2.89 ± 0.25 m | 0.89 | 2.63 [2.3 - 3.2] |
| **HCA Softmax (50M)** | 49 | 2.86 ± 0.13 m | 0.46 | 2.82 [2.5 - 3.1] |
| **HCA Max (50M)** | 50 | 2.93 ± 0.10 m | 0.37 | 2.87 [2.7 - 3.3] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -0.196, p = 0.8448 (df=73.3) | U = 717, z = -3.56, p = 0.0004 | d = -0.039 (Negligible) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 0.341, p = 0.7329 (df=65.2) | U = 482, z = -5.29, p = 0.0000 | d = 0.068 (Negligible) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 146) = 0.184, p-val ≈ 0.4271

---
