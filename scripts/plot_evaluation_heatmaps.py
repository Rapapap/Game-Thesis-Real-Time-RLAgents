"""
Plotting Script for RL Combat Evaluation & Heatmaps
Generates publication-ready figures for thesis and presentation slides:
1. Spatial 2D Heatmap (Player vs. NPC occupancy)
2. Polar Histogram of Multi-Agent Encirclement Angles (0° - 360°)
3. Win-Rate & Damage Efficiency comparison bar plots
4. Time-to-Kill (TTK) and Combat Duration distributions
"""

import os
import glob
import argparse
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

def parse_args():
    parser = argparse.ArgumentParser(description="Generate Thesis Heatmaps and Evaluation Plots")
    parser.add_argument("--eval-dir", type=str, default="EvalResults/Heatmaps", help="Path to heatmap CSVs")
    parser.add_argument("--metrics-dir", type=str, default="EvalResults", help="Path to metrics CSVs")
    parser.add_argument("--output-dir", type=str, default="EvalResults/Plots", help="Directory to save generated figures")
    return parser.parse_args()

def plot_spatial_heatmap(csv_path, title, output_path, cmap="inferno"):
    """Plots a 2D spatial heatmap matrix with arena boundaries and smooth density gradients."""
    if not os.path.exists(csv_path):
        print(f"[Warning] File not found: {csv_path}")
        return

    data = np.loadtxt(csv_path, delimiter=",")
    
    # Check if data is mostly empty or clamped
    if np.sum(data) == 0:
        print(f"[Warning] Heatmap matrix is all zeros: {csv_path}")
        return

    # Use robust percentiles or log1p scaling for rich contrast
    data_scaled = np.log1p(data)

    plt.figure(figsize=(7.5, 6.5), dpi=300)
    sns.set_theme(style="white")
    
    ax = sns.heatmap(
        data_scaled, 
        cmap=cmap, 
        cbar_kws={'label': 'Log Relative Occupancy Density'},
        square=True,
        xticklabels=False,
        yticklabels=False
    )
    
    # Draw Arena boundary box
    plt.axvline(x=0, color='white', linewidth=2.5)
    plt.axvline(x=data.shape[1], color='white', linewidth=2.5)
    plt.axhline(y=0, color='white', linewidth=2.5)
    plt.axhline(y=data.shape[0], color='white', linewidth=2.5)

    plt.title(title, fontsize=13, fontweight='bold', pad=12)
    plt.xlabel("Arena X Axis (West $\\rightarrow$ East)", fontsize=11, fontweight='semibold')
    plt.ylabel("Arena Z Axis (South $\\rightarrow$ North)", fontsize=11, fontweight='semibold')
    
    plt.tight_layout()
    plt.savefig(output_path, dpi=300, bbox_inches='tight')
    plt.close()
    print(f"[Saved] Spatial Heatmap: {output_path}")

def plot_polar_encirclement(csv_path, title, output_path):
    """Plots a 360° Polar Angle Histogram representing multi-agent encirclement distribution."""
    if not os.path.exists(csv_path):
        print(f"[Warning] File not found: {csv_path}")
        return

    df = pd.read_csv(csv_path)
    
    angles_mid = np.radians((df['AngleStartDeg'] + df['AngleEndDeg']) / 2.0)
    counts = df['ObservationCount'].values
    
    plt.figure(figsize=(7, 7), dpi=300)
    ax = plt.subplot(111, projection='polar')
    
    bars = ax.bar(angles_mid, counts, width=np.radians(10), bottom=0.0, color='#E65100', alpha=0.80, edgecolor='#212121', lw=0.8)
    
    ax.set_theta_zero_location('N') # 0° is North (Forward)
    ax.set_theta_direction(-1)      # Clockwise
    
    plt.title(title, fontsize=13, fontweight='bold', pad=22)
    plt.tight_layout()
    plt.savefig(output_path, dpi=300, bbox_inches='tight')
    plt.close()
    print(f"[Saved] Polar Encirclement Plot: {output_path}")

def plot_metrics_summary(metrics_csv, output_path):
    """Plots Win-rate, Duration, and Encirclement distributions from metrics CSV."""
    if not os.path.exists(metrics_csv):
        print(f"[Warning] Metrics CSV not found: {metrics_csv}")
        return

    df = pd.read_csv(metrics_csv)
    if len(df) == 0:
        return

    fig, axes = plt.subplots(2, 2, figsize=(13, 9.5), dpi=300)
    sns.set_theme(style="whitegrid")
    
    # 1. Match Outcomes Pie Chart
    if 'Outcome' in df.columns:
        outcomes = df['Outcome'].value_counts()
        colors = ['#2E7D32' if 'Enemy' in str(k) else '#C62828' for k in outcomes.index]
        axes[0, 0].pie(
            outcomes.values, 
            labels=outcomes.index, 
            autopct='%1.1f%%', 
            colors=colors, 
            startangle=140,
            wedgeprops=dict(edgecolor='#333333', linewidth=1.2)
        )
        axes[0, 0].set_title("(a) Match Outcomes (Win Rate)", fontsize=12, fontweight='bold')
    
    # 2. Combat Duration (TTK) Distribution
    if 'DurationSeconds' in df.columns:
        sns.histplot(df['DurationSeconds'], kde=True, ax=axes[0, 1], color='#1565C0', bins=15)
        axes[0, 1].set_title("(b) Combat Duration (Time-to-Kill)", fontsize=12, fontweight='bold')
        axes[0, 1].set_xlabel("Duration (Seconds)", fontweight='semibold')
    
    # 3. Mean Encirclement Span Distribution
    if 'MeanEncirclementSpanDeg' in df.columns:
        sns.histplot(df['MeanEncirclementSpanDeg'], kde=True, ax=axes[1, 0], color='#6A1B9A', bins=15)
        axes[1, 0].set_title("(c) Multi-Agent Encirclement Span ($E_E$)", fontsize=12, fontweight='bold')
        axes[1, 0].set_xlabel("Mean Encirclement Angle (°)", fontweight='semibold')
    
    # 4. Damage Distribution
    if 'DamageDealt' in df.columns:
        sns.histplot(df['DamageDealt'], kde=True, ax=axes[1, 1], color='#E65100', bins=15)
        axes[1, 1].set_title("(d) Damage Dealt to Player per Episode", fontsize=12, fontweight='bold')
        axes[1, 1].set_xlabel("Damage (HP)", fontweight='semibold')

    plt.suptitle("Quantitative Evaluation Summary Dashboard", fontsize=14, fontweight='bold', y=0.995)
    plt.tight_layout()
    plt.savefig(output_path, dpi=300, bbox_inches='tight')
    plt.close()
    print(f"[Saved] Metrics Summary Dashboard: {output_path}")

def main():
    args = parse_args()
    os.makedirs(args.output_dir, exist_ok=True)
    
    player_csvs = sorted(glob.glob(os.path.join(args.eval_dir, "heatmap_player_*.csv")))
    if player_csvs:
        p_csv = player_csvs[-1] # Plot latest
        basename = os.path.basename(p_csv).replace("heatmap_player_", "").replace(".csv", "")
        out = os.path.join(args.output_dir, f"plot_player_heatmap_{basename}.png")
        plot_spatial_heatmap(p_csv, f"Player Spatial Occupancy ({basename})", out, cmap="viridis")

    enemy_csvs = sorted(glob.glob(os.path.join(args.eval_dir, "heatmap_enemies_*.csv")))
    if enemy_csvs:
        e_csv = enemy_csvs[-1] # Plot latest
        basename = os.path.basename(e_csv).replace("heatmap_enemies_", "").replace(".csv", "")
        out = os.path.join(args.output_dir, f"plot_enemies_heatmap_{basename}.png")
        plot_spatial_heatmap(e_csv, f"Multi-Agent NPC Occupancy ({basename})", out, cmap="inferno")

    polar_csvs = sorted(glob.glob(os.path.join(args.eval_dir, "heatmap_polar_angles_*.csv")))
    if polar_csvs:
        a_csv = polar_csvs[-1] # Plot latest
        basename = os.path.basename(a_csv).replace("heatmap_polar_angles_", "").replace(".csv", "")
        out = os.path.join(args.output_dir, f"plot_polar_encirclement_{basename}.png")
        plot_polar_encirclement(a_csv, f"Encirclement Angular Profile ({basename})", out)

    metrics_csvs = sorted(glob.glob(os.path.join(args.metrics_dir, "metrics_*.csv")))
    if metrics_csvs:
        m_csv = metrics_csvs[-1] # Plot latest
        basename = os.path.basename(m_csv).replace("metrics_", "").replace(".csv", "")
        out = os.path.join(args.output_dir, f"plot_dashboard_{basename}.png")
        plot_metrics_summary(m_csv, out)

    print("\n[Done] All latest evaluation plots generated in:", args.output_dir)

if __name__ == "__main__":
    main()
