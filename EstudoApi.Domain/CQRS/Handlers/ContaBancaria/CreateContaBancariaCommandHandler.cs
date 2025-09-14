using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.ContaBancaria;
using EstudoApi.Domain.Interfaces.Repositories;
using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.CQRS.Handlers.ContaBancaria
{
    public class CreateContaBancariaCommandHandler : IRequestHandler<CreateContaBancariaCommand, (bool Success, int? NumeroConta, string? Error, string? Tipo)>
    {
        private readonly IContaBancariaRepository _repository;
        
        public CreateContaBancariaCommandHandler(IContaBancariaRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<(bool Success, int? NumeroConta, string? Error, string? Tipo)> Handle(CreateContaBancariaCommand command, CancellationToken cancellationToken)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(command.Cpf))
                return (false, null, "CPF é obrigatório.", "INVALID_DATA");
                
            if (string.IsNullOrWhiteSpace(command.Senha))
                return (false, null, "Senha é obrigatória.", "INVALID_DATA");
                
            if (!CpfUtils.IsValid(command.Cpf))
                return (false, null, "CPF inválido.", "INVALID_DOCUMENT");
                
            // Verificar se CPF já existe
            var existente = await _repository.GetByCpfAsync(command.Cpf);
            if (existente != null)
                return (false, null, "CPF já cadastrado.", "DUPLICATE_DOCUMENT");
                
            try
            {
                // Criar conta bancária seguindo o schema do banco (apenas CPF e senha)
                var conta = new EstudoApi.Domain.Models.ContaBancaria(command.Cpf, command.Senha);
                await _repository.AddAsync(conta);
                
                return (true, conta.Numero, null, null);
            }
            catch (Exception ex)
            {
                return (false, null, "Erro interno do servidor.", "INTERNAL_ERROR");
            }
        }
    }

    public static class CpfUtils
    {
        public static bool IsValid(string cpf)
        {
            // Validação simplificada de CPF
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11) return false;
            return long.TryParse(cpf, out _);
        }
    }
}
