# PowerShell Sequential Runner - v3 Experiments (Low Entropy)
# Runs 3 models sequentially to 10M steps:
#   1. PPO Baseline v3       (reference, same config as v2 - no changes needed)
#   2. HCA Softmax v3        (beta=0.002, linear LR schedule)
#   3. HCA Max v3            (beta=0.002, linear LR schedule)
#
# Perubahan vs v2: beta dikurangi 5x (0.01 -> 0.002) + learning_rate_schedule: linear
# Target: entropy turun dari ~2.37 ke ~1.4-1.7
#
# Jalankan dari root project:
#   .\run_all_experiments_v3.ps1

$env:PYTHONIOENCODING = "utf-8"
$mlagentsPath = "C:\Users\RavaRazan\anaconda3\envs\mlagents\Scripts\mlagents-learn.exe"
$exePath = "C:\Users\RavaRazan\Downloads\Builds-NewBotPlayer\Code Crusader.exe"

Write-Host "" 
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  V3 EXPERIMENT SUITE - LOW ENTROPY HCA  " -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# --- Experiment 1/3: PPO Baseline (same config as v2, re-run for fresh comparison) ---
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host " EXPERIMENT 1/3: PPO Baseline v3 (Ref)   " -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
& $mlagentsPath config/ppo/NormalEnemyCC_NoCurriculum.yaml `
    --env=$exePath `
    --run-id=PPO_NoCurriculum_v3 `
    --no-graphics `
    --num-envs=8 `
    --force

Write-Host ""
Write-Host " [DONE] PPO Baseline v3 selesai." -ForegroundColor Green
Write-Host ""

# --- Experiment 2/3: HCA Softmax v3 (low entropy) ---
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host " EXPERIMENT 2/3: HCA Softmax v3           " -ForegroundColor Green
Write-Host "   beta: 0.01 -> 0.002, LR: linear        " -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Cyan
& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Softmax_v3.yaml `
    --env=$exePath `
    --run-id=HCA_Softmax_v3 `
    --no-graphics `
    --num-envs=8 `
    --force

Write-Host ""
Write-Host " [DONE] HCA Softmax v3 selesai." -ForegroundColor Green
Write-Host ""

# --- Experiment 3/3: HCA Max v3 (low entropy) ---
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host " EXPERIMENT 3/3: HCA Max v3               " -ForegroundColor Green
Write-Host "   beta: 0.01 -> 0.002, LR: linear        " -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Cyan
& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Max_v3.yaml `
    --env=$exePath `
    --run-id=HCA_Max_v3 `
    --no-graphics `
    --num-envs=8 `
    --force

Write-Host ""
Write-Host " [DONE] HCA Max v3 selesai." -ForegroundColor Green
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  SEMUA 3 EKSPERIMEN V3 SELESAI!         " -ForegroundColor Yellow
Write-Host "  Bandingkan hasil di TensorBoard:        " -ForegroundColor White
Write-Host "  tensorboard --logdir=results            " -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Cyan
