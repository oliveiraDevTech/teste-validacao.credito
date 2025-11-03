# Documentação Técnica - Validação de Crédito API

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Arquitetura](#arquitetura)
3. [Stack Tecnológica](#stack-tecnológica)
4. [Entidades e Modelo de Domínio](#entidades-e-modelo-de-domínio)
5. [Regras de Negócio](#regras-de-negócio)
6. [APIs e Endpoints](#apis-e-endpoints)
7. [Fluxos de Processo](#fluxos-de-processo)
8. [Integração e Mensageria](#integração-e-mensageria)
9. [Segurança](#segurança)
10. [Persistência de Dados](#persistência-de-dados)
11. [Padrões e Práticas](#padrões-e-práticas)
12. [Configurações](#configurações)

---

## 📊 Visão Geral

### Propósito do Sistema
A **Validação de Crédito API** é um microsserviço especializado responsável por:
- **Análise de crédito** de clientes
- **Cálculo de score de crédito** (0-1000 pontos)
- **Determinação de limites de crédito** por cartão
- **Definição de número de cartões** permitidos
- **Publicação de eventos** de avaliação de crédito
- **Consumo de eventos** de cadastro de clientes

### Contexto de Negócio
O serviço atua como **motor de decisão de crédito** na arquitetura:
1. Consome evento de cliente criado/atualizado
2. Realiza análise de crédito baseada em score
3. Calcula limite de crédito e número de cartões permitidos
4. Atualiza informações de crédito do cliente
5. Publica evento de crédito avaliado para downstream

### Características Principais
- ✅ **Clean Architecture** com separação de camadas
- ✅ **Event-Driven** com RabbitMQ
- ✅ **Análise de crédito automatizada**
- ✅ **Regras de negócio configuráveis**
- ✅ **Processamento assíncrono**
- ✅ **Idempotência** em avaliações
- ✅ **Auditoria completa** de decisões

---

## 🏗️ Arquitetura

### Diagrama de Camadas

```
┌───────────────────────────────────────────────────────────────┐
│                    Driving.Api Layer                          │
│  Controllers, Middleware, JWT Authentication                  │
│  - AuthController: Login e autenticação                       │
│  - (Futuramente: CreditoController para consultas)            │
└─────────────────────────────┬─────────────────────────────────┘
                              │
┌─────────────────────────────▼─────────────────────────────────┐
│                  Core.Application Layer                       │
│  Services, DTOs, Business Logic                               │
│  - ClienteService: Gestão de dados de crédito                 │
│  - AuthenticationService: JWT e autenticação                  │
│  - CreditoAvaliacaoService: Lógica de avaliação              │
└─────────────────────────────┬─────────────────────────────────┘
                              │
┌─────────────────────────────▼─────────────────────────────────┐
│                     Core.Domain Layer                         │
│  Entities, Value Objects, Business Rules                      │
│  - Cliente: Dados do cliente + informações de crédito         │
│  - RegraCredito: Regras de pontuação e limites               │
└─────────────────────────────┬─────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼───────┐  ┌──────────▼─────────┐  ┌───────▼──────────┐
│ Driven.SqlLite│  │  Core.Infra        │  │ Driven.RabbitMQ  │
│ Repositories  │  │  Cache             │  │ Subscribers      │
│ EF Core       │  │  Logging           │  │ Publishers       │
│ Migrations    │  │  Email             │  │ Events           │
└───────────────┘  └────────────────────┘  └──────────────────┘
```

### Fluxo Event-Driven

```
[Cliente API] → Pub "cliente.criado"
                       ↓
                [RabbitMQ Broker]
                       ↓
        [Crédito API] ← Sub "cliente.criado"
                       ↓
          [CreditoAvaliacaoService]
                       ↓
              [Calcular Score]
                       ↓
          [Determinar Limite e Cartões]
                       ↓
              [Salvar no BD]
                       ↓
        Pub "credito.avaliado" → [RabbitMQ]
                       ↓
        [Cliente API] ← Atualiza ranking
                       ↓
        [Cartão API] ← Dados para emissão
```

---

## 🛠️ Stack Tecnológica

### Framework & Runtime
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **.NET** | 8.0 | Runtime e Framework base |
| **ASP.NET Core** | 8.0 | Web API framework |
| **C#** | 12 | Linguagem de programação |

### Persistência
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **Entity Framework Core** | 8.0 | ORM para acesso a dados |
| **SQLite** | 3.x | Banco de dados embarcado |
| **EF Core Migrations** | 8.0 | Versionamento de schema |

### Mensageria
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **RabbitMQ** | 3.12+ | Message broker AMQP |
| **RabbitMQ.Client** | 6.x | Client library .NET |

### Segurança
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **JWT Bearer** | - | Autenticação stateless |
| **BCrypt.Net** | - | Hashing de senhas |

### Observabilidade
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **Serilog** | 3.x | Logging estruturado |
| **Serilog.Sinks.Console** | - | Output para console |
| **Serilog.Sinks.File** | - | Output para arquivos |

### Qualidade & Testes
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **xUnit** | 2.5+ | Framework de testes |
| **Moq** | 4.x | Mocking library |
| **FluentAssertions** | 6.x | Assertions fluentes |

---

## 📦 Entidades e Modelo de Domínio

### 1. Cliente

**Responsabilidade**: Armazena dados do cliente com foco em informações de crédito

```csharp
public class Cliente : BaseEntity
{
    // Dados Pessoais
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Telefone { get; private set; }
    public string Cpf { get; private set; }
    
    // Endereço
    public string Endereco { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public string Cep { get; private set; }
    
    // Informações de Crédito (Core do serviço)
    public int ScoreCredito { get; private set; }                    // 0-1000
    public decimal LimiteCreditoPorCartao { get; private set; }      // Em R$
    public int NumeroMaximoCartoes { get; private set; }             // 0, 1 ou 2
    public DateTime? DataUltimaAvaliacaoCredito { get; private set; }
}
```

**Constantes de Negócio**
```csharp
// Faixas de Score
SCORE_MIN = 0
SCORE_MAX = 1000

// Thresholds de Score
SCORE_SEM_APROVACAO = 100           // 0-100: Sem cartão
SCORE_UM_CARTAO = 500               // 101-500: 1 cartão
SCORE_DOIS_CARTOES = 501            // 501-1000: 2 cartões

// Limites de Crédito
LIMITE_BASICO = 1000.00             // Score 101-500
LIMITE_AVANCADO = 5000.00           // Score 501-1000
LIMITE_ZERO = 0.00                  // Score 0-100
```

**Factory Methods**
- `Cliente.Criar()`: Cria novo cliente
- `Cliente.AtualizarCredito()`: Atualiza score e recalcula limites
- `Cliente.AvaliarCredito()`: Executa análise completa

### 2. BaseEntity (Herança)

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime DataCriacao { get; protected set; }
    public DateTime? DataAtualizacao { get; protected set; }
    public bool Ativo { get; protected set; }
}
```

---

## ⚖️ Regras de Negócio

### Análise de Crédito

#### RN-001: Faixas de Score de Crédito
- **Regra**: Score determina elegibilidade e limites
- **Faixas**:
  ```
  Faixa 1 (0-100):     SEM APROVAÇÃO
                       - Cartões: 0
                       - Limite: R$ 0,00
                       - Status: Reprovado
  
  Faixa 2 (101-500):   APROVAÇÃO BÁSICA
                       - Cartões: 1
                       - Limite por cartão: R$ 1.000,00
                       - Status: Aprovado Básico
  
  Faixa 3 (501-1000):  APROVAÇÃO PREMIUM
                       - Cartões: 2
                       - Limite por cartão: R$ 5.000,00
                       - Status: Aprovado Premium
  ```

#### RN-002: Cálculo de Número de Cartões
- **Regra**: Número de cartões baseado exclusivamente no score
- **Algoritmo**:
  ```csharp
  if (scoreCredito <= 100)
      numeroCartoes = 0;
  else if (scoreCredito <= 500)
      numeroCartoes = 1;
  else
      numeroCartoes = 2;
  ```
- **Implementação**: `Cliente.CalcularNumeroCartoes()`

#### RN-003: Cálculo de Limite de Crédito
- **Regra**: Limite por cartão baseado na faixa de score
- **Algoritmo**:
  ```csharp
  if (scoreCredito <= 100)
      limite = 0;
  else if (scoreCredito <= 500)
      limite = 1000.00;
  else
      limite = 5000.00;
  ```
- **Moeda**: Real brasileiro (BRL)
- **Implementação**: `Cliente.CalcularLimiteCredito()`

#### RN-004: Atualização de Score
- **Regra**: Score pode ser atualizado a qualquer momento
- **Gatilhos**:
  - Atualização manual via API
  - Evento de avaliação externa
  - Reavaliação periódica
- **Efeitos colaterais**:
  - Recalcula número de cartões
  - Recalcula limite de crédito
  - Atualiza `DataUltimaAvaliacaoCredito`
  - Publica evento `credito.avaliado`

#### RN-005: Idempotência de Avaliação
- **Regra**: Mesma avaliação não deve ser processada múltiplas vezes
- **Estratégia**: 
  - Verificar `DataUltimaAvaliacaoCredito`
  - Se última avaliação < 1 hora: ignorar
  - Se última avaliação >= 1 hora: processar
- **Exceção**: Atualização manual sempre processa

### Publicação de Eventos

#### RN-006: Evento credito.avaliado
- **Regra**: Publicado após conclusão de avaliação
- **Trigger**: Quando score é atualizado
- **Payload**:
  ```json
  {
    "clienteId": "guid",
    "scoreCredito": 780,
    "limiteCreditoPorCartao": 5000.00,
    "numeroMaximoCartoes": 2,
    "dataAvaliacao": "2024-11-03T10:00:00Z"
  }
  ```
- **Consumidores**:
  - Cliente API: Atualiza ranking
  - Cartão API: Usa para emissão

#### RN-007: Evento credito.reprovado
- **Regra**: Publicado quando score <= 100
- **Payload**:
  ```json
  {
    "clienteId": "guid",
    "scoreCredito": 85,
    "motivoReprovacao": "Score abaixo do mínimo",
    "dataAvaliacao": "2024-11-03T10:00:00Z"
  }
  ```
- **Consumidores**:
  - Cliente API: Atualiza status
  - Notificação: Envia email ao cliente

### Consumo de Eventos

#### RN-008: Consumo de cliente.criado
- **Regra**: Ao receber evento de novo cliente, criar registro local
- **Fluxo**:
  1. Recebe evento `cliente.criado`
  2. Cria registro em banco local (replica dados)
  3. Executa avaliação de crédito inicial
  4. Publica `credito.avaliado`

#### RN-009: Consumo de cliente.atualizado
- **Regra**: Atualizar dados locais quando cliente muda
- **Campos sincronizados**:
  - Nome
  - Email
  - Telefone
  - Endereço
- **Não sincronizado**:
  - Score (gerenciado localmente)
  - Limites (calculados localmente)

---

## 🌐 APIs e Endpoints

### Base URL
```
http://localhost:5002/api
```

### Autenticação

#### POST /auth/login
Autentica usuário e retorna token JWT

**Request**
```json
{
  "email": "admin@sistema.com",
  "password": "Admin@123"
}
```

**Response 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "type": "Bearer",
  "expiresIn": 3600,
  "usuario": {
    "id": "guid",
    "nome": "Admin Sistema",
    "email": "admin@sistema.com",
    "role": "Admin"
  }
}
```

### Crédito (APIs Futuras)

#### GET /credito/cliente/{clienteId}
Obtém avaliação de crédito de um cliente

**Headers**
```
Authorization: Bearer {token}
```

**Response 200 OK**
```json
{
  "sucesso": true,
  "mensagem": "Avaliação obtida com sucesso",
  "dados": {
    "clienteId": "guid",
    "nome": "João Silva",
    "scoreCredito": 780,
    "limiteCreditoPorCartao": 5000.00,
    "numeroMaximoCartoes": 2,
    "dataUltimaAvaliacao": "2024-11-03T10:00:00Z",
    "status": "APROVADO_PREMIUM"
  }
}
```

#### PUT /credito/cliente/{clienteId}
Atualiza score de crédito de um cliente

**Request**
```json
{
  "scoreCredito": 850
}
```

**Response 200 OK**
```json
{
  "sucesso": true,
  "mensagem": "Score atualizado com sucesso",
  "dados": {
    "clienteId": "guid",
    "scoreCredito": 850,
    "limiteCreditoPorCartao": 5000.00,
    "numeroMaximoCartoes": 2,
    "alteracoes": {
      "scoreAnterior": 780,
      "limiteAnterior": 5000.00,
      "cartoesAnterior": 2
    }
  }
}
```

#### POST /credito/reavaliar/{clienteId}
Força reavaliação de crédito

**Response 202 Accepted**
```json
{
  "sucesso": true,
  "mensagem": "Reavaliação iniciada",
  "dados": {
    "clienteId": "guid",
    "status": "EM_PROCESSAMENTO"
  }
}
```

---

## 🔄 Fluxos de Processo

### Fluxo 1: Criação de Cliente (Event-Driven)

```
[Cliente API] → Cria cliente
       ↓
[Publica "cliente.criado" no RabbitMQ]
       ↓
[Crédito API] ← Consome "cliente.criado"
       ↓
[ClienteCriadoHandler.Handle()]
       ↓
[ClienteService.CriarAsync()]
       ↓
[Cliente.Criar()] → Cria registro local
       ↓
[ScoreCredito = 0] (inicial)
       ↓
[Cliente.AvaliarCredito()]
       ↓
[CalcularNumeroCartoes()] → numeroCartoes = 0
       ↓
[CalcularLimiteCredito()] → limite = 0
       ↓
[ClienteRepository.AdicionarAsync()]
       ↓
[SaveChanges()]
       ↓
[PublicarEvento("credito.avaliado")]
       ↓
[Log: "Cliente criado e avaliado"]
```

### Fluxo 2: Atualização de Score

```
[API Request] → PUT /credito/cliente/{id}
       ↓
[CreditoController.AtualizarScore()]
       ↓
[ClienteService.AtualizarCreditoAsync()]
       ↓
[ClienteRepository.ObterPorIdAsync()]
       ↓
[Cliente.AtualizarCredito(novoScore)]
       ↓
       ├→ [CalcularNumeroCartoes()]
       ├→ [CalcularLimiteCredito()]
       └→ [DataUltimaAvaliacaoCredito = Now]
       ↓
[ClienteRepository.AtualizarAsync()]
       ↓
[Begin Transaction]
       ↓
[SaveChanges()]
       ↓
[PublicarEvento("credito.avaliado")]
       ↓
[Commit Transaction]
       ↓
[Retorna 200 OK com novos dados]
```

### Fluxo 3: Reavaliação Periódica (Background Job)

```
[Hosted Service] → Timer: Diário 03:00
       ↓
[ClienteRepository.ObterClientesParaReavaliacao()]
       ↓
[Filtro: DataUltimaAvaliacao < 30 dias]
       ↓
Loop: Para cada cliente
       ↓
       [ObterScoreExterno()] → Integração futura
       ↓
       [Cliente.AtualizarCredito(scoreExterno)]
       ↓
       [SaveChanges()]
       ↓
       [PublicarEvento("credito.avaliado")]
       ↓
Fim Loop
       ↓
[Log: "Reavaliação concluída: X clientes"]
```

---

## 📨 Integração e Mensageria

### RabbitMQ

**Configuração**
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "AutomaticRecovery": true
  }
}
```

**Exchanges e Filas**

| Exchange | Tipo | Routing Key | Fila | Papel |
|----------|------|-------------|------|-------|
| `cliente-events` | Topic | `cliente.criado` | `credito-cliente-criado` | Consumer |
| `cliente-events` | Topic | `cliente.atualizado` | `credito-cliente-atualizado` | Consumer |
| `credito-events` | Topic | `credito.avaliado` | `credito-avaliado-queue` | Producer |
| `credito-events` | Topic | `credito.reprovado` | `credito-reprovado-queue` | Producer |

### Eventos Consumidos

**Evento: cliente.criado**
```json
{
  "eventId": "guid",
  "eventType": "cliente.criado",
  "timestamp": "2024-11-03T10:00:00Z",
  "data": {
    "clienteId": "guid",
    "nome": "João Silva",
    "email": "joao@email.com",
    "cpf": "12345678901",
    "telefone": "11987654321",
    "endereco": {
      "logradouro": "Rua Exemplo, 123",
      "cidade": "São Paulo",
      "estado": "SP",
      "cep": "01234567"
    }
  }
}
```

**Handler**: `ClienteCriadoHandler`
**Ação**: Criar registro local + avaliar crédito inicial

**Evento: cliente.atualizado**
```json
{
  "eventId": "guid",
  "eventType": "cliente.atualizado",
  "timestamp": "2024-11-03T10:30:00Z",
  "data": {
    "clienteId": "guid",
    "camposAlterados": ["telefone", "endereco"],
    "dadosAtualizados": {
      "telefone": "11999887766",
      "endereco": {...}
    }
  }
}
```

**Handler**: `ClienteAtualizadoHandler`
**Ação**: Atualizar dados locais (exceto score)

### Eventos Publicados

**Evento: credito.avaliado**
```json
{
  "eventId": "guid",
  "eventType": "credito.avaliado",
  "timestamp": "2024-11-03T10:35:00Z",
  "correlationId": "req-123",
  "data": {
    "clienteId": "guid",
    "scoreCredito": 780,
    "limiteCreditoPorCartao": 5000.00,
    "numeroMaximoCartoes": 2,
    "dataAvaliacao": "2024-11-03T10:35:00Z",
    "statusAprovacao": "APROVADO_PREMIUM",
    "alteracoes": {
      "scoreAlterado": true,
      "limiteAlterado": false,
      "cartoesAlterado": false
    }
  }
}
```

**Consumidores**:
- **Cliente API**: Atualiza `RankingCredito` e `AptoParaCartaoCredito`
- **Cartão API**: Usa dados para validar emissão

**Evento: credito.reprovado**
```json
{
  "eventId": "guid",
  "eventType": "credito.reprovado",
  "timestamp": "2024-11-03T10:40:00Z",
  "data": {
    "clienteId": "guid",
    "scoreCredito": 85,
    "motivoReprovacao": "Score inferior ao mínimo exigido (101)",
    "dataAvaliacao": "2024-11-03T10:40:00Z",
    "recomendacoes": [
      "Regularizar pendências financeiras",
      "Aguardar 90 dias para nova avaliação"
    ]
  }
}
```

**Consumidores**:
- **Cliente API**: Atualiza status de crédito
- **Notificação API**: Envia email ao cliente

---

## 🔒 Segurança

### Autenticação JWT

**Configuração**
```json
{
  "Jwt": {
    "Secret": "chave-secreta-minimo-32-caracteres",
    "Issuer": "CadastroClientesApi",
    "Audience": "CadastroClientesApp",
    "ExpirationMinutes": 60
  }
}
```

### Autorização

**Endpoints Protegidos**
```csharp
[Authorize]  // Requer autenticação
[Authorize(Roles = "Admin,CreditAnalyst")]  // Requer role específica
```

**Roles**
- `Admin`: Acesso total
- `CreditAnalyst`: Consulta e atualização de scores
- `ReadOnly`: Apenas consulta

---

## 💾 Persistência de Dados

### Schema do Banco de Dados

**Tabela: Clientes**
```sql
CREATE TABLE Clientes (
    Id TEXT PRIMARY KEY,
    Nome TEXT NOT NULL,
    Email TEXT NOT NULL,
    Telefone TEXT NOT NULL,
    Cpf TEXT NOT NULL UNIQUE,
    Endereco TEXT NOT NULL,
    Cidade TEXT NOT NULL,
    Estado TEXT NOT NULL,
    Cep TEXT NOT NULL,
    
    -- Crédito (Core)
    ScoreCredito INTEGER DEFAULT 0,
    LimiteCreditoPorCartao REAL DEFAULT 0,
    NumeroMaximoCartoes INTEGER DEFAULT 0,
    DataUltimaAvaliacaoCredito TEXT,
    
    -- Auditoria
    DataCriacao TEXT NOT NULL,
    DataAtualizacao TEXT,
    Ativo INTEGER NOT NULL DEFAULT 1,
    
    CONSTRAINT CK_Clientes_Score 
        CHECK (ScoreCredito BETWEEN 0 AND 1000),
    CONSTRAINT CK_Clientes_Cartoes 
        CHECK (NumeroMaximoCartoes BETWEEN 0 AND 2)
);

CREATE INDEX IX_Clientes_Cpf ON Clientes(Cpf);
CREATE INDEX IX_Clientes_Score ON Clientes(ScoreCredito);
CREATE INDEX IX_Clientes_DataAvaliacao ON Clientes(DataUltimaAvaliacaoCredito);
```

**Tabela: Usuarios**
```sql
CREATE TABLE Usuarios (
    Id TEXT PRIMARY KEY,
    Nome TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    SenhaHash TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'ReadOnly',
    EmailConfirmado INTEGER DEFAULT 0,
    DataCriacao TEXT NOT NULL,
    DataAtualizacao TEXT,
    Ativo INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
```

---

## 📐 Padrões e Práticas

### Design Patterns

#### Repository Pattern
```csharp
public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id);
    Task<Cliente?> ObterPorCpfAsync(string cpf);
    Task AdicionarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
    Task<List<Cliente>> ObterClientesParaReavaliacao();
}
```

#### Service Pattern
```csharp
public interface IClienteService
{
    Task<ApiResponseDto<ClienteResponseDto>> ObterPorIdAsync(Guid id);
    Task<ApiResponseDto<ClienteResponseDto>> AtualizarCreditoAsync(
        Guid id, AtualizarCreditoDto dto);
}
```

#### Event Handler Pattern
```csharp
public interface IMessageHandler<T> where T : DomainEvent
{
    Task HandleAsync(T evento);
}

public class ClienteCriadoHandler : IMessageHandler<ClienteCriadoEvent>
{
    public async Task HandleAsync(ClienteCriadoEvent evento)
    {
        // Processar evento
    }
}
```

### Princípios SOLID

✅ **Single Responsibility**: Classes com responsabilidade única
✅ **Open/Closed**: Extensível via interfaces
✅ **Liskov Substitution**: Herança apropriada
✅ **Interface Segregation**: Interfaces específicas
✅ **Dependency Inversion**: Dependência de abstrações

---

## ⚙️ Configurações

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=validacao_credito.db;"
  },
  "Jwt": {
    "Secret": "sua_chave_super_secreta_com_minimo_32_caracteres_para_producao",
    "Issuer": "CadastroClientesApi",
    "Audience": "CadastroClientesApp",
    "ExpirationMinutes": 60
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "Enabled": true
  },
  "CreditoAvaliacao": {
    "ScoreSemAprovacao": 100,
    "ScoreUmCartao": 500,
    "ScoreDoisCartoes": 501,
    "LimiteBasico": 1000.00,
    "LimiteAvancado": 5000.00,
    "IntervaloReavaliacao": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Variáveis de Ambiente (Docker)

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5002
ConnectionStrings__DefaultConnection=Data Source=/app/data/credito.db;
JWT_SECRET=producao-secret-key-32-chars
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```

---

## 📊 Métricas de Negócio

### KPIs de Crédito

| Métrica | Descrição | Objetivo |
|---------|-----------|----------|
| **Taxa de Aprovação** | % clientes aprovados (score > 100) | > 70% |
| **Score Médio** | Média de score dos clientes | > 500 |
| **Taxa Premium** | % clientes com score > 500 | > 40% |
| **Tempo de Avaliação** | Tempo médio de processamento | < 2s |
| **Reavaliações/Dia** | Quantidade de reavaliações automáticas | - |

### Distribuição de Score (Exemplo)

```
Faixa 0-100:    15% dos clientes (Reprovados)
Faixa 101-500:  45% dos clientes (Básico - 1 cartão)
Faixa 501-1000: 40% dos clientes (Premium - 2 cartões)
```

---

## 🔗 Integração com Outros Serviços

### Dependências

**Upstream (Consome eventos)**
- Cliente API: Eventos de criação/atualização

**Downstream (Publica para)**
- Cliente API: Atualiza ranking de crédito
- Cartão API: Dados para emissão de cartões
- Notificação API: Alertas de aprovação/reprovação

### Fluxo Completo da Arquitetura

```
1. [Cliente API] 
   ↓ cria cliente
   ↓ pub: cliente.criado
   
2. [Crédito API] (este serviço)
   ↓ sub: cliente.criado
   ↓ avalia crédito
   ↓ pub: credito.avaliado
   
3. [Cliente API]
   ↓ sub: credito.avaliado
   ↓ atualiza ranking
   ↓ pub: cliente.elegivel.cartao
   
4. [Cartão API]
   ↓ sub: cliente.elegivel.cartao
   ↓ emite cartões
   ↓ pub: card.issued
```

---

## 📚 Referências

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)
- [RabbitMQ Patterns](https://www.rabbitmq.com/getstarted.html)
- [Credit Scoring Best Practices](https://www.fico.com/credit-scoring)

---

**Última Atualização**: 03 de Novembro de 2024
**Versão**: 1.0.0
**Mantenedor**: Equipe de Desenvolvimento Backend
