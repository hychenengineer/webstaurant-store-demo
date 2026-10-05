# Start WebstaurantStore IDS Demo (Backend + Frontend)
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Starting WebstaurantStore Mini-IDS Demo Pipeline..." -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

$root = $PSScriptRoot

Write-Host "`n[1/2] Launching .NET 8 Backend API on http://localhost:5067 ..." -ForegroundColor Cyan
$backendProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "$root\backend" -PassThru

Write-Host "[2/2] Launching React Vite Frontend on http://localhost:5173 ..." -ForegroundColor Cyan
$frontendProc = Start-Process -FilePath "npm" -ArgumentList "run dev" -WorkingDirectory "$root\frontend" -PassThru

Start-Sleep -Seconds 3
Start-Process "http://localhost:5173"

Write-Host "`n✓ Demo is now running!" -ForegroundColor Green
Write-Host "Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "Backend API: http://localhost:5067" -ForegroundColor White
Write-Host "`nPress Ctrl+C or close this window when done to stop processes." -ForegroundColor Yellow

try {
    Wait-Process -Id $backendProc.Id, $frontendProc.Id
} finally {
    Stop-Process -Id $backendProc.Id -ErrorAction SilentlyContinue
    Stop-Process -Id $frontendProc.Id -ErrorAction SilentlyContinue
}
