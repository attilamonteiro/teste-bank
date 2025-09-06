namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade Transferência seguindo o esquema SQLite da Ana
    /// Tabela: transferencia
    /// </summary>
    public class Transferencia
    {
        public string IdTransferencia { get; set; } = string.Empty; // TEXT(37) PRIMARY KEY
        public string IdContaCorrenteOrigem { get; set; } = string.Empty; // TEXT(37) NOT NULL
        public string IdContaCorrenteDestino { get; set; } = string.Empty; // TEXT(37) NOT NULL
        public string DataMovimento { get; set; } = string.Empty; // TEXT(25) NOT NULL formato DD/MM/YYYY
        public decimal Valor { get; set; } // REAL NOT NULL

        // Relacionamentos
        public ContaCorrente? ContaOrigem { get; set; }
        public ContaCorrente? ContaDestino { get; set; }

        // Construtor para EF
        public Transferencia() { }

        public Transferencia(string idContaOrigem, string idContaDestino, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(idContaOrigem))
                throw new ArgumentException("Id da conta origem é obrigatório");

            if (string.IsNullOrWhiteSpace(idContaDestino))
                throw new ArgumentException("Id da conta destino é obrigatório");

            if (idContaOrigem == idContaDestino)
                throw new ArgumentException("Contas origem e destino devem ser diferentes");

            if (valor <= 0)
                throw new ArgumentException("Valor deve ser positivo");

            IdTransferencia = Guid.NewGuid().ToString();
            IdContaCorrenteOrigem = idContaOrigem;
            IdContaCorrenteDestino = idContaDestino;
            Valor = valor;
            DataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
