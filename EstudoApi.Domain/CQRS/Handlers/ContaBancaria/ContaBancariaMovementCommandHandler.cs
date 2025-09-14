using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.ContaBancaria;
using EstudoApi.Domain.Interfaces.Repositories;
using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.CQRS.Handlers.ContaBancaria
{
    public class ContaBancariaMovementCommandHandler : IRequestHandler<ContaBancariaMovementCommand, (bool Success, string? Error, string? Tipo)>
    {
        private readonly IContaBancariaRepository _repository;
        
        public ContaBancariaMovementCommandHandler(IContaBancariaRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<(bool Success, string? Error, string? Tipo)> Handle(ContaBancariaMovementCommand command, CancellationToken cancellationToken)
        {
            // Validação de valor
            if (command.Valor <= 0)
                return (false, "Apenas valores positivos podem ser recebidos.", "INVALID_VALUE");
                
            if (command.Tipo != "C" && command.Tipo != "D")
                return (false, "Apenas os tipos 'débito' ou 'crédito' podem ser aceitos.", "INVALID_TYPE");

            // Buscar conta
            var numeroConta = command.NumeroConta;
            if (numeroConta == null)
                return (false, "Conta não informada.", "INVALID_ACCOUNT");
                
            var conta = await _repository.GetByNumeroAsync(numeroConta.Value);
            if (conta == null)
                return (false, "Conta não encontrada.", "INVALID_ACCOUNT");
                
            if (!conta.Ativo)
                return (false, "Apenas contas ativas podem receber movimentação.", "INACTIVE_ACCOUNT");

            // Para débito, verificar saldo
            if (command.Tipo == "D")
            {
                if (!conta.PodeDebitar(command.Valor))
                    return (false, "Saldo insuficiente para débito.", "INSUFFICIENT_FUNDS");
            }

            try
            {
                // Adicionar movimento
                conta.AdicionarMovimento(command.Tipo, command.Valor, command.Descricao);
                
                // Criar movimento na tabela para compatibilidade
                var movimento = new Movimento
                {
                    IdMovimento = Guid.NewGuid().ToString(),
                    IdContaCorrente = conta.Id,
                    DataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    TipoMovimento = command.Tipo,
                    Valor = command.Valor
                };

                await _repository.AddMovimentoAsync(movimento);
                await _repository.UpdateAsync(conta);

                return (true, null, null);
            }
            catch (Exception ex)
            {
                return (false, "Erro interno do servidor.", "INTERNAL_ERROR");
            }
        }
    }
}
