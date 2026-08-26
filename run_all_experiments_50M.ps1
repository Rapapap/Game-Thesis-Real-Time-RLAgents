# PowerShell Sequential Runner - 50M Convergence Training Experiments
# Runs 3 models sequentially to 50,000,000 steps:
#   1. PPO Baseline 50M       (PPO_NoCurriculum_50M)
#   2. HCA Softmax 50M        (HCA_Softmax_50M)
#   3. HCA Max 50M            (HCA_Max_50M)
#
# Total Steps per model: 50,000,000
# Target: Full asymptotic policy convergence & steady-state value estimation
#
# Jalankan dari PowerShell di root project:
#   .\run_all_experiments_50M.ps1

$env:PYTHONIOENCODING = "utf-8"
$mlagentsPath = "C:\Users\RavaRazan\anaconda3\envs\mlagents\Scripts\mlagents-learn.exe"
$exePath = "C:\Users\RavaRazan\Downloads\Builds-NewBotPlayer\Code Crusader.exe"

Write-Host "" 
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  50 MILLION STEPS (50M) CONVERGENCE TRAINING SUITE      " -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  Total Target Steps : 50,000,000 per model               " -ForegroundColor White
Write-Host "  Parallel Envs      : 8 standalone workers               " -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

# --- Experiment 1/3: PPO Baseline 50M ---
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host " EXPERIMENT 1/3: PPO Baseline (50M Steps)                 " -ForegroundColor Green
Write-Host "===========================================================" -ForegroundColor Cyan
& $mlagentsPath config/ppo/NormalEnemyCC_NoCurriculum_50M.yaml `
    --env=$exePath `
    --run-id=PPO_NoCurriculum_50M `
    --no-graphics `
    --num-envs=8 `
    --force

Write-Host ""
Write-Host " [DONE] PPO Baseline 50M Selesai." -ForegroundColor Green
Write-Host ""

# --- Experiment 2/3: HCA Softmax 50M ---
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host " EXPERIMENT 2/3: HCA Softmax (50M Steps)                  " -ForegroundColor Green
Write-Host "===========================================================" -ForegroundColor Cyan
& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Softmax_50M.yaml `
    --env=$exePath `
    --run-id=HCA_Softmax_50M `
    --no-graphics `
    --num-envs=8 `
    --force

Write-Host ""
Write-Host " [DONE] HCA Softmax 50M Selesai." -ForegroundColor Green
Write-Host ""

# --- Experiment 3/3: HCA Max 50M ---
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host " EXPERIMENT 3/3: HCA Max (50M Steps - RLHC Canonical)     " -ForegroundColor Green
Write-Host "===========================================================" -ForegroundColor Cyan
& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Max_50M.yaml `
    --env=$exePath `
    --run-id=HCA_Max_50M `
    --no-graphics `
    --num-envs=8 `
    --force

Write-Host ""
Write-Host " [DONE] HCA Max 50M Selesai." -ForegroundColor Green
Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  SELURUH 3 MODEL 50M TRAINING SELESAI!                   " -ForegroundColor Yellow
Write-Host "  Buka TensorBoard untuk memantau grafik konvergensi:    " -ForegroundColor White
Write-Host "  tensorboard --logdir=results                            " -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
