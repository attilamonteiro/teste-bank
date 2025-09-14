# 🔄 PROPOSTA DE REFATORAÇÃO: UNIFICAR USUÁRIO E CONTA CORRENTE

## 📋 ANÁLISE DO PROBLEMA ATUAL

### Entidades Duplicadas:
- **Account** (tabela Accounts): Autenticação básica
- **ContaCorrente** (tabela contacorrente): Operações bancárias

### Problemas Identificados:
1. ❌ **Duplicação de dados**: CPF, senha, ativo
2. ❌ **Lógica fragmentada**: Auth vs Banking
3. ❌ **Complexidade desnecessária**: 2 tabelas para 1 conceito
4. ❌ **Sincronização manual**: Dados podem ficar inconsistentes
5. ❌ **Manutenção complexa**: Mudanças precisam ser feitas em 2 lugares

## 🎯 SOLUÇÃO PROPOSTA: ENTIDADE UNIFICADA

### Nova Entidade: `ContaBancaria`

```csharp
public class ContaBancaria
{
    // Identificação
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
    
    // Métodos
    public bool ValidarSenha(string senha) => BCrypt.Net.BCrypt.Verify(senha + Salt, SenhaHash);
    public decimal CalcularSaldo() => Movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor) - 
                                     Movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);
    public void AdicionarMovimento(string tipo, decimal valor, string? descricao = null) { /* */ }
    public void Inativar() { Ativo = false; DataInativacao = DateTime.UtcNow; }
}
```

## 🔄 MIGRAÇÃO ESTRATÉGICA

### Passo 1: Criar Nova Entidade
```sql
CREATE TABLE conta_bancaria (
    id TEXT(37) PRIMARY KEY,
    numero INTEGER UNIQUE NOT NULL,
    nome TEXT(100) NOT NULL,
    cpf TEXT(11) UNIQUE NOT NULL,
    senha_hash TEXT(100) NOT NULL,
    salt TEXT(100) NOT NULL,
    ativo INTEGER(1) DEFAULT 1,
    data_criacao TEXT(25) NOT NULL,
    data_inativacao TEXT(25)
);
```

### Passo 2: Migrar Dados Existentes
```sql
-- Migrar dados da tabela contacorrente (que tem mais campos)
INSERT INTO conta_bancaria (id, numero, nome, cpf, senha_hash, salt, ativo, data_criacao)
SELECT 
    IdContaCorrente,
    Numero,
    Nome,
    '00000000000' as cpf, -- CPF temporário, precisa ser preenchido
    Senha,
    Salt,
    Ativo,
    datetime('now')
FROM contacorrente;

-- Atualizar CPFs baseado na tabela Accounts (se houver correlação)
-- Esta parte precisa de lógica específica dependendo de como correlacionar
```

### Passo 3: Atualizar Controllers
```csharp
[ApiController]
[Route("api/conta")]
public class ContaBancariaController : ControllerBase
{
    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroRequest request)
    {
        // Validar CPF único
        var existente = await _repository.GetByCpfAsync(request.Cpf);
        if (existente != null)
            return BadRequest("CPF já cadastrado");
            
        // Criar conta única que serve para auth E banking
        var conta = new ContaBancaria(request.Nome, request.Cpf, request.Senha);
        await _repository.AddAsync(conta);
        
        return Ok(new { numeroConta = conta.Numero, id = conta.Id });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var conta = await _repository.GetByNumeroAsync(request.NumeroConta);
        if (conta?.ValidarSenha(request.Senha) == true)
        {
            var token = _jwtService.GenerateToken(conta);
            return Ok(new { token });
        }
        return Unauthorized();
    }
    
    [HttpGet("saldo")]
    [Authorize]
    public async Task<IActionResult> ConsultarSaldo()
    {
        var numeroConta = GetNumeroContaFromToken();
        var conta = await _repository.GetByNumeroAsync(numeroConta);
        
        return Ok(new 
        { 
            numeroConta = conta.Numero,
            nomeTitular = conta.Nome,
            saldo = conta.CalcularSaldo(),
            dataConsulta = DateTime.Now
        });
    }
    
    [HttpPost("movimentar")]
    [Authorize]
    public async Task<IActionResult> Movimentar([FromBody] MovimentacaoRequest request)
    {
        var numeroConta = GetNumeroContaFromToken();
        var conta = await _repository.GetByNumeroAsync(numeroConta);
        
        if (!conta.Ativo)
            return BadRequest("Conta inativa");
            
        conta.AdicionarMovimento(request.Tipo, request.Valor, request.Descricao);
        await _repository.UpdateAsync(conta);
        
        return Ok(new { saldoFinal = conta.CalcularSaldo() });
    }
}
```

## 🎉 BENEFÍCIOS DA REFATORAÇÃO

### ✅ Simplicidade
- **1 entidade** em vez de 2
- **1 tabela** para manter
- **1 sistema** de autenticação

### ✅ Consistência
- **Dados únicos**: sem duplicação
- **Estado único**: uma fonte da verdade
- **Validação única**: CPF/senha em um lugar

### ✅ Manutenibilidade
- **Mudanças simples**: alterar em 1 lugar
- **Menos bugs**: menos pontos de falha
- **Código limpo**: lógica coesa

### ✅ Performance
- **Menos JOINs**: dados em uma tabela
- **Menos consultas**: buscar tudo de uma vez
- **Cache eficiente**: entidade completa

### ✅ Experiência do Usuário
- **Cadastro único**: criar conta = pode operar
- **Login simples**: uma credencial para tudo
- **Operações diretas**: sem validações extras

## 🚀 IMPLEMENTAÇÃO GRADUAL

### Fase 1: Implementar nova entidade
- Criar `ContaBancaria`
- Implementar repository
- Criar testes unitários

### Fase 2: Migrar endpoints
- Atualizar `/cadastrar`
- Atualizar `/login`
- Manter compatibilidade temporária

### Fase 3: Migrar dados
- Script de migração
- Validação de integridade
- Backup de segurança

### Fase 4: Remover código antigo
- Deletar `Account` e `ContaCorrente`
- Limpar repositories antigos
- Atualizar documentação

## 💡 CONSIDERAÇÕES ESPECIAIS

### Compatibilidade com Sistema Banking
- API de transferências precisa ser atualizada
- Validações devem usar nova entidade
- Manter interface similar para não quebrar

### Segurança
- Manter hash de senha robusta
- Implementar rate limiting
- Auditoria de operações

### Escalabilidade Futura
- Permitir múltiplas contas por CPF (se necessário)
- Separar dados pessoais de dados bancários (se necessário)
- Implementar soft delete

## 🎯 CONCLUSÃO

A unificação elimina **complexidade desnecessária** e cria um sistema mais **robusto**, **simples** e **fácil de manter**. 

**Recomendação: IMPLEMENTAR a refatoração** para ter um sistema bancário mais coeso e eficiente.
