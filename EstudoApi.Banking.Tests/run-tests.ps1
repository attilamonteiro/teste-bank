Write-Host "Testing Banking API..." -ForegroundColor Green
dotnet test --verbosity normal > test-results.txt 2>&1
$exitCode = $LASTEXITCODE
Write-Host "Test exit code: $exitCode" -ForegroundColor Yellow
Get-Content test-results.txt | Select-Object -Last 20
Write-Host "Test completed!" -ForegroundColor Green
