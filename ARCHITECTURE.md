# Arquitetura - Sistema de Validação de Crédito

Documentação técnica detalhada da arquitetura e design do sistema.

## 📐 Visão Geral da Arquitetura

O projeto segue o padrão **Clean Architecture** com **SOLID Principles** e **Design Patterns**.

```
┌─────────────────────────────────────────────────────────┐
│                  CAMADA DE APRESENTAÇÃO                 │
│                   (Driving.Api)                          │
│  Controllers • Swagger/OpenAPI • Middlewares • Routes   │
└────────────┬────────────────────────────────────────────┘
             │
             │ HTTP Requests/Responses
             │
┌────────────▼────────────────────────────────────────────┐
│              CAMADA DE APLICAÇÃO                        │
│                (Core.Application)                        │
│  Services • DTOs • Mappers • Business Logic             │
└────────────┬────────────────────────────────────────────┘
             │
             │ Interfaces
             │
┌────────────▼────────────────────────────────────────────┐
│              CAMADA DE DOMÍNIO                          │
│                (Core.Domain)                             │
│  Entities • Enums • Value Objects • Business Rules      │
└────────────┬────────────────────────────────────────────┘
             │
             │ Abstrações
             │
┌──────────┬─┴──────────┬───────────────────┬───────────┐
│          │            │                   │           │
│          │            │                   │           │
▼          ▼            ▼                   ▼           ▼
┌────┐ ┌─────┐ ┌──────────┐ ┌───────┐ ┌─────────┐
│  Infra   │ SQLite │ RabbitMQ │ Cache │  Email   │
│(Logging) │ Data   │Messaging │Memory │ Service  │
└────┘ └─────┘ └──────────┘ └───────┘ └─────────┘
  │      │          │           │         │
  └──────┴──────────┴───────────┴─────────┘
        CAMADA DE INFRAESTRUTURA
         (Adaptadores Externos)
```

---

## 🏗️ Camadas Detalhadas

### 1. Camada de Apresentação (Driving.Api)

**Responsabilidade:** Receber requisições HTTP e enviar respostas

```
Driving.Api/
├── Controllers/
│   ├── AuthController.cs        ← Login/Autenticação
│   └── CreditoController.cs     ← Validação de Crédito
├── Extensions/
│   ├── SerilogExtensions.cs     ← Configuração de logs
│   └── ServiceExtensions.cs     ← Injeção de dependência
├── Middlewares/                  ← Custom middlewares
├── Properties/
│   └── launchSettings.json      ← Portas e profiles
└── Program.cs                   ← Configuração da app
```

**Responsabilidades:**
- ✅ Receber requisições HTTP
- ✅ Validar entrada (ModelState)
- ✅ Autenticação e autorização
- ✅ Retornar respostas HTTP
- ✅ Não contém lógica de negócio

**Padrão de Resposta:**
```csharp
public class ApiResponseDto<T>
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; }
    public T Dados { get; set; }
    public List<string> Erros { get; set; }
}
```

### 2. Camada de Aplicação (Core.Application)

**Responsabilidade:** Implementar casos de uso e orquestrar o domínio

```
Core.Application/
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   └── LoginResponseDto.cs
│   └── Credito/
│       ├── ValidarCreditoRequestDto.cs
│       └── ValidarCreditoResponseDto.cs
├── Interfaces/
│   ├── Services/
│   │   ├── IAuthenticationService.cs
│   │   └── ICreditoService.cs
│   └── Repositories/
│       ├── IUsuarioRepository.cs
│       └── ICreditoRepository.cs
├── Mappers/
│   └── MappingProfile.cs         ← AutoMapper configs
└── Services/
    ├── AuthenticationService.cs
    └── CreditoService.cs
```

**Responsabilidades:**
- ✅ Implementar casos de uso
- ✅ Orquestrar domínio
- ✅ Gerenciar transações
- ✅ Mapear DTOs
- ✅ Depender apenas de abstrações (interfaces)

**Exemplo de Serviço:**
```csharp
public interface IAuthenticationService
{
    Task<ApiResponse<LoginResponseDto>> AutenticarAsync(LoginDto login);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;

    public async Task<ApiResponse<LoginResponseDto>> AutenticarAsync(LoginDto login)
    {
        // Validar credenciais
        var usuario = await _usuarioRepository.ObterPorUsuarioAsync(login.Usuario);

        if (usuario == null || !VerificaSenha(login.Senha, usuario.SenhaHash))
            return ErroResponse("Credenciais inválidas");

        // Gerar token
        var token = _tokenService.GerarToken(usuario);

        return SuccessResponse(new LoginResponseDto
        {
            Token = token,
            ExpiracaoEm = DateTime.UtcNow.AddHours(1)
        });
    }
}
```

### 3. Camada de Domínio (Core.Domain)

**Responsabilidade:** Expressar regras de negócio

```
Core.Domain/
├── Entities/
│   ├── Usuario.cs               ← Usuário do sistema
│   ├── Credito.cs               ← Validação de crédito
│   └── Auditoria.cs             ← Rastreamento
├── Enums/
│   ├── StatusCredito.cs
│   ├── NivelRisco.cs
│   └── TipoDocumento.cs
└── ValueObjects/
    ├── CPF.cs                   ← Validação de CPF
    ├── Email.cs                 ← Validação de Email
    └── Score.cs                 ← Tipo fortemente tipado
```

**Responsabilidades:**
- ✅ Definir entidades
- ✅ Implementar regras de negócio
- ✅ Validações de domínio
- ✅ Sem dependências externas

**Exemplo de Entity:**
```csharp
public class Credito
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string CPF { get; set; }
    public int ScoreCrediticio { get; set; }  // 0-1000
    public NivelRisco NivelRisco { get; set; }
    public decimal LimiteAprovado { get; set; }
    public DateTime DataAnalise { get; set; }
    public bool Ativo { get; set; }

    // Lógica de domínio
    public void AtualizarScore(int novoScore)
    {
        if (novoScore < 0 || novoScore > 1000)
            throw new DomainException("Score deve estar entre 0 e 1000");

        ScoreCrediticio = novoScore;
        NivelRisco = DeterminarNivel(novoScore);
        LimiteAprovado = CalcularLimite(novoScore);
    }

    private NivelRisco DeterminarNivel(int score) => score switch
    {
        >= 800 => NivelRisco.Baixo,
        >= 600 => NivelRisco.Moderado,
        _ => NivelRisco.Alto
    };
}
```

### 4. Camada de Infraestrutura (Core.Infra)

**Responsabilidade:** Prover serviços técnicos

```
Core.Infra/
├── Logging/
│   └── SerilogConfiguration.cs  ← Serilog setup
├── Cache/
│   └── CacheService.cs          ← In-memory cache
├── Email/
│   └── EmailService.cs          ← SMTP integration
└── Extension/
    └── ServiceCollectionExtensions.cs
```

**Responsabilidades:**
- ✅ Logging estruturado
- ✅ Cache em memória
- ✅ Envio de emails
- ✅ Configurações técnicas

### 5. Adaptadores (Driven)

#### 5.1 Driven.SqlLite (Persistência)

```
Driven.SqlLite/
├── Data/
│   ├── ApplicationDbContext.cs  ← EF Core DbContext
│   ├── DesignTimeDbContextFactory.cs
│   └── SeedData.cs              ← Dados iniciais
├── Repositories/
│   ├── UsuarioRepository.cs
│   └── CreditoRepository.cs
├── Configurations/
│   ├── UsuarioConfiguration.cs  ← Modelagem EF
│   └── CreditoConfiguration.cs
└── Migrations/
    └── [Migrações do banco]
```

**Responsabilidades:**
- ✅ Acesso aos dados
- ✅ Persistência com EF Core
- ✅ Migrações do banco
- ✅ Queries e filtros

**Exemplo:**
```csharp
public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<Usuario> ObterPorUsuarioAsync(string usuario)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NomeUsuario == usuario);
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }
}
```

#### 5.2 Driven.RabbitMQ (Mensageria)

```
Driven.RabbitMQ/
├── Services/
│   ├── MessageBusService.cs     ← Produtor
│   └── RabbitMQSubscriber.cs    ← Consumidor
├── Interfaces/
│   └── IMessageBus.cs
└── Models/
    └── [Mensagens de domínio]
```

**Responsabilidades:**
- ✅ Publicar eventos
- ✅ Consumir mensagens
- ✅ Tratamento de erros
- ✅ Retry automático

---

## 🔄 Fluxo de Dados

### Exemplo: Login de Usuário

```
1. HTTP Request
   POST /api/Auth/login
   Body: { usuario: "john", senha: "pass123" }
         │
         ▼
2. AuthController (Driving.Api)
   - Validar ModelState
   - Chamar IAuthenticationService
         │
         ▼
3. AuthenticationService (Core.Application)
   - Validar credenciais
   - Chamar IUsuarioRepository.ObterPorUsuarioAsync()
         │
         ▼
4. UsuarioRepository (Driven.SqlLite)
   - Query EF Core
   - Retornar Usuario from DB
         │
         ▼
5. AuthenticationService (voltando)
   - Gerar JWT Token
   - Mapear para LoginResponseDto
         │
         ▼
6. AuthController (voltando)
   - Retornar 200 OK
         │
         ▼
7. HTTP Response
   200 OK
   Body: {
     sucesso: true,
     dados: { token: "eyJ...", expiracaoEm: "..." }
   }
```

---

## 🔐 Segurança em Camadas

### Autenticação & Autorização

```
Request HTTP
    │
    ▼
[JwtBearerToken Middleware]  ← Valida JWT
    │
    ├─ Valid ─────────────────────┐
    │                             │
    ├─ Invalid ──► 401 Unauthorized
    │
    ▼
[Authorization Middleware]   ← Verifica [Authorize]
    │
    ├─ Authorized ───────────┐
    │                        │
    ├─ Not Authorized ─► 403 Forbidden
    │
    ▼
[Controller Action] ← Execute com identity
```

### Validações em Camadas

```
┌─ AuthController
│  └─ ModelState validation
│     └─ [Required], [MinLength], etc
│
└─ AuthenticationService
   └─ Business rule validation
      └─ Credenciais corretas?
      └─ Usuário ativo?

└─ Entity (Domain)
   └─ Domain validations
      └─ Regras de negócio
```

---

## 📦 Padrões de Design Utilizados

### 1. Repository Pattern

```csharp
// Interface (Core.Application)
public interface IUsuarioRepository
{
    Task<Usuario> ObterPorIdAsync(Guid id);
    Task AdicionarAsync(Usuario usuario);
}

// Implementação (Driven.SqlLite)
public class UsuarioRepository : IUsuarioRepository
{
    // ...
}

// Uso (Core.Application Service)
public class UsuarioService
{
    private readonly IUsuarioRepository _repository;

    public async Task<Usuario> GetUserAsync(Guid id)
        => await _repository.ObterPorIdAsync(id);
}
```

### 2. Dependency Injection

```csharp
// Program.cs
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Controller
public class UsuarioController
{
    private readonly IUsuarioService _service;

    public UsuarioController(IUsuarioService service)
        => _service = service;  // Injetado pelo DI
}
```

### 3. Data Transfer Objects (DTO)

```csharp
// Request DTO
public class LoginDto
{
    [Required]
    public string Usuario { get; set; }

    [Required]
    public string Senha { get; set; }
}

// Response DTO
public class LoginResponseDto
{
    public string Token { get; set; }
    public DateTime ExpiracaoEm { get; set; }
}

// Entity ➜ DTO Mapping (AutoMapper)
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>();
        CreateMap<Credito, CreditoResponseDto>();
    }
}
```

### 4. Factory Pattern (Implícito)

```csharp
// Criar instâncias do DbContext
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
        => new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("Data Source=credito.db")
                .Options);
}
```

### 5. Observer Pattern (RabbitMQ)

```csharp
// Publicador
public class CreditoService
{
    private readonly IMessageBus _messageBus;

    public async Task ValidarAsync(Credito credito)
    {
        // ... validação

        // Publicar evento
        await _messageBus.PublishAsync(
            new CreditoValidadoEvent { CreditoId = credito.Id });
    }
}

// Assinante (Subscriber)
public class CreditoValidadoEventHandler
{
    [RabbitMQQueue("credito.validado")]
    public async Task HandleAsync(CreditoValidadoEvent evento)
    {
        // ... processar evento
    }
}
```

---

## 🔗 Dependências Entre Camadas

```
Camada                 Pode depender de
────────────────────────────────────────
Driving.Api        →   Core.Application, Core.Domain

Core.Application   →   Core.Domain, Core.Infra

Core.Domain        →   (Nenhuma - Isolado)

Core.Infra         →   Core.Domain

Driven.SqlLite     →   Core.Domain, Core.Application

Driven.RabbitMQ    →   Core.Domain, Core.Application
```

**Importante:** Dependências sempre apontam PARA BAIXO (direção ao domínio).

---

## 📊 Entity-Relationship Diagram (ERD)

```
┌──────────────┐
│   Usuarios   │
├──────────────┤
│ ID (PK)      │
│ NomeUsuario  │
│ SenhaHash    │
│ Email        │
│ Ativo        │
│ DataCriacao  │
└──────────────┘
      │
      │ 1:N
      │
      ▼
┌──────────────────┐
│    Creditos      │
├──────────────────┤
│ ID (PK)          │
│ UsuarioId (FK)   │
│ CPF              │
│ ScoreCrediticio  │
│ NivelRisco       │
│ LimiteAprovado   │
│ DataAnalise      │
│ Ativo            │
└──────────────────┘
```

---

## 🔄 Lifecycle de uma Requisição

```
1. HTTP Request chega
   ↓
2. Middleware: Logging
   ↓
3. Middleware: JWT Authentication
   ↓
4. Middleware: Authorization
   ↓
5. Controller Action
   ├─ Validação ModelState
   ├─ Chamar Service
   │
   └─ Service (Core.Application)
      ├─ Validação de negócio
      ├─ Chamar Repository
      │
      └─ Repository (Driven)
         ├─ Acesso ao banco
         ├─ Executar query
         └─ Retornar dados

      ├─ Mapear para DTO
      └─ Retornar resultado

   └─ Retornar resposta HTTP
      ↓
6. Middleware: Logging
   ↓
7. Response enviada ao cliente
```

---

## 🛡️ Tratamento de Erros

```csharp
// Controller
try
{
    var resultado = await _service.ProcessarAsync(request);
    return Ok(resultado);
}
catch (ValidationException ex)
{
    _logger.LogWarning("Validação falhou: {Message}", ex.Message);
    return BadRequest(new ApiResponse { Erros = ex.Errors });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Erro desconhecido");
    return StatusCode(500, new ApiResponse { Mensagem = "Erro interno" });
}
```

---

## 📈 Escalabilidade

### Estrutura atual suporta:

- ✅ **Horizontal Scaling**: Stateless design
- ✅ **Caching**: In-memory cache implementado
- ✅ **Async/Await**: Operações não-bloqueantes
- ✅ **Connection Pooling**: EF Core + SQLite
- ✅ **Message Queue**: RabbitMQ para async processing

### Melhorias futuras:

- 🚀 Redis para distributed cache
- 🚀 SQL Server para melhor performance
- 🚀 Load Balancer (nginx/haproxy)
- 🚀 CDN para assets estáticos
- 🚀 Message Bus patterns (CQRS, Event Sourcing)

---

## 📚 Referências

- [Clean Architecture - Robert Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://www.digitalocean.com/community/conceptual_articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [RabbitMQ Patterns](https://www.rabbitmq.com/getstarted.html)

---

**Última atualização**: 03/11/2024 | **Versão**: 1.0.0
