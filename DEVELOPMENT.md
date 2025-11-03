# Guia de Desenvolvimento - Sistema de Validação de Crédito

Documentação técnica para desenvolvedores que trabalham no projeto.

## 📚 Índice

- [Configuração do Ambiente](#configuração-do-ambiente)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Padrões de Código](#padrões-de-código)
- [Como Adicionar Novos Endpoints](#como-adicionar-novos-endpoints)
- [Como Adicionar Novos Serviços](#como-adicionar-novos-serviços)
- [Migrações de Banco de Dados](#migrações-de-banco-de-dados)
- [Depuração](#depuração)
- [Git Workflow](#git-workflow)

---

## 🛠️ Configuração do Ambiente

### 1. Instalar Dependências

```bash
# Clone o repositório
git clone <seu-repositorio>
cd Credito

# Restaurar pacotes NuGet
dotnet restore

# Compilar solução
dotnet build
```

### 2. Configurar IDE

#### Visual Studio 2022
1. Abrir `Validacao.Credito.sln`
2. Definir `Driving.Api` como projeto de inicialização
3. Pressionar `F5` para iniciar com debug

#### VS Code
1. Instalar extensão "C# Dev Kit"
2. Abrir pasta do projeto
3. Pressionar `F5` para iniciar

### 3. Configurar Banco de Dados

```bash
# Aplicar migrações
dotnet ef database update -p Driven.SqlLite -s Driving.Api

# Verificar status
dotnet ef migrations list -p Driven.SqlLite -s Driving.Api
```

---

## 📁 Estrutura de Pastas

### Convenções

```
Driving.Api/
├── Controllers/          # Um arquivo por controller
│   └── AuthController.cs
├── Extensions/          # Métodos de extensão e configurações
│   ├── SerilogExtensions.cs
│   └── ServiceExtensions.cs
├── Middlewares/         # Middlewares customizados (se houver)
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── Driving.Api.csproj

Core.Application/
├── DTOs/                # Data Transfer Objects
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   └── LoginResponseDto.cs
│   └── Credito/
├── Interfaces/
│   ├── Services/        # Contratos de serviços
│   │   ├── IAuthenticationService.cs
│   │   └── ICreditoService.cs
│   └── Repositories/    # Contratos de repositórios
├── Mappers/            # AutoMapper profiles
│   └── MappingProfile.cs
└── Services/           # Implementação dos serviços
    ├── AuthenticationService.cs
    └── CreditoService.cs

Core.Domain/
├── Entities/           # Modelos de domínio
│   ├── Usuario.cs
│   └── Credito.cs
├── Enums/
│   ├── StatusCredito.cs
│   └── NivelRisco.cs
└── ValueObjects/

Driven.SqlLite/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DesignTimeDbContextFactory.cs
├── Repositories/
│   ├── UsuarioRepository.cs
│   └── CreditoRepository.cs
└── Migrations/

Driven.RabbitMQ/
├── Services/
│   ├── MessageBusService.cs
│   └── RabbitMQSubscriber.cs
└── Interfaces/
    └── IMessageBus.cs
```

---

## 💻 Padrões de Código

### 1. Nomenclatura

```csharp
// Classes públicas: PascalCase
public class UsuarioService { }

// Propriedades: PascalCase
public string Nome { get; set; }

// Variáveis locais e parâmetros: camelCase
public void ProcessarUsuario(Usuario usuario)
{
    var nomeFormatado = usuario.Nome.ToUpper();
}

// Constantes: UPPER_SNAKE_CASE
private const string CHAVE_JWT = "sua_chave";

// Interfaces: IPrefixPascalCase
public interface IUsuarioService { }
```

### 2. Estrutura de Classe

```csharp
using System;
using System.Collections.Generic;
using Core.Domain.Entities;
using Core.Application.Interfaces.Repositories;

namespace Core.Application.Services;

/// <summary>
/// Descrição breve da classe.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UsuarioService> _logger;

    // Construtor com injeção de dependência
    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    // Métodos públicos
    public async Task<Usuario> ObterPorIdAsync(Guid id)
    {
        _logger.LogInformation("Obtendo usuário com ID: {UserId}", id);
        return await _usuarioRepository.GetByIdAsync(id);
    }

    // Métodos privados
    private bool ValidarEmail(string email)
    {
        return email.Contains("@");
    }
}
```

### 3. Async/Await

```csharp
// ✅ Correto: Sempre async para operações I/O
public async Task<Usuario> ObterUsuarioAsync(Guid id)
{
    return await _usuarioRepository.GetByIdAsync(id);
}

// ❌ Evitar: Não usar .Result ou .Wait()
var usuario = _usuarioRepository.GetByIdAsync(id).Result; // Pode deadlock!

// ✅ Correto: Usar async/await
var usuario = await _usuarioRepository.GetByIdAsync(id);
```

### 4. Validação de Entrada

```csharp
public async Task<ApiResponse<LoginResponseDto>> AutenticarAsync(LoginDto login)
{
    // Validações imediatas
    if (string.IsNullOrWhiteSpace(login.Usuario))
        return new ApiResponse<LoginResponseDto>
        {
            Sucesso = false,
            Mensagem = "Usuário não pode ser vazio",
            Erros = new[] { "Campo obrigatório: usuario" }
        };

    // Lógica de negócio
    // ...
}
```

### 5. Logging

```csharp
// ✅ Bom: Usar structured logging
_logger.LogInformation("Usuário {UserId} realizado login", usuario.Id);

// ❌ Ruim: String concatenation
_logger.LogInformation($"Usuário {usuario.Id} realizado login");

// Debug: Informações detalhadas
_logger.LogDebug("Validando credenciais para usuário: {Usuario}", usuario.Nome);

// Warning: Situações inesperadas
_logger.LogWarning("Múltiplas tentativas de login falhadas para: {Usuario}", usuario.Nome);

// Error: Exceções
catch (Exception ex)
{
    _logger.LogError(ex, "Erro ao processar login para usuário: {Usuario}", usuario.Nome);
}
```

---

## 🆕 Como Adicionar Novos Endpoints

### Exemplo: Criar Endpoint de Validação de Crédito

#### Passo 1: Criar DTO (Core.Application/DTOs)

```csharp
// ValidarCreditoRequestDto.cs
public class ValidarCreditoRequestDto
{
    public string Cpf { get; set; }
    public decimal RendaMensal { get; set; }
    public int TempoEmpregoMeses { get; set; }
}

// ValidarCreditoResponseDto.cs
public class ValidarCreditoResponseDto
{
    public Guid CreditoId { get; set; }
    public int ScoreCrediticio { get; set; }
    public NivelRisco NivelRisco { get; set; }
    public decimal LimiteAprovado { get; set; }
    public string Mensagem { get; set; }
}
```

#### Passo 2: Criar Interface de Serviço

```csharp
// Core.Application/Interfaces/Services/ICreditoService.cs
public interface ICreditoService
{
    Task<ApiResponse<ValidarCreditoResponseDto>> ValidarCreditoAsync(
        ValidarCreditoRequestDto request);
}
```

#### Passo 3: Implementar Serviço

```csharp
// Core.Application/Services/CreditoService.cs
public class CreditoService : ICreditoService
{
    private readonly ICreditoRepository _creditoRepository;
    private readonly ILogger<CreditoService> _logger;

    public CreditoService(
        ICreditoRepository creditoRepository,
        ILogger<CreditoService> logger)
    {
        _creditoRepository = creditoRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<ValidarCreditoResponseDto>> ValidarCreditoAsync(
        ValidarCreditoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Validando crédito para CPF: {Cpf}", request.Cpf);

            // Validações
            if (request.RendaMensal <= 0)
                return new ApiResponse<ValidarCreditoResponseDto>
                {
                    Sucesso = false,
                    Mensagem = "Renda mensal inválida",
                    Erros = new[] { "Renda deve ser maior que zero" }
                };

            // Lógica de scoring
            int score = CalcularScore(request);
            var nivelRisco = DeterminarNivelRisco(score);
            var limite = CalcularLimite(request, score);

            // Persistir resultado
            var credito = new Credito
            {
                Cpf = request.Cpf,
                ScoreCrediticio = score,
                NivelRisco = nivelRisco,
                LimiteAprovado = limite,
                DataAnalise = DateTime.UtcNow
            };

            await _creditoRepository.AddAsync(credito);

            _logger.LogInformation(
                "Crédito validado: CPF={Cpf}, Score={Score}, Limite={Limite}",
                request.Cpf, score, limite);

            return new ApiResponse<ValidarCreditoResponseDto>
            {
                Sucesso = true,
                Mensagem = "Crédito validado com sucesso",
                Dados = new ValidarCreditoResponseDto
                {
                    CreditoId = credito.Id,
                    ScoreCrediticio = score,
                    NivelRisco = nivelRisco,
                    LimiteAprovado = limite,
                    Mensagem = $"Aprovado com limite de R$ {limite:C}"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar crédito para CPF: {Cpf}", request.Cpf);
            throw;
        }
    }

    private int CalcularScore(ValidarCreditoRequestDto request)
    {
        // Algoritmo de scoring
        int score = 600;

        // Renda
        if (request.RendaMensal > 5000) score += 100;
        if (request.RendaMensal > 10000) score += 100;

        // Tempo de emprego
        if (request.TempoEmpregoMeses > 24) score += 50;
        if (request.TempoEmpregoMeses > 60) score += 50;

        return Math.Min(score, 1000);
    }

    private NivelRisco DeterminarNivelRisco(int score)
    {
        return score switch
        {
            >= 800 => NivelRisco.Baixo,
            >= 600 => NivelRisco.Moderado,
            _ => NivelRisco.Alto
        };
    }

    private decimal CalcularLimite(ValidarCreditoRequestDto request, int score)
    {
        var limite = request.RendaMensal * 3;
        var fator = score / 1000m;
        return limite * fator;
    }
}
```

#### Passo 4: Criar Controller

```csharp
// Driving.Api/Controllers/CreditoController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditoController : ControllerBase
{
    private readonly ICreditoService _creditoService;
    private readonly ILogger<CreditoController> _logger;

    public CreditoController(
        ICreditoService creditoService,
        ILogger<CreditoController> logger)
    {
        _creditoService = creditoService;
        _logger = logger;
    }

    /// <summary>
    /// Valida e analisa o crédito de um cliente
    /// </summary>
    /// <param name="request">Dados do cliente para análise</param>
    /// <returns>Score e limite de crédito aprovado</returns>
    /// <response code="200">Crédito validado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("validar")]
    [ProducesResponseType(typeof(ApiResponse<ValidarCreditoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidarCreditoResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidarCredito([FromBody] ValidarCreditoRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(new ApiResponse<ValidarCreditoResponseDto>
            {
                Sucesso = false,
                Mensagem = "Dados inválidos",
                Erros = errors
            });
        }

        var resultado = await _creditoService.ValidarCreditoAsync(request);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }
}
```

#### Passo 5: Registrar Serviço (Program.cs)

```csharp
// Em Program.cs, adicione na seção de injeção de dependência
builder.Services.AddScoped<ICreditoService, CreditoService>();
builder.Services.AddScoped<ICreditoRepository, CreditoRepository>();
```

---

## 🆕 Como Adicionar Novos Serviços

### Exemplo: Criar Serviço de Cache

#### 1. Criar Interface

```csharp
// Core.Application/Interfaces/Services/ICacheService.cs
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}
```

#### 2. Implementar Serviço

```csharp
// Core.Infra/Cache/CacheService.cs
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T> GetAsync<T>(string key)
    {
        _logger.LogDebug("Obtendo do cache: {CacheKey}", key);
        var result = _memoryCache.TryGetValue(key, out T value) ? value : default;
        return Task.FromResult(result);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration;

        _logger.LogDebug("Armazenando em cache: {CacheKey}", key);
        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _logger.LogDebug("Removendo do cache: {CacheKey}", key);
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}
```

#### 3. Registrar em Program.cs

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();
```

---

## 💾 Migrações de Banco de Dados

### Criar Nova Migração

```bash
cd Driven.SqlLite

# Criar migração
dotnet ef migrations add NomeMigracao -s ../Driving.Api

# Exemplo
dotnet ef migrations add AdicionarTabelaCredito -s ../Driving.Api
```

### Estrutura da Migração

```csharp
public partial class AdicionarTabelaCredito : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Creditos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Cpf = table.Column<string>(type: "TEXT", nullable: false),
                ScoreCrediticio = table.Column<int>(type: "INTEGER", nullable: false),
                NivelRisco = table.Column<int>(type: "INTEGER", nullable: false),
                LimiteAprovado = table.Column<decimal>(type: "REAL", nullable: false),
                DataAnalise = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Creditos", x => x.Id);
                table.UniqueConstraint("UQ_Creditos_Cpf", x => x.Cpf);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Creditos");
    }
}
```

### Aplicar Migrações

```bash
# Atualizar para última migração
dotnet ef database update -s ../Driving.Api

# Atualizar para migração específica
dotnet ef database update AdicionarTabelaCredito -s ../Driving.Api

# Reverter última migração
dotnet ef database update NomeMigracaoAnterior -s ../Driving.Api
```

---

## 🐛 Depuração

### Configurar Breakpoints

1. Clicar na margem esquerda do código
2. Um ponto vermelho aparecerá
3. Pressionar `F5` para iniciar debug
4. A execução parará no breakpoint

### Condicional Breakpoint

```csharp
// Clicar direito no breakpoint > Filter
// Adicionar condição: usuario.Id == "123"
```

### Debug Console

```
> Methods
> _usuarioRepository.Count()
< 42

> variables local
< usuario { Id = Guid, Nome = "João" }
```

### Immediate Window

```csharp
// Ctrl+Alt+I (Visual Studio)
usuario.Nome.Length
"João".ToUpper()
```

---

## 🔄 Git Workflow

### Fluxo de Branches

```
main (produção)
 ↑
develop (desenvolvimento)
 ↑
feature/nova-funcionalidade
feature/corrigir-bug
hotfix/problema-critico
```

### Criar Feature

```bash
# Atualizar develop
git checkout develop
git pull origin develop

# Criar branch de feature
git checkout -b feature/validacao-credito

# Fazer alterações
git add .
git commit -m "feat: adicionar validação de crédito"

# Push para remoto
git push -u origin feature/validacao-credito

# Abrir Pull Request no GitHub
```

### Padrão de Commit

```
<tipo>(<escopo>): <descrição>

<corpo>

<rodapé>
```

**Tipos válidos:**
- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `docs`: Documentação
- `style`: Formatação (sem lógica)
- `refactor`: Refatoração
- `test`: Testes
- `chore`: Dependências, build

**Exemplos:**
```bash
feat(credito): adicionar validação de score
fix(auth): corrigir expiração de token
docs: atualizar README com instruções de setup
test(credito): adicionar testes unitários de scoring
```

### Merge para develop

```bash
# Cria Pull Request via GitHub interface
# Após aprovação:

git checkout develop
git pull origin develop
git merge feature/validacao-credito
git push origin develop
```

---

## 📋 Checklist de Pull Request

Antes de submeter um PR, certifique-se:

- [ ] Código compila sem erros
- [ ] Testes passam: `dotnet test`
- [ ] Sem warnings graves
- [ ] Código segue padrões do projeto
- [ ] Comentários e documentação adicionados
- [ ] Commits com mensagens descritivas
- [ ] Branch atualizado com develop
- [ ] Sem arquivos desnecessários commitados

---

**Última atualização**: 03/11/2024 | **Versão**: 1.0.0
