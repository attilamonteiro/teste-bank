namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade que representa uma transferência entre contas
    /// </summary>
    public class Transfer
    {
        public int Id { get; set; }
        public string RequisicaoId { get; private set; }
        public int ContaOrigem { get; private set; }
        public int ContaDestino { get; private set; }
        public decimal Valor { get; private set; }
        public string? Descricao { get; private set; }
        public DateTime DataTransferencia { get; private set; }
        public string Status { get; private set; } // PROCESSANDO, CONCLUIDA, ERRO, ESTORNADA

        // Construtor para EF
        private Transfer()
        {
            RequisicaoId = string.Empty;
            Status = string.Empty;
        }

        public Transfer(
            string requisicaoId,
            int contaOrigem,
            int contaDestino,
            decimal valor,
            string? descricao = null)
        {
            if (string.IsNullOrWhiteSpace(requisicaoId))
                throw new ArgumentException("RequisicaoId é obrigatório");

            if (contaOrigem <= 0)
                throw new ArgumentException("Conta origem deve ser válida");

            if (contaDestino <= 0)
                throw new ArgumentException("Conta destino deve ser válida");

            if (contaOrigem == contaDestino)
                throw new ArgumentException("Conta origem e destino devem ser diferentes");

            if (valor <= 0)
                throw new ArgumentException("Valor deve ser positivo");

            RequisicaoId = requisicaoId;
            ContaOrigem = contaOrigem;
            ContaDestino = contaDestino;
            Valor = valor;
            Descricao = descricao;
            DataTransferencia = DateTime.UtcNow;
            Status = "PROCESSANDO";
        }

        public void MarcarComoConcluida()
        {
            Status = "CONCLUIDA";
        }

        public void MarcarComoErro()
        {
            Status = "ERRO";
        }

        public void MarcarComoEstornada()
        {
            Status = "ESTORNADA";
        }
    }
}
