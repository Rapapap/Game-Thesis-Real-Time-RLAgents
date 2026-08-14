# PowerShell Sequential Runner for 3 Experiments (PPO, HCA Softmax, HCA Max)
# Runs each model sequentially to 10M steps without manual intervention.

$env:PYTHONIOENCODING="utf-8"
$mlagentsPath = "C:\Users\RavaRazan\anaconda3\envs\mlagents\Scripts\mlagents-learn.exe"
$exePath = "Builds/Code Crusader.exe"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "STARTING EXPERIMENT 1/3: PPO Baseline" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
& $mlagentsPath config/ppo/NormalEnemyCC_NoCurriculum.yaml --env=$exePath --run-id=PPO_NoCurriculum_v2 --no-graphics --num-envs=8 --force

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "STARTING EXPERIMENT 2/3: HCA Softmax" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Softmax.yaml --env=$exePath --run-id=HCA_Softmax_v2 --no-graphics --num-envs=8 --force

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "STARTING EXPERIMENT 3/3: HCA Max" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Max.yaml --env=$exePath --run-id=HCA_Max_v2 --no-graphics --num-envs=8 --force

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "ALL 3 EXPERIMENTS COMPLETED SUCCESSFULLY!" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
