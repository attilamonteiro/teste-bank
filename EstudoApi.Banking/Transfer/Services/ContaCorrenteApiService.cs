using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EstudoApi.Domain.CQRS.Commands.Account;

namespace EstudoApi.Banking.Transfer.Services
{
    /// <summary>
    /// Serviço para comunicação com a API de Conta Corrente
    /// </summary>
    public interface IContaCorrenteApiService
    {
        Task<MovimentacaoResult> RealizarMovimentacao(string token, AccountMovementCommand command);
    }

    public class ContaCorrenteApiService : IContaCorrenteApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContaCorrenteApiService> _logger;
        private readonly IConfiguration _configuration;

        public ContaCorrenteApiService(
            HttpClient httpClient,
            ILogger<ContaCorrenteApiService> logger,
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
                var baseUrl = _configuration["ApiContaCorrente:BaseUrl"] ?? "http://api-main";
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
