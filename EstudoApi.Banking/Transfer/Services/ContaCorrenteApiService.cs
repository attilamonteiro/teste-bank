using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Domain.Models;

namespace EstudoApi.Banking.Transfer.Services
{
    /// <summary>
    /// Serviço para comunicação com a API de Conta Corrente (Transfer)
    /// </summary>
    public interface ITransferContaCorrenteApiService
    {
        Task<MovimentacaoResult> RealizarMovimentacao(string token, AccountMovementCommand command);
        Task<ContaCorrente?> GetContaAsync(string numeroConta);
        Task<bool> UpdateSaldoAsync(string numeroConta, decimal novoSaldo);
        Task<bool> ValidateContaAsync(string numeroConta);
    }

    public class TransferContaCorrenteApiService : ITransferContaCorrenteApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TransferContaCorrenteApiService> _logger;
        private readonly IConfiguration _configuration;

        public TransferContaCorrenteApiService(
            HttpClient httpClient,
            ILogger<TransferContaCorrenteApiService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<MovimentacaoResult> RealizarMovimentacao(string token, AccountMovementCommand command)
        {
            try
            {
                var baseUrl = _configuration["ApiContaCorrente:BaseUrl"]
                              ?? _configuration["ApiUrls:ContaCorrenteApi"]
                              ?? "http://localhost:5041";
                var endpoint = $"{baseUrl}/api/conta/movimentar";

                _logger.LogInformation("Realizando movimentação via API. Endpoint: {Endpoint}, RequisicaoId: {RequisicaoId}",
                    endpoint, command.RequisicaoId);

                var json = JsonSerializer.Serialize(command);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Movimentação realizada com sucesso. RequisicaoId: {RequisicaoId}", command.RequisicaoId);
                    return MovimentacaoResult.Success();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Erro na movimentação. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                // Tentar extrair erro estruturado
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                    return MovimentacaoResult.Failure(
                        errorResponse?.mensagem ?? "Erro na movimentação",
                        errorResponse?.tipo ?? "MOVIMENTACAO_ERROR"
                    );
                }
                catch
                {
                    return MovimentacaoResult.Failure("Erro na comunicação com API de movimentação", "API_ERROR");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de rede ao chamar API de movimentação. RequisicaoId: {RequisicaoId}", command.RequisicaoId);
                return MovimentacaoResult.Failure("Erro de comunicação com API de movimentação", "NETWORK_ERROR");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout ao chamar API de movimentação. RequisicaoId: {RequisicaoId}", command.RequisicaoId);
                return MovimentacaoResult.Failure("Timeout na comunicação com API de movimentação", "TIMEOUT_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao chamar API de movimentação. RequisicaoId: {RequisicaoId}", command.RequisicaoId);
                return MovimentacaoResult.Failure("Erro interno na comunicação", "INTERNAL_ERROR");
            }
        }

        public async Task<ContaCorrente?> GetContaAsync(string numeroConta)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/contas/{numeroConta}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var conta = JsonSerializer.Deserialize<ContaCorrente>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return conta;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar conta {NumeroConta}", numeroConta);
                return null;
            }
        }

        public async Task<bool> UpdateSaldoAsync(string numeroConta, decimal novoSaldo)
        {
            try
            {
                var updateCommand = new { Saldo = novoSaldo };
                var jsonContent = JsonSerializer.Serialize(updateCommand);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/contas/{numeroConta}/saldo", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar saldo da conta {NumeroConta}", numeroConta);
                return false;
            }
        }

        public async Task<bool> ValidateContaAsync(string numeroConta)
        {
            try
            {
                // Primeiro, testar ping da API
                _logger.LogInformation("Testando comunicação com API principal...");
                var pingResponse = await _httpClient.GetAsync("/api/ping");

                if (!pingResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API principal não respondeu ao ping. Status: {StatusCode}", pingResponse.StatusCode);
                    return false;
                }

                _logger.LogInformation("Ping OK. Validando conta {NumeroConta}...", numeroConta);

                // Validar se a conta existe
                var conta = await GetContaAsync(numeroConta);
                var isValid = conta != null;

                _logger.LogInformation("Conta {NumeroConta} é válida: {IsValid}", numeroConta, isValid);
                return isValid;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de HTTP ao validar conta {NumeroConta}", numeroConta);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout ao validar conta {NumeroConta}", numeroConta);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral ao validar conta {NumeroConta}", numeroConta);
                return false;
            }
        }
    }

    public class MovimentacaoResult
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public string? ErrorCode { get; private set; }

        private MovimentacaoResult() { }

        public static MovimentacaoResult Success() => new() { IsSuccess = true };

        public static MovimentacaoResult Failure(string error, string errorCode) => new()
        {
            IsSuccess = false,
            Error = error,
            ErrorCode = errorCode
        };
    }

    internal class ErrorResponse
    {
        public string mensagem { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }
}
