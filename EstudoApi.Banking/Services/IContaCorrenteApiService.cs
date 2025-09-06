using EstudoApi.Domain.Models;
using EstudoApi.Domain.CQRS.Commands.Account;

namespace EstudoApi.Banking.Services;

public interface IContaCorrenteApiService
{
    Task<ContaCorrente?> GetContaAsync(string numeroConta);
    Task<bool> UpdateSaldoAsync(string numeroConta, decimal novoSaldo);
    Task<bool> ValidateContaAsync(string numeroConta);
    Task<MovementResult> RealizarMovimentacao(string token, AccountMovementCommand command);
}

public class MovementResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }

    public static MovementResult Success() => new() { IsSuccess = true };
    public static MovementResult Failure(string error, string errorCode) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorCode = errorCode
    };
}
