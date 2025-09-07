using EstudoApi.Domain.Models;
using EstudoApi.Domain.CQRS.Commands.Account;
using System.Text.Json;
using System.Text;

namespace EstudoApi.Banking.Services;

public interface IMainContaCorrenteApiService
{
    Task<MovementResult> RealizarMovimentacao(string token, AccountMovementCommand command);
    Task<ContaCorrente?> GetContaAsync(int numeroConta);
    Task<bool> PingAsync();
}

public class MovementResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public decimal? FinalBalance { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public static MovementResult CreateSuccess(decimal? finalBalance = null, string? message = null)
    {
        return new MovementResult
        {
            Success = true,
            Message = message,
            FinalBalance = finalBalance
        };
    }

    public static MovementResult CreateFailure(string message, string? errorCode = null)
    {
        return new MovementResult
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

public class ContaCorrenteApiService : IMainContaCorrenteApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContaCorrenteApiService> _logger;
    private readonly string _baseUrl;

    public ContaCorrenteApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ContaCorrenteApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["ApiUrls:ContaCorrenteApi"] ?? throw new ArgumentException("ContaCorrenteApi URL not configured");
    }

    public async Task<MovementResult> RealizarMovimentacao(string token, AccountMovementCommand command)
    {
        try
        {
            _logger.LogInformation("Realizando movimentação {Tipo} na conta {NumeroConta} valor {Valor}",
                command.Tipo, command.NumeroConta, command.Valor);

            // Configurar autenticação
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(command);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/contas/movimentacao", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Movimentação {Tipo} realizada com sucesso na conta {NumeroConta}",
                    command.Tipo, command.NumeroConta);
                return MovementResult.CreateSuccess();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro na movimentação {Tipo} conta {NumeroConta}: {StatusCode} - {Error}",
                command.Tipo, command.NumeroConta, response.StatusCode, errorContent);

            return MovementResult.CreateFailure($"Erro na movimentação: {response.StatusCode}", "MOVEMENT_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar movimentação {Tipo} na conta {NumeroConta}",
                command.Tipo, command.NumeroConta);
            return MovementResult.CreateFailure("Erro de comunicação com a API", "COMMUNICATION_ERROR");
        }
    }

    public async Task<ContaCorrente?> GetContaAsync(int numeroConta)
    {
        try
        {
            _logger.LogInformation("Buscando conta {NumeroConta} na API principal", numeroConta);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/contas/{numeroConta}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var conta = JsonSerializer.Deserialize<ContaCorrente>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Conta {NumeroConta} encontrada com sucesso", numeroConta);
                return conta;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Conta {NumeroConta} não encontrada", numeroConta);
                return null;
            }

            _logger.LogError("Erro ao buscar conta {NumeroConta}: {StatusCode}", numeroConta, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comunicar com a API principal para buscar conta {NumeroConta}", numeroConta);
            return null;
        }
    }

    public async Task<bool> UpdateSaldoAsync(string numeroConta, decimal novoSaldo)
    {
        try
        {
            _logger.LogInformation("Atualizando saldo da conta {NumeroConta} para {NovoSaldo}", numeroConta, novoSaldo);

            var updateRequest = new { Saldo = novoSaldo };
            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/contas/{numeroConta}/saldo", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Saldo da conta {NumeroConta} atualizado com sucesso", numeroConta);
                return true;
            }

            _logger.LogError("Erro ao atualizar saldo da conta {NumeroConta}: {StatusCode}", numeroConta, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comunicar com a API principal para atualizar saldo da conta {NumeroConta}", numeroConta);
            return false;
        }
    }

    public async Task<bool> ValidateContaAsync(string numeroConta)
    {
        try
        {
            _logger.LogInformation("Validando existência da conta {NumeroConta}", numeroConta);

            // Usar o endpoint de ping para testar se a API principal está respondendo primeiro
            var pingResponse = await _httpClient.GetAsync($"{_baseUrl}/api/v1/ping");
            if (!pingResponse.IsSuccessStatusCode)
            {
                _logger.LogError("API principal não está respondendo: {StatusCode}", pingResponse.StatusCode);
                return false;
            }

            // Agora tentar validar a conta
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/contas/{numeroConta}");
            var exists = response.IsSuccessStatusCode;

            _logger.LogInformation("Conta {NumeroConta} {Status} (StatusCode: {StatusCode})",
                numeroConta, exists ? "existe" : "não existe", response.StatusCode);
            return exists;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Erro de rede ao validar conta {NumeroConta}", numeroConta);
            return false;
        }
        catch (TaskCanceledException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout ao validar conta {NumeroConta}", numeroConta);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro geral ao validar conta {NumeroConta}", numeroConta);
            return false;
        }
    }

    public async Task<bool> PingAsync()
    {
        try
        {
            _logger.LogInformation("Testando conectividade com a API");

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ping");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Ping bem-sucedido para a API");
                return true;
            }
            
            _logger.LogWarning("Ping falhou: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (TaskCanceledException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout no ping da API");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no ping da API");
            return false;
        }
    }
}
