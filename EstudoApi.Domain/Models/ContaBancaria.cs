using System.ComponentModel.DataAnnotations;

namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade unificada que combina autenticação de usuário e operações bancárias
    /// </summary>
    public class ContaBancaria
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Numero { get; set; } // Número da conta (gerado automaticamente)
        
        // Dados do cliente
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        
        // Autenticação
        public string SenhaHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        
        // Estado da conta
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataInativacao { get; set; }
        
        // Relacionamentos
        public List<Movimento> Movimentos { get; set; } = new();
        
        // Construtor para EF
        public ContaBancaria() { }
        
        // Construtor seguindo os requisitos (apenas CPF e senha)
        public ContaBancaria(string cpf, string senha)
        {
            if (string.IsNullOrWhiteSpace(cpf)) throw new ArgumentException("CPF é obrigatório");
            if (string.IsNullOrWhiteSpace(senha)) throw new ArgumentException("Senha é obrigatória");
            
            Id = Guid.NewGuid().ToString();
            Nome = string.Empty; // Nome não é obrigatório nos requisitos
            Cpf = cpf;
            
            // Gerar número da conta (em produção seria sequencial controlado)
            Numero = new Random().Next(100000, 999999);
            
            // Gerar salt e hash da senha
            Salt = BCrypt.Net.BCrypt.GenerateSalt();
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha + Salt);
            
            Ativo = true;
            DataCriacao = DateTime.UtcNow;
        }
        
        public bool ValidarSenha(string senha)
        {
            return BCrypt.Net.BCrypt.Verify(senha + Salt, SenhaHash);
        }
        
        public decimal CalcularSaldo()
        {
            var creditos = Movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor);
            var debitos = Movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);
            return creditos - debitos;
        }
        
        public void AdicionarMovimento(string tipoMovimento, decimal valor, string? descricao = null)
        {
            if (tipoMovimento != "C" && tipoMovimento != "D")
                throw new ArgumentException("Tipo de movimento deve ser 'C' ou 'D'");
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser positivo");
                
            var movimento = new Movimento(Id, tipoMovimento, valor, descricao);
            Movimentos.Add(movimento);
        }
        
        public bool PodeDebitar(decimal valor)
        {
            return CalcularSaldo() >= valor;
        }
        
        public void Inativar()
        {
            Ativo = false;
            DataInativacao = DateTime.UtcNow;
        }
        
        public void Ativar()
        {
            Ativo = true;
            DataInativacao = null;
        }
    }
}
