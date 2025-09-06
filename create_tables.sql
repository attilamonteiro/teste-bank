-- Criar tabelas bancárias seguindo esquema da Ana

-- Tabela contacorrente
CREATE TABLE IF NOT EXISTS contacorrente (
    IdContaCorrente TEXT(37) PRIMARY KEY,
    Numero INTEGER NOT NULL UNIQUE,
    Nome TEXT(100) NOT NULL,
    Ativo INTEGER NOT NULL DEFAULT 1,
    Senha TEXT(100) NOT NULL,
    Salt TEXT(100) NOT NULL
);

-- Tabela movimento
CREATE TABLE IF NOT EXISTS movimento (
    IdMovimento TEXT(37) PRIMARY KEY,
    IdContaCorrente TEXT(37) NOT NULL,
    DataMovimento TEXT(25) NOT NULL,
    TipoMovimento TEXT(1) NOT NULL CHECK (TipoMovimento IN ('C', 'D')),
    Valor REAL NOT NULL,
    FOREIGN KEY (IdContaCorrente) REFERENCES contacorrente(IdContaCorrente)
);

-- Tabela transferencia
CREATE TABLE IF NOT EXISTS transferencia (
    IdTransferencia TEXT(37) PRIMARY KEY,
    IdContaCorrenteOrigem TEXT(37) NOT NULL,
    IdContaCorrenteDestino TEXT(37) NOT NULL,
    DataMovimento TEXT(25) NOT NULL,
    Valor REAL NOT NULL,
    FOREIGN KEY (IdContaCorrenteOrigem) REFERENCES contacorrente(IdContaCorrente),
    FOREIGN KEY (IdContaCorrenteDestino) REFERENCES contacorrente(IdContaCorrente)
);

-- Tabela idempotencia
CREATE TABLE IF NOT EXISTS idempotencia (
    IdIdempotencia TEXT(37) PRIMARY KEY,
    Requisicao TEXT(255) NOT NULL UNIQUE,
    Recurso TEXT(255) NOT NULL,
    DataMovimento TEXT(25) NOT NULL
);

-- Tabela tarifa
CREATE TABLE IF NOT EXISTS tarifa (
    IdTarifa TEXT(37) PRIMARY KEY,
    IdContaCorrente TEXT(37) NOT NULL,
    Tipo TEXT(50) NOT NULL,
    DataMovimento TEXT(25) NOT NULL,
    Valor REAL NOT NULL,
    FOREIGN KEY (IdContaCorrente) REFERENCES contacorrente(IdContaCorrente)
);

-- Criar índices para performance
CREATE INDEX IF NOT EXISTS idx_movimento_conta ON movimento(IdContaCorrente);
CREATE INDEX IF NOT EXISTS idx_movimento_data ON movimento(DataMovimento);
CREATE INDEX IF NOT EXISTS idx_transferencia_origem ON transferencia(IdContaCorrenteOrigem);
CREATE INDEX IF NOT EXISTS idx_transferencia_destino ON transferencia(IdContaCorrenteDestino);
CREATE INDEX IF NOT EXISTS idx_tarifa_conta ON tarifa(IdContaCorrente);

-- Inserir conta de teste
INSERT OR IGNORE INTO contacorrente (IdContaCorrente, Numero, Nome, Ativo, Senha, Salt) 
VALUES 
    ('550e8400-e29b-41d4-a716-446655440000', 123456, 'João Silva', 1, 'hashedpassword123', 'salt123'),
    ('550e8400-e29b-41d4-a716-446655440001', 789012, 'Maria Santos', 1, 'hashedpassword456', 'salt456');
