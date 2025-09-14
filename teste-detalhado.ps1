#!/usr/bin/env pwsh
# Teste detalhado com captura de erro

Write-Host "=== TESTE DETALHADO - CAPTURA DE ERRO ===" -ForegroundColor Cyan

try {
    # Gerar CPF único para o teste
    $timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $uniqueCpf = "987654321" + ($timestamp % 100).ToString("00")
    Write-Host "Usando CPF: $uniqueCpf" -ForegroundColor Cyan

    # 0. Cadastro da conta primeiro
    Write-Host "`n[0] Criando conta..." -ForegroundColor Yellow
    $cadastroBody = @{
        cpf = $uniqueCpf
        senha = "123456"
    } | ConvertTo-Json
    
    $cadastroResponse = Invoke-WebRequest -Uri "http://localhost:5041/api/conta-bancaria/cadastrar" -Method POST -Body $cadastroBody -ContentType "application/json" -UseBasicParsing
    
    if ($cadastroResponse.StatusCode -eq 200) {
        Write-Host "✅ Cadastro OK" -ForegroundColor Green
        $cadastroData = $cadastroResponse.Content | ConvertFrom-Json
        $numeroConta = $cadastroData.numeroConta
        Write-Host "Número da conta: $numeroConta" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Cadastro falhou: $($cadastroResponse.StatusCode)" -ForegroundColor Red
        exit 1
    }

    # 1. Login
    Write-Host "`n[1] Fazendo login..." -ForegroundColor Yellow
    $loginBody = @{
        cpf = $uniqueCpf
        senha = "123456"
    } | ConvertTo-Json
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5041/api/conta-bancaria/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing
    
    if ($loginResponse.StatusCode -eq 200) {
        Write-Host "✅ Login OK" -ForegroundColor Green
        $loginData = $loginResponse.Content | ConvertFrom-Json
        $jwt = $loginData.token
        Write-Host "JWT: $($jwt.Substring(0,30))..." -ForegroundColor Cyan
    } else {
        Write-Host "❌ Login falhou: $($loginResponse.StatusCode)" -ForegroundColor Red
        exit 1
    }

    # 2. Transferência com captura detalhada de erro
    Write-Host "`n[2] Testando transferência com captura de erro..." -ForegroundColor Yellow
    $transferBody = @{
        requisicaoId = [System.Guid]::NewGuid().ToString()
        contaOrigem = $numeroConta
        contaDestino = 3
        valor = 25.00
    } | ConvertTo-Json

    $headers = @{
        "Authorization" = "Bearer $jwt"
        "Content-Type" = "application/json"
    }

    Write-Host "Body da requisição:" -ForegroundColor Gray
    Write-Host $transferBody -ForegroundColor Gray

    try {
        $transferResponse = Invoke-WebRequest -Uri "http://localhost:60016/api/banking/transfer" -Method POST -Body $transferBody -Headers $headers -UseBasicParsing
        
        if ($transferResponse.StatusCode -eq 200) {
            Write-Host "🎉 TRANSFERÊNCIA SUCESSO!" -ForegroundColor Green
            Write-Host "Resposta: $($transferResponse.Content)" -ForegroundColor Cyan
        } else {
            Write-Host "❌ Status inesperado: $($transferResponse.StatusCode)" -ForegroundColor Red
            Write-Host "Resposta: $($transferResponse.Content)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ ERRO HTTP CAPTURADO:" -ForegroundColor Red
        $errorResponse = $_.Exception.Response
        if ($errorResponse) {
            Write-Host "Status Code: $($errorResponse.StatusCode.value__)" -ForegroundColor Red
            Write-Host "Status Description: $($errorResponse.StatusDescription)" -ForegroundColor Red
            
            # Capturar corpo da resposta de erro
            try {
                $reader = New-Object System.IO.StreamReader($errorResponse.GetResponseStream())
                $errorBody = $reader.ReadToEnd()
                $reader.Close()
                
                Write-Host "`nCorpo da resposta de erro:" -ForegroundColor Yellow
                Write-Host $errorBody -ForegroundColor Yellow
                
                # Tentar parsear JSON se possível
                try {
                    $errorJson = $errorBody | ConvertFrom-Json
                    Write-Host "`nErro estruturado:" -ForegroundColor Cyan
                    Write-Host "Mensagem: $($errorJson.mensagem)" -ForegroundColor Cyan
                    Write-Host "Tipo: $($errorJson.tipo)" -ForegroundColor Cyan
                } catch {
                    Write-Host "Erro não é JSON válido" -ForegroundColor Gray
                }
            } catch {
                Write-Host "Não foi possível ler o corpo da resposta de erro" -ForegroundColor Gray
            }
        } else {
            Write-Host "Erro sem resposta HTTP: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

} catch {
    Write-Host "❌ ERRO GERAL: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== FIM DO TESTE DETALHADO ===" -ForegroundColor Cyan
