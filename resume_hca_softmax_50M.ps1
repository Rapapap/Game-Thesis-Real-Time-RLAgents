# PowerShell Resume Runner - HCA Softmax 50M
# Resumes HCA Softmax from step 40,660,000 to reach the full 50,000,000 steps.
#
# Jalankan dari PowerShell di root project:
#   .\resume_hca_softmax_50M.ps1

$env:PYTHONIOENCODING = "utf-8"
$mlagentsPath = "C:\Users\RavaRazan\anaconda3\envs\mlagents\Scripts\mlagents-learn.exe"
$exePath = "C:\Users\RavaRazan\Downloads\Builds-NewBotPlayer\Code Crusader.exe"

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  RESUME TRAINING: HCA Softmax 50M (Sisa ~9.34M Steps)   " -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  Run ID       : HCA_Softmax_50M                          " -ForegroundColor White
Write-Host "  Starting at  : ~40,660,000 steps                        " -ForegroundColor White
Write-Host "  Target       : 50,000,000 steps                         " -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

& $mlagentsPath config/hca/NormalEnemyHCA_NoCurriculum_Softmax_50M.yaml `
    --env=$exePath `
    --run-id=HCA_Softmax_50M `
    --resume `
    --no-graphics `
    --num-envs=8

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [DONE] HCA Softmax 50M Berhasil Diselesaikan Penuh!      " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
