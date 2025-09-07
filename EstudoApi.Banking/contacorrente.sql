CREATE TABLE IF NOT EXISTS "contacorrente" (
    "IdContaCorrente" TEXT NOT NULL CONSTRAINT "PK_contacorrente" PRIMARY KEY,
    "Numero" INTEGER NOT NULL,
    "Nome" TEXT NOT NULL,
    "Ativo" INTEGER NOT NULL DEFAULT 1,
    "Senha" TEXT NOT NULL,
    "Salt" TEXT NOT NULL,
    CONSTRAINT "CK_contacorrente_ativo" CHECK (ativo IN (0,1))
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_contacorrente_Numero" ON "contacorrente" ("Numero");

INSERT INTO contacorrente (IdContaCorrente, Numero, Nome, Ativo, Senha, Salt) VALUES 
('550e8400-e29b-41d4-a716-446655440000', 123456, 'João Silva', 1, 'hashedpassword123', 'salt123'),
('550e8400-e29b-41d4-a716-446655440001', 789012, 'Maria Santos', 1, 'hashedpassword456', 'salt456');
