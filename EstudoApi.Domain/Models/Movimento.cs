namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade Movimento seguindo o esquema SQLite da Ana
    /// Tabela: movimento
    /// </summary>
    public class Movimento
    {
        public string IdMovimento { get; set; } = string.Empty; // TEXT(37) PRIMARY KEY
        public string IdContaCorrente { get; set; } = string.Empty; // TEXT(37) NOT NULL
        public string DataMovimento { get; set; } = string.Empty; // TEXT(25) NOT NULL formato DD/MM/YYYY
        public string TipoMovimento { get; set; } = string.Empty; // TEXT(1) NOT NULL (C = Credito, D = Debito)
        public decimal Valor { get; set; } // REAL NOT NULL

        // Relacionamento
        public ContaCorrente? ContaCorrente { get; set; }

        // Construtor para EF
        public Movimento() { }

        public Movimento(string idContaCorrente, string tipoMovimento, decimal valor, string? descricao = null)
        {
            if (!new[] { "C", "D" }.Contains(tipoMovimento))
                throw new ArgumentException("Tipo de movimento deve ser 'C' ou 'D'");

            if (valor <= 0)
                throw new ArgumentException("Valor deve ser positivo");

            IdMovimento = Guid.NewGuid().ToString();
            IdContaCorrente = idContaCorrente;
            TipoMovimento = tipoMovimento;
            Valor = valor;
            DataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
