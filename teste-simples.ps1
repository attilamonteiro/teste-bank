Write-Host "=== TESTE END-TO-END SISTEMA BANCÁRIO ===" -ForegroundColor Cyan

# FASE 1: Ping das APIs
Write-Host "`nFASE 1: Testando conectividade" -ForegroundColor Yellow
try {
    $ping1 = Invoke-RestMethod -Uri "http://localhost:5041/api/v1/ping" -Method GET
    Write-Host " API Principal: OK" -ForegroundColor Green
} catch {
    Write-Host " API Principal: FALHA" -ForegroundColor Red
    exit 1
}

# FASE 2: Cadastro de conta
Write-Host "`nFASE 2: Cadastrando conta de teste" -ForegroundColor Yellow
$cadastroBody = '{"Nome":"João Silva","Cpf":"12345678901","Senha":"senha123"}'
try {
    $cadastro = Invoke-RestMethod -Uri "http://localhost:5041/api/auth/conta/cadastrar" -Method POST -Body $cadastroBody -ContentType "application/json"
    $numeroConta = $cadastro.numeroConta
    Write-Host " Conta criada: $numeroConta" -ForegroundColor Green
} catch {
    Write-Host " Erro no cadastro: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# FASE 3: Login
Write-Host "`nFASE 3: Fazendo login" -ForegroundColor Yellow
$loginBody = "{`"numeroConta`":$numeroConta,`"senha`":`"senha123`"}"
try {
    $login = Invoke-RestMethod -Uri "http://localhost:5041/api/auth/conta/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $login.token
    Write-Host " Login OK - Token: $($token.Substring(0,30))..." -ForegroundColor Green
} catch {
    Write-Host " Erro no login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# FASE 4: Consultar saldo inicial
Write-Host "`nFASE 4: Consultando saldo inicial" -ForegroundColor Yellow
$headers = @{"Authorization" = "Bearer $token"}
try {
    $saldo = Invoke-RestMethod -Uri "http://localhost:5041/api/conta/saldo" -Method GET -Headers $headers
    Write-Host " Saldo inicial: R$ $($saldo.saldo)" -ForegroundColor Green
} catch {
    Write-Host " Erro na consulta: $($_.Exception.Message)" -ForegroundColor Red
}

# FASE 5: Depósito
Write-Host "`nFASE 5: Realizando depósito" -ForegroundColor Yellow
$depositoBody = "{`"requisicaoId`":`"$(New-Guid)`",`"numeroConta`":$numeroConta,`"valor`":1000.00,`"tipo`":`"C`"}"
try {
    $deposito = Invoke-RestMethod -Uri "http://localhost:5041/api/conta/movimentar" -Method POST -Body $depositoBody -Headers $headers -ContentType "application/json"
    Write-Host " Depósito realizado: R$ 1000,00" -ForegroundColor Green
} catch {
    Write-Host " Erro no depósito: $($_.Exception.Message)" -ForegroundColor Red
}

# FASE 6: Consultar saldo final
Write-Host "`nFASE 6: Consultando saldo final" -ForegroundColor Yellow
try {
    $saldoFinal = Invoke-RestMethod -Uri "http://localhost:5041/api/conta/saldo" -Method GET -Headers $headers
    Write-Host " Saldo final: R$ $($saldoFinal.saldo)" -ForegroundColor Green
} catch {
    Write-Host " Erro na consulta final: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TESTE CONCLUÍDO ===" -ForegroundColor Cyan
