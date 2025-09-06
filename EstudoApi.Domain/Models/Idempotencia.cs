namespace EstudoApi.Domain.Models
{
    /// <summary>
    /// Entidade Idempotência seguindo o esquema SQLite da Ana
    /// Tabela: idempotencia
    /// </summary>
    public class Idempotencia
    {
        public string ChaveIdempotencia { get; set; } = string.Empty; // TEXT(37) PRIMARY KEY
        public string? Requisicao { get; set; } // TEXT(1000)
        public string? Resultado { get; set; } // TEXT(1000)

        // Construtor para EF
        public Idempotencia() { }

        public Idempotencia(string chave, string requisicao, string resultado)
        {
            ChaveIdempotencia = chave;
            Requisicao = requisicao?.Length > 1000 ? requisicao.Substring(0, 1000) : requisicao;
            Resultado = resultado?.Length > 1000 ? resultado.Substring(0, 1000) : resultado;
        }
    }
}
