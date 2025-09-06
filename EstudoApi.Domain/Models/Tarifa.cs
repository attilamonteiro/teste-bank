namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade Tarifa seguindo o esquema SQLite da Ana
    /// Tabela: tarifa
    /// </summary>
    public class Tarifa
    {
        public string IdTarifa { get; set; } = string.Empty; // TEXT(37) PRIMARY KEY
        public string IdContaCorrente { get; set; } = string.Empty; // TEXT(37) NOT NULL
        public string DataMovimento { get; set; } = string.Empty; // TEXT(25) NOT NULL formato DD/MM/YYYY
        public decimal Valor { get; set; } // REAL NOT NULL

        // Relacionamento
        public ContaCorrente? ContaCorrente { get; set; }

        // Construtor para EF
        public Tarifa() { }

        public Tarifa(string idContaCorrente, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(idContaCorrente))
                throw new ArgumentException("Id da conta corrente é obrigatório");

            if (valor <= 0)
                throw new ArgumentException("Valor da tarifa deve ser positivo");

            IdTarifa = Guid.NewGuid().ToString();
            IdContaCorrente = idContaCorrente;
            Valor = valor;
            DataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
