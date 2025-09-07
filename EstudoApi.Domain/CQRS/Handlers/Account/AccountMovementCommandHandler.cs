using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;
using EstudoApi.Domain.Interfaces.Repositories;
using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class AccountMovementCommandHandler : IRequestHandler<AccountMovementCommand, (bool Success, string? Error, string? Tipo)>
    {
        private readonly IContaCorrenteRepository _contaRepository;
        private readonly IAccountRepository _repository;
        
        public AccountMovementCommandHandler(IContaCorrenteRepository contaRepository, IAccountRepository repository)
        {
            _contaRepository = contaRepository;
            _repository = repository;
        }
        
        public async Task<(bool Success, string? Error, string? Tipo)> Handle(AccountMovementCommand command, CancellationToken cancellationToken)
        {
            // Validação de valor
            if (command.Valor <= 0)
                return (false, "Apenas valores positivos podem ser recebidos.", "INVALID_VALUE");
            if (command.Tipo != "C" && command.Tipo != "D")
                return (false, "Apenas os tipos 'débito' ou 'crédito' podem ser aceitos.", "INVALID_TYPE");

            // Buscar conta nas tabelas SQLite
            var numeroConta = command.NumeroConta;
            if (numeroConta == null)
                return (false, "Conta não informada.", "INVALID_ACCOUNT");
                
            var contaCorrente = await _contaRepository.GetByNumeroAsync(numeroConta.Value);
            if (contaCorrente == null)
                return (false, "Apenas contas correntes cadastradas podem receber movimentação.", "INVALID_ACCOUNT");
            if (contaCorrente.Ativo != 1)
                return (false, "Apenas contas correntes ativas podem receber movimentação.", "INACTIVE_ACCOUNT");

            // Para débito, verificar saldo atual
            if (command.Tipo == "D")
            {
                var saldoAtual = await _contaRepository.GetSaldoAsync(contaCorrente.IdContaCorrente);
                if (saldoAtual < command.Valor)
                    return (false, "Saldo insuficiente para débito.", "INVALID_VALUE");
            }

            // Criar movimento na tabela SQLite
            var movimento = new Movimento
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdContaCorrente = contaCorrente.IdContaCorrente,
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TipoMovimento = command.Tipo,
                Valor = command.Valor
            };

            // Adicionar movimento diretamente via repository
            await _contaRepository.AddMovimentoAsync(movimento);
            
            return (true, null, null);
        }
    }
}
