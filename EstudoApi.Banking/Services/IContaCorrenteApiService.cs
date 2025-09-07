using EstudoApi.Domain.Models;
using EstudoApi.Domain.CQRS.Commands.Account;

namespace EstudoApi.Banking.Services
{
    public interface IContaCorrenteApiService
    {
        Task<MovementResult> RealizarMovimentacao(string token, AccountMovementCommand command);
        Task<ContaCorrente?> GetContaAsync(int numeroConta);
        Task<bool> PingAsync();
    }

    public class MovementResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public decimal? FinalBalance { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        public static MovementResult CreateSuccess(decimal? finalBalance = null, string? message = null)
        {
            return new MovementResult
            {
                Success = true,
                Message = message,
                FinalBalance = finalBalance
            };
        }

        public static MovementResult CreateFailure(string message, string? errorCode = null)
        {
            return new MovementResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
