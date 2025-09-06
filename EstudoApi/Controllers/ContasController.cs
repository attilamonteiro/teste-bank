using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;

namespace EstudoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContasController : ControllerBase
    {
        private readonly IContaCorrenteRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;

        public ContasController(
            IContaCorrenteRepository contaRepository,
            IMovimentoRepository movimentoRepository)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
        }

        /// <summary>
        /// Buscar conta corrente por número
        /// </summary>
        [HttpGet("{numero}")]
        public async Task<ActionResult<ContaCorrenteResponse>> GetConta(int numero)
        {
            var conta = await _contaRepository.GetByNumeroAsync(numero);
            if (conta == null)
                return NotFound($"Conta {numero} não encontrada");

            var saldo = await _movimentoRepository.GetSaldoAtualAsync(conta.IdContaCorrente);

            return Ok(new ContaCorrenteResponse
            {
                IdContaCorrente = conta.IdContaCorrente,
                Numero = conta.Numero,
                Nome = conta.Nome,
                Ativo = conta.Ativo == 1,
                Saldo = saldo
            });
        }

        /// <summary>
        /// Buscar saldo da conta
        /// </summary>
        [HttpGet("{numero}/saldo")]
        public async Task<ActionResult<SaldoResponse>> GetSaldo(int numero)
        {
            var conta = await _contaRepository.GetByNumeroAsync(numero);
            if (conta == null)
                return NotFound($"Conta {numero} não encontrada");

            var saldo = await _movimentoRepository.GetSaldoAtualAsync(conta.IdContaCorrente);

            return Ok(new SaldoResponse
            {
                Numero = numero,
                Nome = conta.Nome,
                Saldo = saldo,
                DataConsulta = DateTime.Now
            });
        }

        /// <summary>
        /// Listar movimentos da conta
        /// </summary>
        [HttpGet("{numero}/movimentos")]
        public async Task<ActionResult<IEnumerable<MovimentoResponse>>> GetMovimentos(
            int numero,
            [FromQuery] int limite = 10)
        {
            var conta = await _contaRepository.GetByNumeroAsync(numero);
            if (conta == null)
                return NotFound($"Conta {numero} não encontrada");

            var movimentos = await _movimentoRepository.GetUltimosMovimentosAsync(
                conta.IdContaCorrente, limite);

            var response = movimentos.Select(m => new MovimentoResponse
            {
                IdMovimento = m.IdMovimento,
                DataMovimento = m.DataMovimento,
                TipoMovimento = m.TipoMovimento,
                Valor = m.Valor,
                DescricaoTipo = m.TipoMovimento == "C" ? "Crédito" : "Débito"
            });

            return Ok(response);
        }

        /// <summary>
        /// Criar nova conta corrente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ContaCorrenteResponse>> CriarConta(
            [FromBody] CriarContaRequest request)
        {
            // Verificar se já existe conta com este número
            if (await _contaRepository.ExisteContaAsync(request.Numero))
                return BadRequest($"Já existe conta com número {request.Numero}");

            var conta = new ContaCorrente
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Numero = request.Numero,
                Nome = request.Nome,
                Ativo = 1,
                Senha = request.Senha, // Em produção, usar hash
                Salt = "salt_placeholder" // Em produção, gerar salt aleatório
            };

            await _contaRepository.AddAsync(conta);

            return CreatedAtAction(nameof(GetConta), new { numero = conta.Numero },
                new ContaCorrenteResponse
                {
                    IdContaCorrente = conta.IdContaCorrente,
                    Numero = conta.Numero,
                    Nome = conta.Nome,
                    Ativo = true,
                    Saldo = 0
                });
        }

        /// <summary>
        /// Fazer depósito na conta
        /// </summary>
        [HttpPost("{numero}/deposito")]
        public async Task<ActionResult<MovimentoResponse>> FazerDeposito(
            int numero,
            [FromBody] DepositoRequest request)
        {
            var conta = await _contaRepository.GetByNumeroAsync(numero);
            if (conta == null)
                return NotFound($"Conta {numero} não encontrada");

            if (request.Valor <= 0)
                return BadRequest("Valor do depósito deve ser maior que zero");

            await _movimentoRepository.AddCreditoAsync(conta.IdContaCorrente, request.Valor);

            var novoSaldo = await _movimentoRepository.GetSaldoAtualAsync(conta.IdContaCorrente);

            return Ok(new MovimentoResponse
            {
                IdMovimento = Guid.NewGuid().ToString(),
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                TipoMovimento = "C",
                Valor = request.Valor,
                DescricaoTipo = "Depósito",
                SaldoAtual = novoSaldo
            });
        }

        /// <summary>
        /// Realizar movimentação (débito ou crédito) - Endpoint para comunicação entre APIs
        /// </summary>
        [HttpPost("movimentacao")]
        public async Task<ActionResult<MovimentoResponse>> RealizarMovimentacao(
            [FromBody] MovimentacaoRequest request)
        {
            var conta = await _contaRepository.GetByNumeroAsync(request.NumeroConta);
            if (conta == null)
                return NotFound($"Conta {request.NumeroConta} não encontrada");

            if (request.Valor <= 0)
                return BadRequest("Valor da movimentação deve ser maior que zero");

            if (request.Tipo != "C" && request.Tipo != "D")
                return BadRequest("Tipo deve ser 'C' (crédito) ou 'D' (débito)");

            // Se for débito, verificar saldo
            if (request.Tipo == "D")
            {
                var saldoAtual = await _movimentoRepository.GetSaldoAtualAsync(conta.IdContaCorrente);
                if (saldoAtual < request.Valor)
                    return BadRequest("Saldo insuficiente para realizar o débito");

                await _movimentoRepository.AddDebitoAsync(conta.IdContaCorrente, request.Valor);
            }
            else
            {
                await _movimentoRepository.AddCreditoAsync(conta.IdContaCorrente, request.Valor);
            }

            var novoSaldo = await _movimentoRepository.GetSaldoAtualAsync(conta.IdContaCorrente);

            return Ok(new MovimentoResponse
            {
                IdMovimento = Guid.NewGuid().ToString(),
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                TipoMovimento = request.Tipo,
                Valor = request.Valor,
                DescricaoTipo = request.Tipo == "C" ? "Crédito" : "Débito",
                SaldoAtual = novoSaldo
            });
        }
    }

    // DTOs
    public class ContaCorrenteResponse
    {
        public string IdContaCorrente { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public decimal Saldo { get; set; }
    }

    public class SaldoResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public DateTime DataConsulta { get; set; }
    }

    public class MovimentoResponse
    {
        public string IdMovimento { get; set; } = string.Empty;
        public string DataMovimento { get; set; } = string.Empty;
        public string TipoMovimento { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string DescricaoTipo { get; set; } = string.Empty;
        public decimal? SaldoAtual { get; set; }
    }

    public class CriarContaRequest
    {
        [Required]
        public int Numero { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Senha { get; set; } = string.Empty;
    }

    public class DepositoRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }
    }

    public class MovimentacaoRequest
    {
        [Required]
        public string RequisicaoId { get; set; } = string.Empty;

        [Required]
        public int NumeroConta { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required]
        [RegularExpression("^[CD]$", ErrorMessage = "Tipo deve ser 'C' ou 'D'")]
        public string Tipo { get; set; } = string.Empty;
    }
}
