using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.ContaBancaria;
using EstudoApi.Domain.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.ContaBancaria
{
    public class LoginContaBancariaCommandHandler : IRequestHandler<LoginContaBancariaCommand, (bool Success, EstudoApi.Domain.Models.ContaBancaria? Conta, string? Error, string? Tipo)>
    {
        private readonly IContaBancariaRepository _repository;
        
        public LoginContaBancariaCommandHandler(IContaBancariaRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<(bool Success, EstudoApi.Domain.Models.ContaBancaria? Conta, string? Error, string? Tipo)> Handle(LoginContaBancariaCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Senha))
                return (false, null, "Senha é obrigatória.", "INVALID_DATA");
                
            EstudoApi.Domain.Models.ContaBancaria? conta = null;
            
            // Buscar por número da conta ou CPF
            if (command.NumeroConta.HasValue)
            {
                conta = await _repository.GetByNumeroAsync(command.NumeroConta.Value);
            }
            else if (!string.IsNullOrWhiteSpace(command.Cpf))
            {
                conta = await _repository.GetByCpfAsync(command.Cpf);
            }
            else
            {
                return (false, null, "Número da conta ou CPF é obrigatório.", "INVALID_DATA");
            }
            
            if (conta == null)
                return (false, null, "Conta não encontrada.", "ACCOUNT_NOT_FOUND");
                
            if (!conta.Ativo)
                return (false, null, "Conta inativa.", "INACTIVE_ACCOUNT");
                
            if (!conta.ValidarSenha(command.Senha))
                return (false, null, "Senha inválida.", "INVALID_PASSWORD");
                
            // Retorna a conta para que o controller possa gerar o token
            return (true, conta, null, null);
        }
    }
}
