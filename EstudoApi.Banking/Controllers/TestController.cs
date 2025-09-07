using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstudoApi.Banking.Transfer.Services;

namespace EstudoApi.Banking.Controllers
{
    /// <summary>
    /// Controller para testar comunicação entre APIs
    /// </summary>
    [ApiController]
    [Route("api/banking/test")]
    public class TestController : ControllerBase
    {
        private readonly ITransferContaCorrenteApiService _contaApiService;
        private readonly ILogger<TestController> _logger;

        public TestController(ITransferContaCorrenteApiService contaApiService, ILogger<TestController> logger)
        {
            _contaApiService = contaApiService;
            _logger = logger;
        }

        /// <summary>
        /// Testa a comunicação com a API principal
        /// </summary>
        [HttpGet("ping")]
        public async Task<IActionResult> TestPing()
        {
            try
            {
                _logger.LogInformation("Testando comunicação com API principal");

                // Fazer uma chamada simples para a API principal
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("http://localhost:5041/api/v1/ping");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Comunicação com API principal bem-sucedida");

                    return Ok(new
                    {
                        message = "Comunicação entre APIs funcionando!",
                        apiPrincipalResponse = content,
                        timestamp = DateTime.Now
                    });
                }

                _logger.LogError("Falha na comunicação: {StatusCode}", response.StatusCode);
                return BadRequest(new
                {
                    message = "Falha na comunicação com API principal",
                    statusCode = response.StatusCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar comunicação");
                return StatusCode(500, new
                {
                    message = "Erro interno ao testar comunicação",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Testa validação de conta via API principal
        /// </summary>
        [HttpGet("conta/{numero}")]
        [Authorize]
        public async Task<IActionResult> TestValidarConta(int numero)
        {
            try
            {
                // Validar número da conta
                if (numero <= 0)
                {
                    _logger.LogWarning("Número de conta inválido: {Numero}", numero);
                    return BadRequest(new
                    {
                        message = "Número da conta deve ser positivo",
                        numero = numero
                    });
                }

                _logger.LogInformation("Testando validação da conta {Numero}", numero);

                var existe = await _contaApiService.ValidateContaAsync(numero.ToString());

                return Ok(new
                {
                    numeroConta = numero,
                    existe = existe,
                    timestamp = DateTime.Now,
                    message = existe ? "Conta encontrada!" : "Conta não encontrada"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar conta {Numero}", numero);
                return StatusCode(500, new
                {
                    message = "Erro ao validar conta",
                    error = ex.Message
                });
            }
        }
    }
}
