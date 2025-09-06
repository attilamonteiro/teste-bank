-- Script de inicialização do banco de dados
-- Este script será executado automaticamente quando o container PostgreSQL for criado

-- Criar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Criar schema para auditoria (se necessário no futuro)
CREATE SCHEMA IF NOT EXISTS audit;

-- Log de inicialização
SELECT 'Banco de dados bankdb inicializado com sucesso!' AS status;
