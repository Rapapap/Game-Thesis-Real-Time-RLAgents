# STATISTICAL SIGNIFICANCE & HYPOTHESIS TESTING REPORT (50M CONVERGENCE)
Generated for Thesis Chapter 4: Results and Discussions

## Metric: Damage Dealt to Player (HP)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 80.35 ± 17.23 HP | 63.40 | 69.00 [28.0 - 122.0] |
| **HCA Softmax (50M)** | 49 | 98.41 ± 10.55 HP | 37.66 | 94.00 [72.0 - 134.0] |
| **HCA Max (50M)** | 72 | 96.00 ± 8.37 HP | 36.23 | 88.00 [68.0 - 124.5] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 1.752, p = 0.0797 (df=83.9) | U = -41, z = -8.94, p = 0.0000 | d = 0.344 (Small) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 1.602, p = 0.1092 (df=74.9) | U = 364, z = -7.64, p = 0.0000 | d = 0.317 (Small) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 170) = 2.373, p-val ≈ 0.0088

---

## Metric: Combat Duration / TTK (s)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 14.42 ± 2.82 s | 10.37 | 12.75 [6.5 - 23.4] |
| **HCA Softmax (50M)** | 49 | 28.58 ± 3.50 s | 12.50 | 22.82 [19.3 - 37.6] |
| **HCA Max (50M)** | 72 | 27.56 ± 2.27 s | 9.82 | 24.56 [21.2 - 33.0] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 6.174, p = 0.0000 (df=93.5) | U = 470, z = -5.46, p = 0.0000 | d = 1.236 (Large) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 7.118, p = 0.0000 (df=106.4) | U = 710, z = -5.88, p = 0.0000 | d = 1.307 (Large) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 170) = 28.762, p-val ≈ 0.0017

---

## Metric: Mean Encirclement Angle Span (E_E)
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 114.37 ± 10.46 ° | 38.47 | 121.30 [90.2 - 147.8] |
| **HCA Softmax (50M)** | 49 | 70.58 ± 5.14 ° | 18.37 | 67.50 [58.6 - 82.1] |
| **HCA Max (50M)** | 72 | 56.58 ± 3.56 ° | 15.39 | 54.40 [47.1 - 65.0] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = -7.364, p = 0.0000 (df=74.1) | U = 648, z = -4.25, p = 0.0000 | d = -1.439 (Large) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = -10.255, p = 0.0000 (df=62.9) | U = 425, z = -7.33, p = 0.0000 | d = -2.101 (Large) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 170) = 81.867, p-val ≈ 0.0006

---

## Metric: Mean Inter-Agent Distance to Player
| Model Architecture | Sample Size (N) | Mean ± 95% CI | Std Dev | Median [IQR: Q25 - Q75] |
| :--- | :---: | :---: | :---: | :---: |
| **PPO Baseline (50M)** | 52 | 2.80 ± 0.27 m | 0.99 | 2.58 [2.2 - 3.2] |
| **HCA Softmax (50M)** | 49 | 2.86 ± 0.13 m | 0.46 | 2.82 [2.5 - 3.1] |
| **HCA Max (50M)** | 72 | 2.89 ± 0.09 m | 0.37 | 2.86 [2.6 - 3.2] |

### Pairwise Inferential Hypothesis Testing vs. PPO Baseline:
| Comparison | Welch's t-test (t, p) | Mann-Whitney U (U, z, p) | Cohen's d (Effect Size) | Statistical Conclusion |
| :--- | :---: | :---: | :---: | :--- |
| **HCA Softmax (50M)** vs. **PPO Baseline** | t = 0.420, p = 0.6748 (df=72.5) | U = 697, z = -3.92, p = 0.0001 | d = 0.082 (Negligible) | Statistically Significant (p < 0.05) |
| **HCA Max (50M)** vs. **PPO Baseline** | t = 0.640, p = 0.5225 (df=61.3) | U = 711, z = -5.88, p = 0.0000 | d = 0.132 (Negligible) | Statistically Significant (p < 0.05) |

* **One-Way ANOVA Across All 3 Architectures:** F(2, 170) = 0.318, p-val ≈ 0.3754

---
