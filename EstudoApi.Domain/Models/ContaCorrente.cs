namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade ContaCorrente seguindo o esquema SQLite da Ana
    /// Tabela: contacorrente
    /// </summary>
    public class ContaCorrente
    {
        public string IdContaCorrente { get; set; } = string.Empty; // TEXT(37) PRIMARY KEY
        public int Numero { get; set; } // INTEGER(10) NOT NULL UNIQUE
        public string Nome { get; set; } = string.Empty; // TEXT(100) NOT NULL
        public int Ativo { get; set; } = 1; // INTEGER(1) NOT NULL default 0
        public string Senha { get; set; } = string.Empty; // TEXT(100) NOT NULL
        public string Salt { get; set; } = string.Empty; // TEXT(100) NOT NULL

        // Propriedades calculadas
        public bool IsAtiva => Ativo == 1;

        // Relacionamentos
        public List<Movimento> Movimentos { get; set; } = new();

        // Construtor para EF
        public ContaCorrente() { }

        public ContaCorrente(string nome, string cpf, string senha)
        {
            IdContaCorrente = Guid.NewGuid().ToString();
            Nome = nome;
            Ativo = 1;

            // Gerar próximo número da conta (em produção seria mais sofisticado)
            Numero = new Random().Next(100000, 999999);

            // Gerar salt e hash da senha
            Salt = BCrypt.Net.BCrypt.GenerateSalt();
            Senha = BCrypt.Net.BCrypt.HashPassword(senha + Salt);
        }

        public bool ValidarSenha(string senha)
        {
            return BCrypt.Net.BCrypt.Verify(senha + Salt, Senha);
        }

        public void AdicionarMovimento(string tipoMovimento, decimal valor, string? descricao = null)
        {
            var movimento = new Movimento(IdContaCorrente, tipoMovimento, valor, descricao);
            Movimentos.Add(movimento);
        }

        public bool PodeDebitar(decimal valor)
        {
            return CalcularSaldo() >= valor;
        }

        public void Inativar()
        {
            Ativo = 0;
        }

        public decimal CalcularSaldo()
        {
            var creditos = Movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor);
            var debitos = Movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);
            return creditos - debitos;
        }
    }
}
