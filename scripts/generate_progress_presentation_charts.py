import os
import glob
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
from tensorboard.backend.event_processing.event_accumulator import EventAccumulator

# Configure Matplotlib styling for high-quality slide presentations
plt.rcParams['font.family'] = 'sans-serif'
plt.rcParams['font.sans-serif'] = ['DejaVu Sans', 'Arial', 'Helvetica']
plt.rcParams['axes.edgecolor'] = '#CCCCCC'
plt.rcParams['axes.linewidth'] = 1.0
plt.rcParams['grid.color'] = '#EAEAEA'
plt.rcParams['grid.linestyle'] = '--'
plt.rcParams['grid.alpha'] = 0.7

RESULTS_DIR = r"C:\Users\RavaRazan\Downloads\Research Rava\Game-Thesis-Real-Time-RLAgents\results"
OUTPUT_DIR = r"C:\Users\RavaRazan\Downloads\Research Rava\Game-Thesis-Real-Time-RLAgents\presentation_charts"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Curated palette for clear differentiation in presentations
COLORS = {
    'PPO_v1': '#E57373',
    'HCA_Softmax_v1': '#FFB74D',
    'HCA_Max_v1': '#BA68C8',
    'PPO_v2': '#D32F2F',
    'HCA_Softmax_v2': '#F57C00',
    'HCA_Max_v2': '#7B1FA2',
    'PPO_v3': '#E91E63',        # Magenta/Pink
    'HCA_Softmax_v3': '#FF9800', # Warm Orange/Yellow
    'HCA_Max_v3': '#673AB7',     # Deep Purple
}

LABELS = {
    'PPO_v3': 'PPO Baseline (v3)',
    'HCA_Softmax_v3': 'HCA Softmax (v3 - Low Entropy)',
    'HCA_Max_v3': 'HCA Max (v3 - RLHC Canonical)',
    'PPO_v2': 'PPO Baseline (v2)',
    'HCA_Softmax_v2': 'HCA Softmax (v2)',
    'HCA_Max_v2': 'HCA Max (v2)',
    'PPO_v1': 'PPO Baseline (v1)',
    'HCA_Softmax_v1': 'HCA Softmax (v1)',
    'HCA_Max_v1': 'HCA Max (v1)',
}

def extract_scalar_events(run_name, tag_map):
    """
    Extracts scalar time-series from all event files in a run directory.
    """
    run_dir = os.path.join(RESULTS_DIR, run_name, 'NormalEnemy')
    if not os.path.isdir(run_dir):
        return {}
    
    event_files = sorted(glob.glob(os.path.join(run_dir, "events.out.tfevents.*")))
    if not event_files:
        return {}
    
    data = {}
    for ef in event_files:
        try:
            ea = EventAccumulator(ef, size_guidance={'scalars': 0})
            ea.Reload()
            available_tags = ea.Tags().get('scalars', [])
            
            for key, tb_tag in tag_map.items():
                if tb_tag in available_tags:
                    events = ea.Scalars(tb_tag)
                    steps = [e.step for e in events]
                    values = [e.value for e in events]
                    wall_times = [e.wall_time for e in events]
                    
                    df = pd.DataFrame({'step': steps, 'value': values, 'wall_time': wall_times})
                    if key not in data:
                        data[key] = df
                    else:
                        data[key] = pd.concat([data[key], df]).drop_duplicates(subset=['step']).sort_values('step')
        except Exception as e:
            print(f"Error reading {ef}: {e}")
            
    return data

def exponential_moving_average(series, alpha=0.08):
    return series.ewm(alpha=alpha).mean()

def load_all_experiments():
    tag_map = {
        'reward': 'Environment/Cumulative Reward',
        'entropy': 'Policy/Entropy',
        'policy_loss': 'Losses/Policy Loss',
        'value_loss': 'Losses/Value Loss',
        'extrinsic_reward': 'Policy/Extrinsic Reward'
    }
    
    runs = {
        'PPO_v3': 'PPO_NoCurriculum_v3',
        'HCA_Softmax_v3': 'HCA_Softmax_v3',
        'HCA_Max_v3': 'HCA_Max_v3',
        'PPO_v2': 'PPO_NoCurriculum_v2',
        'HCA_Softmax_v2': 'HCA_Softmax_v2',
        'HCA_Max_v2': 'HCA_Max_v2',
        'PPO_v1': 'PPO_NoCurriculum_Parallel8_v1',
        'HCA_Softmax_v1': 'HCA_Softmax_Parallel8_v1',
        'HCA_Max_v1': 'HCA_Max_Parallel8_v1',
    }
    
    all_data = {}
    for alias, run_name in runs.items():
        extracted = extract_scalar_events(run_name, tag_map)
        if extracted:
            all_data[alias] = extracted
            print(f"[Loaded] {alias} ({run_name}) -> {list(extracted.keys())}")
        else:
            print(f"[Warning] No data found for {alias} ({run_name})")
            
    return all_data

def plot_v3_head_to_head(all_data):
    """
    Slide 1: V3 Head-to-Head Comparison (Reward & Entropy)
    """
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(15, 6), dpi=300)
    fig.patch.set_facecolor('#FFFFFF')
    
    v3_keys = ['PPO_v3', 'HCA_Softmax_v3', 'HCA_Max_v3']
    
    # 1. Cumulative Reward
    ax1.set_title("A. Akumulasi Reward Lingkungan (v3 - 10M Steps)", fontsize=13, fontweight='bold', pad=12, color='#212121')
    for k in v3_keys:
        if k in all_data and 'reward' in all_data[k]:
            df = all_data[k]['reward']
            color = COLORS[k]
            label = LABELS[k]
            # Raw faint
            ax1.plot(df['step'] / 1e6, df['value'], color=color, alpha=0.18, linewidth=0.8)
            # Smoothed
            smoothed = exponential_moving_average(df['value'], alpha=0.06)
            ax1.plot(df['step'] / 1e6, smoothed, color=color, linewidth=2.4, label=label)
            
    ax1.set_xlabel("Langkah Pelatihan (Juta Steps)", fontsize=11, fontweight='semibold', labelpad=8)
    ax1.set_ylabel("Cumulative Reward", fontsize=11, fontweight='semibold', labelpad=8)
    ax1.grid(True)
    ax1.set_xlim(0, 10.05)
    ax1.legend(frameon=True, facecolor='#FFFFFF', edgecolor='#CCCCCC', fontsize=9.5, loc='lower right')
    
    # 2. Policy Entropy
    ax2.set_title("B. Konvergensi Entropi Kebijakan (v3 - Low Entropy)", fontsize=13, fontweight='bold', pad=12, color='#212121')
    for k in v3_keys:
        if k in all_data and 'entropy' in all_data[k]:
            df = all_data[k]['entropy']
            color = COLORS[k]
            label = LABELS[k]
            # Raw faint
            ax2.plot(df['step'] / 1e6, df['value'], color=color, alpha=0.18, linewidth=0.8)
            # Smoothed
            smoothed = exponential_moving_average(df['value'], alpha=0.06)
            ax2.plot(df['step'] / 1e6, smoothed, color=color, linewidth=2.4, label=label)
            
    ax2.set_xlabel("Langkah Pelatihan (Juta Steps)", fontsize=11, fontweight='semibold', labelpad=8)
    ax2.set_ylabel("Policy Entropy (Exploration Level)", fontsize=11, fontweight='semibold', labelpad=8)
    ax2.grid(True)
    ax2.set_xlim(0, 10.05)
    ax2.legend(frameon=True, facecolor='#FFFFFF', edgecolor='#CCCCCC', fontsize=9.5, loc='upper right')
    
    # Annotation highlight
    ax2.annotate('HCA Sukses Konvergen\nke ~1.50 (Deterministik)', 
                 xy=(9.8, 1.51), xytext=(6.5, 1.9),
                 arrowprops=dict(facecolor='#673AB7', shrink=0.05, width=1.5, headwidth=7),
                 fontsize=10, fontweight='bold', color='#4A148C',
                 bbox=dict(boxstyle="round,pad=0.4", fc="#EDE7F6", ec="#BA68C8", lw=1.2))

    plt.tight_layout()
    out_path = os.path.join(OUTPUT_DIR, "01_v3_benchmark_reward_entropy.png")
    plt.savefig(out_path, bbox_inches='tight')
    plt.close()
    print(f"[Saved] {out_path}")

def plot_v1_v2_v3_evolution(all_data):
    """
    Slide 2: Evolusi dari v1 -> v2 -> v3 (Milestone Kemajuan)
    """
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(16, 6), dpi=300)
    fig.patch.set_facecolor('#FFFFFF')
    
    # Entropy evolution in HCA
    ax1.set_title("A. Evolusi Penurunan Entropi HCA (v1 vs v2 vs v3)", fontsize=13, fontweight='bold', pad=12, color='#212121')
    
    hca_runs = [
        ('HCA_Softmax_v1', '#FFB74D', 'v1 Softmax (High Entropy ~2.4)'),
        ('HCA_Softmax_v2', '#F57C00', 'v2 Softmax (Stagnant ~2.37)'),
        ('HCA_Softmax_v3', '#E65100', 'v3 Softmax (Resolved -> 1.50)'),
        ('HCA_Max_v1', '#CE93D8', 'v1 Max (High Entropy ~2.4)'),
        ('HCA_Max_v2', '#8E24AA', 'v2 Max (Stagnant ~2.35)'),
        ('HCA_Max_v3', '#4A148C', 'v3 Max (Resolved -> 1.52)'),
    ]
    
    for key, color, label in hca_runs:
        if key in all_data and 'entropy' in all_data[key]:
            df = all_data[key]['entropy']
            smoothed = exponential_moving_average(df['value'], alpha=0.08)
            linewidth = 2.5 if 'v3' in key else 1.6
            linestyle = '-' if 'v3' in key else ('--' if 'v2' in key else ':')
            ax1.plot(df['step'] / 1e6, smoothed, color=color, linewidth=linewidth, linestyle=linestyle, label=label)
            
    ax1.set_xlabel("Langkah Pelatihan (Juta Steps)", fontsize=11, fontweight='semibold', labelpad=8)
    ax1.set_ylabel("Policy Entropy", fontsize=11, fontweight='semibold', labelpad=8)
    ax1.grid(True)
    ax1.legend(frameon=True, facecolor='#FFFFFF', edgecolor='#CCCCCC', fontsize=9, loc='upper right')
    
    # Reward comparison across v1, v2, v3
    ax2.set_title("B. Perbandingan Akumulasi Reward per Generasi", fontsize=13, fontweight='bold', pad=12, color='#212121')
    
    comparison_runs = [
        ('PPO_v1', '#EF9A9A', 'PPO v1', ':'),
        ('PPO_v2', '#E53935', 'PPO v2', '--'),
        ('PPO_v3', '#880E4F', 'PPO v3 (Baseline)', '-'),
        ('HCA_Max_v1', '#CE93D8', 'HCA Max v1', ':'),
        ('HCA_Max_v2', '#8E24AA', 'HCA Max v2', '--'),
        ('HCA_Max_v3', '#673AB7', 'HCA Max v3', '-'),
    ]
    
    for key, color, label, ls in comparison_runs:
        if key in all_data and 'reward' in all_data[key]:
            df = all_data[key]['reward']
            smoothed = exponential_moving_average(df['value'], alpha=0.08)
            lw = 2.4 if 'v3' in key else 1.5
            ax2.plot(df['step'] / 1e6, smoothed, color=color, linewidth=lw, linestyle=ls, label=label)
            
    ax2.set_xlabel("Langkah Pelatihan (Juta Steps)", fontsize=11, fontweight='semibold', labelpad=8)
    ax2.set_ylabel("Cumulative Reward", fontsize=11, fontweight='semibold', labelpad=8)
    ax2.grid(True)
    ax2.legend(frameon=True, facecolor='#FFFFFF', edgecolor='#CCCCCC', fontsize=9, loc='lower right')
    
    plt.tight_layout()
    out_path = os.path.join(OUTPUT_DIR, "02_v1_vs_v2_vs_v3_evolution_hca.png")
    plt.savefig(out_path, bbox_inches='tight')
    plt.close()
    print(f"[Saved] {out_path}")

def plot_v3_comprehensive_dashboard(all_data):
    """
    Slide 3: 4-Quadrant Comprehensive Dashboard for V3 (Reward, Entropy, Value Loss, Policy Loss)
    """
    fig, axs = plt.subplots(2, 2, figsize=(16, 10), dpi=300)
    fig.patch.set_facecolor('#FFFFFF')
    
    v3_keys = ['PPO_v3', 'HCA_Softmax_v3', 'HCA_Max_v3']
    
    # 1. Cumulative Reward
    axs[0, 0].set_title("1. Akumulasi Reward (Environment/Cumulative Reward)", fontsize=12, fontweight='bold', color='#212121')
    for k in v3_keys:
        if k in all_data and 'reward' in all_data[k]:
            df = all_data[k]['reward']
            axs[0, 0].plot(df['step'] / 1e6, exponential_moving_average(df['value'], 0.06), color=COLORS[k], linewidth=2.2, label=LABELS[k])
    axs[0, 0].set_ylabel("Reward", fontweight='semibold')
    axs[0, 0].grid(True)
    axs[0, 0].legend(fontsize=9, loc='lower right')
    
    # 2. Entropy
    axs[0, 1].set_title("2. Entropi Kebijakan (Policy/Entropy)", fontsize=12, fontweight='bold', color='#212121')
    for k in v3_keys:
        if k in all_data and 'entropy' in all_data[k]:
            df = all_data[k]['entropy']
            axs[0, 1].plot(df['step'] / 1e6, exponential_moving_average(df['value'], 0.06), color=COLORS[k], linewidth=2.2, label=LABELS[k])
    axs[0, 1].set_ylabel("Entropy (Nats)", fontweight='semibold')
    axs[0, 1].grid(True)
    axs[0, 1].legend(fontsize=9, loc='upper right')
    
    # 3. Value Loss
    axs[1, 0].set_title("3. Kritik Value Loss (Losses/Value Loss)", fontsize=12, fontweight='bold', color='#212121')
    for k in v3_keys:
        if k in all_data and 'value_loss' in all_data[k]:
            df = all_data[k]['value_loss']
            axs[1, 0].plot(df['step'] / 1e6, exponential_moving_average(df['value'], 0.06), color=COLORS[k], linewidth=2.0, label=LABELS[k])
    axs[1, 0].set_ylabel("Value Loss", fontweight='semibold')
    axs[1, 0].set_xlabel("Juta Steps", fontweight='semibold')
    axs[1, 0].grid(True)
    axs[1, 0].legend(fontsize=9, loc='upper right')
    
    # 4. Policy Loss
    axs[1, 1].set_title("4. Policy Gradient Loss (Losses/Policy Loss)", fontsize=12, fontweight='bold', color='#212121')
    for k in v3_keys:
        if k in all_data and 'policy_loss' in all_data[k]:
            df = all_data[k]['policy_loss']
            axs[1, 1].plot(df['step'] / 1e6, exponential_moving_average(df['value'].abs(), 0.06), color=COLORS[k], linewidth=2.0, label=LABELS[k])
    axs[1, 1].set_ylabel("Policy Loss (|L_clip|)", fontweight='semibold')
    axs[1, 1].set_xlabel("Juta Steps", fontweight='semibold')
    axs[1, 1].grid(True)
    axs[1, 1].legend(fontsize=9, loc='upper right')
    
    plt.suptitle("Ringkasan Pelatihan Eksperimen V3 (10 Juta Steps) — PPO vs HCA", fontsize=15, fontweight='bold', y=0.995, color='#0D47A1')
    plt.tight_layout()
    out_path = os.path.join(OUTPUT_DIR, "03_v3_comprehensive_dashboard.png")
    plt.savefig(out_path, bbox_inches='tight')
    plt.close()
    print(f"[Saved] {out_path}")

def generate_summary_table_presentation(all_data):
    """
    Generates a structured comparison table CSV and a publication-ready graphic table image for slides.
    """
    rows = []
    
    key_order = ['PPO_v3', 'HCA_Max_v3', 'HCA_Softmax_v3', 
                 'PPO_v2', 'HCA_Max_v2', 'HCA_Softmax_v2',
                 'PPO_v1', 'HCA_Max_v1', 'HCA_Softmax_v1']
    
    for k in key_order:
        if k not in all_data:
            continue
        d = all_data[k]
        
        row = {'Model / Iteration': LABELS.get(k, k)}
        
        # Max steps
        if 'reward' in d:
            row['Total Steps'] = f"{int(d['reward']['step'].max()):,}"
            row['Final Reward (Raw)'] = f"{d['reward']['value'].iloc[-1]:.2f}"
            row['Final Reward (Smoothed)'] = f"{exponential_moving_average(d['reward']['value'], 0.06).iloc[-1]:.2f}"
            row['Peak Reward'] = f"{d['reward']['value'].max():.2f}"
        else:
            row['Total Steps'] = 'N/A'
            row['Final Reward (Raw)'] = 'N/A'
            row['Final Reward (Smoothed)'] = 'N/A'
            row['Peak Reward'] = 'N/A'
            
        if 'entropy' in d:
            row['Final Entropy'] = f"{exponential_moving_average(d['entropy']['value'], 0.06).iloc[-1]:.3f}"
        else:
            row['Final Entropy'] = 'N/A'
            
        rows.append(row)
        
    df_summary = pd.DataFrame(rows)
    csv_path = os.path.join(OUTPUT_DIR, "training_summary_table_v1_v3.csv")
    df_summary.to_csv(csv_path, index=False)
    print(f"[Saved] {csv_path}")
    
    # Render table image
    fig, ax = plt.subplots(figsize=(13, len(rows) * 0.65 + 1.2), dpi=300)
    fig.patch.set_facecolor('#FFFFFF')
    ax.axis('off')
    
    table = ax.table(
        cellText=df_summary.values,
        colLabels=df_summary.columns,
        loc='center',
        cellLoc='center'
    )
    table.auto_set_font_size(False)
    table.set_fontsize(10)
    table.scale(1.2, 1.8)
    
    # Styling headers and rows
    for (r, c), cell in table.get_celld().items():
        cell.set_edgecolor('#E0E0E0')
        if r == 0:
            cell.set_facecolor('#1565C0')
            cell.set_text_props(color='white', fontweight='bold')
        elif r in [1, 2, 3]:  # v3 rows
            cell.set_facecolor('#E8F5E9' if c == 0 else '#F1F8E9')
            if c == 0:
                cell.set_text_props(fontweight='bold', color='#1B5E20')
        elif r in [4, 5, 6]:  # v2 rows
            cell.set_facecolor('#FFF3E0' if c == 0 else '#FFF8E1')
        else:
            cell.set_facecolor('#FAFAFA')
            
    plt.title("Tabel Rekapitulasi Metrik Training Lintas Versi (v1 - v3)", fontsize=13, fontweight='bold', pad=20, color='#0D47A1')
    table_img_path = os.path.join(OUTPUT_DIR, "04_training_summary_table.png")
    plt.savefig(table_img_path, bbox_inches='tight')
    plt.close()
    print(f"[Saved] {table_img_path}")

if __name__ == "__main__":
    print("Extracting TensorBoard logs and generating progress presentation graphics...")
    data = load_all_experiments()
    plot_v3_head_to_head(data)
    plot_v1_v2_v3_evolution(data)
    plot_v3_comprehensive_dashboard(data)
    generate_summary_table_presentation(data)
    print("All progress presentation charts generated successfully!")
