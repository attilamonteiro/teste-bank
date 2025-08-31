using System.Collections.Generic;

namespace EstudoApi.Domain.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Cpf { get; private set; }
        public string SenhaHash { get; private set; }
        public bool Ativo { get; private set; } = true;
        public decimal Balance { get; private set; }
        public List<AccountTransaction> Transactions { get; set; } = new();

        // Construtor para EF
        private Account() { Cpf = string.Empty; SenhaHash = string.Empty; }

        public Account(string cpf, string senha)
        {
            if (string.IsNullOrWhiteSpace(cpf)) throw new ArgumentException("CPF obrigatório.");
            if (string.IsNullOrWhiteSpace(senha)) throw new ArgumentException("Senha obrigatória.");
            Cpf = cpf;
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
            Ativo = true;
        }

        public bool ValidarSenha(string senha)
        {
            return BCrypt.Net.BCrypt.Verify(senha, SenhaHash);
        }

        public void Inativar()
        {
            Ativo = false;
        }

        public void Deposit(decimal amount, string? description)
        {
            if (amount <= 0) throw new ArgumentException("Valor inválido para depósito.");
            Balance += amount;
            Transactions.Add(new AccountTransaction
            {
                Amount = amount,
                Description = description,
                Type = TransactionType.Deposit
            });
        }

        public bool Withdraw(decimal amount, string? description)
        {
            if (amount <= 0 || amount > Balance) return false;
            Balance -= amount;
            Transactions.Add(new AccountTransaction
            {
                Amount = -amount,
                Description = description,
                Type = TransactionType.Withdraw
            });
            return true;
        }
    }

    public class AccountTransaction
    {
        public int Id { get; set; } // Chave primária para EF Core
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Deposit,
        Withdraw
    }
}
