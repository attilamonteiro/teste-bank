using EstudoApi.Banking.Transfer.Commands;

namespace EstudoApi.Banking.Transfer.Validators
{
    /// <summary>
    /// Validador para comandos de transferência
    /// </summary>
    public static class TransferValidator
    {
        public static TransferValidationResult ValidateCommand(TransferCommand command)
        {
            var errors = new List<string>();

            // Validar RequisicaoId
            if (string.IsNullOrWhiteSpace(command.RequisicaoId))
                errors.Add("RequisicaoId é obrigatório");

            // Validar contas
            if (command.ContaOrigem <= 0)
                errors.Add("Conta de origem deve ser informada e válida");

            if (command.ContaDestino <= 0)
                errors.Add("Conta de destino deve ser informada e válida");

            if (command.ContaOrigem == command.ContaDestino)
                errors.Add("Conta de origem e destino não podem ser iguais");

            // Validar valor
            if (command.Valor <= 0)
                errors.Add("Valor deve ser positivo");

            if (command.Valor > 1000000) // Limite máximo por transferência
                errors.Add("Valor excede o limite máximo de transferência");

            // Validar descrição (se fornecida)
            if (!string.IsNullOrWhiteSpace(command.Descricao) && command.Descricao.Length > 200)
                errors.Add("Descrição deve ter no máximo 200 caracteres");

            return errors.Count == 0
                ? TransferValidationResult.Valid()
                : TransferValidationResult.Invalid(string.Join("; ", errors), "VALIDATION_ERROR");
        }
    }

    /// <summary>
    /// Resultado da validação
    /// </summary>
    public class TransferValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorCode { get; private set; }

        private TransferValidationResult() { }

        public static TransferValidationResult Valid() => new() { IsValid = true };

        public static TransferValidationResult Invalid(string message, string code) => new()
        {
            IsValid = false,
            ErrorMessage = message,
            ErrorCode = code
        };
    }
}
