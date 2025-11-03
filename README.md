# Sistema de Validação de Crédito API

Sistema robusto de API RESTful para validação e análise de crédito, desenvolvido em **.NET 8** com arquitetura em camadas (Clean Architecture) e padrões enterprise.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Requisitos do Sistema](#requisitos-do-sistema)
- [Instalação e Configuração](#instalação-e-configuração)
- [Como Usar](#como-usar)
- [Endpoints da API](#endpoints-da-api)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Docker](#docker)
- [Testes](#testes)
- [Logs e Monitoramento](#logs-e-monitoramento)
- [Segurança](#segurança)
- [Troubleshooting](#troubleshooting)
- [Contribuição](#contribuição)

---

## 🎯 Visão Geral

A **API de Validação de Crédito** é um serviço especializado em:

- ✅ **Autenticação segura** com JWT (JSON Web Tokens)
- ✅ **Validação e análise de crédito** com scoring automático
- ✅ **Integração com RabbitMQ** para processamento assíncrono
- ✅ **Persistência em SQLite** com Entity Framework Core
- ✅ **Logging estruturado** com Serilog
- ✅ **Health checks** e monitoramento
- ✅ **Documentação automática** com Swagger/OpenAPI
- ✅ **Resiliência** com Polly (retry, circuit breaker, timeout)

---

## 🏛️ Arquitetura

O projeto segue **Clean Architecture** com separação clara de responsabilidades:

```
Sistema de Validação de Crédito
│
├── 🚀 Driving.Api                    (Camada de Apresentação)
│   ├── Controllers/
│   ├── Extensions/
│   ├── Properties/
│   ├── Program.cs
│   └── appsettings.json
│
├── 📊 Core.Application               (Camada de Aplicação)
│   ├── DTOs/                         (Data Transfer Objects)
│   ├── Interfaces/Services/          (Contratos de serviços)
│   ├── Mappers/                      (AutoMapper configs)
│   └── Services/                     (Lógica de negócio)
│
├── 🎯 Core.Domain                    (Camada de Domínio)
│   ├── Entities/                     (Modelos de domínio)
│   ├── Enums/
│   └── ValueObjects/
│
├── 💾 Core.Infra                     (Camada de Infraestrutura)
│   ├── Email/
│   ├── Cache/
│   └── Logging/
│
├── 🗄️ Driven.SqlLite                 (Adaptador de Dados)
│   ├── Data/
│   ├── Repositories/
│   └── Migrations/
│
├── 📨 Driven.RabbitMQ                (Adaptador de Mensageria)
│   ├── Services/
│   └── Subscribers/
│
└── 🧪 Test.XUnit                     (Testes Unitários)
    └── Tests/
```

### Fluxo de Dependências

```
Driving.Api (Controllers)
    ↓
Core.Application (Services, Interfaces)
    ↓
Core.Domain (Entities, Business Rules)
    ↓
Driven.SqlLite + Driven.RabbitMQ (External Services)
    ↓
Core.Infra (Logging, Caching, Email)
```

---

## 📋 Requisitos do Sistema

### Mínimos
- **.NET 8 SDK** (versão 8.0 ou superior)
- **Visual Studio 2022** ou VS Code
- **Git**

### Recomendados (para recursos completos)
- **Docker** e **Docker Compose** (para RabbitMQ e ambientes containerizados)
- **RabbitMQ** (para processamento assíncrono)
- **SQL Server** (opcional, atualmente usa SQLite)
- **Postman** ou **Thunder Client** (para testes de API)

### Versões de Dependências
```
.NET Framework: 8.0
C#: 12.0
Swashbuckle.AspNetCore: 7.0.0
Entity Framework Core: 8.0.11
Serilog: 3.1.1
Polly: 8.2.0
JWT Bearer: 8.0.11
RabbitMQ Client: Integrado
SQLite: Via EF Core
```

---

## ⚙️ Instalação e Configuração

### 1. Clonar o Repositório

```bash
git clone <seu-repositorio>
cd Credito
```

### 2. Restaurar Dependências

```bash
dotnet restore Validacao.Credito.sln
```

### 3. Configurar Variáveis de Ambiente

Criar arquivo `appsettings.Development.json` na pasta `Driving.Api/`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=credito.db;"
  },
  "Jwt": {
    "Secret": "sua_chave_super_secreta_com_minimo_32_caracteres",
    "Issuer": "CadastroClientesApi",
    "Audience": "CadastroClientesApp",
    "ExpirationMinutes": 120
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### 4. Compilar Solução

```bash
dotnet build Validacao.Credito.sln -c Debug
```

### 5. Iniciar a API

#### Opção 1: Via CLI
```bash
cd Driving.Api
dotnet run --configuration Debug
```

#### Opção 2: Via Visual Studio
1. Abrir `Validacao.Credito.sln`
2. Selecionar projeto `Driving.Api` como startup
3. Pressionar `F5`

#### Opção 3: Via Docker
```bash
docker build -t credito-api .
docker run -p 5002:5002 credito-api
```

---

## 🚀 Como Usar

### Acessar Swagger UI

A documentação interativa está disponível em:

- **HTTP**: http://localhost:5202/swagger
- **HTTPS**: https://localhost:7215/swagger
- **Ambiente de Produção**: http://seu-servidor:5002/swagger

### Fluxo de Autenticação

#### 1. Login

```bash
POST /api/Auth/login
Content-Type: application/json

{
  "usuario": "user",
  "senha": "password"
}
```

**Resposta (200 OK):**
```json
{
  "sucesso": true,
  "mensagem": "Login realizado com sucesso",
  "dados": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiracaoEm": "2024-11-03T15:30:00Z"
  },
  "erros": null
}
```

#### 2. Usar Token em Requisições

Adicionar header `Authorization` em todas as requisições subsequentes:

```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Exemplos de Uso

#### Exemplo 1: Obter Token
```bash
curl -X POST "http://localhost:5202/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "user",
    "senha": "password"
  }'
```

#### Exemplo 2: Usar Token em Requisição Autenticada
```bash
curl -X GET "http://localhost:5202/api/credito/validar" \
  -H "Authorization: Bearer TOKEN_AQUI" \
  -H "Content-Type: application/json"
```

---

## 📡 Endpoints da API

### Autenticação

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/Auth/login` | Realizar login e obter token JWT | ❌ Não |

**Request (POST /api/Auth/login):**
```json
{
  "usuario": "user",
  "senha": "password"
}
```

**Response (200):**
```json
{
  "sucesso": true,
  "mensagem": "Login realizado com sucesso",
  "dados": {
    "token": "string",
    "expiracaoEm": "2024-11-03T15:30:00Z"
  },
  "erros": null
}
```

---

## 📁 Estrutura do Projeto

### Driving.Api
**Camada de Apresentação (API REST)**
- Controladores que expõem endpoints
- Configuração do pipeline HTTP
- Swagger/OpenAPI configuration
- Extensões e middlewares

### Core.Application
**Camada de Aplicação (Lógica de Negócio)**
- `Services/`: Implementação de serviços de aplicação
- `Interfaces/Services/`: Contratos dos serviços
- `DTOs/`: Modelos de transferência de dados
- `Mappers/`: Mapeamento entre entities e DTOs

### Core.Domain
**Camada de Domínio (Regras de Negócio)**
- `Entities/`: Modelos principais (Usuario, Credito, etc)
- `Enums/`: Enumerações de domínio
- `ValueObjects/`: Objetos de valor imutáveis

### Core.Infra
**Camada de Infraestrutura**
- `Logging/`: Serilog configuration
- `Cache/`: Implementação de cache em memória
- `Email/`: Serviço de envio de e-mail

### Driven.SqlLite
**Adaptador de Dados (Persistência)**
- `Data/ApplicationDbContext`: DbContext do EF Core
- `Repositories/`: Implementação de repositórios
- `Migrations/`: Migrações do banco de dados

### Driven.RabbitMQ
**Adaptador de Mensageria**
- `Services/MessageBus`: Produtor de mensagens
- `Subscribers/`: Consumidores de mensagens
- Integração com RabbitMQ

### Test.XUnit
**Testes Unitários**
- Testes de serviços
- Testes de controllers
- Testes de repositórios

---

## 🔐 Variáveis de Ambiente

### Arquivo: `appsettings.json` (Produção)

```json
{
  "ApplicationSettings": {
    "ServiceName": "CreditoAPI",
    "ServiceVersion": "1.0.0",
    "Environment": "Production"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=credito.db;"
  },
  "Security": {
    "Jwt": {
      "Secret": "sua_chave_super_secreta_com_minimo_32_caracteres_para_producao",
      "Issuer": "CadastroClientesApi",
      "Audience": "CadastroClientesApp",
      "ExpirationMinutes": 60
    }
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "MaxRetries": 3,
    "RetryDelay": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Serilog": {
      "MinimumLevel": "Information",
      "WriteTo": [
        {
          "Name": "Console",
          "Args": {}
        },
        {
          "Name": "File",
          "Args": {
            "path": "logs/credito-api-.txt",
            "rollingInterval": "Day"
          }
        }
      ]
    }
  },
  "HealthChecks": {
    "Enabled": true,
    "Endpoint": "/health",
    "DetailedEndpoint": "/health/detailed"
  }
}
```

### Variáveis de Ambiente do Sistema

```bash
# Autenticação
JWT_SECRET=sua_chave_super_secreta_com_minimo_32_caracteres

# Banco de Dados
ConnectionStrings__DefaultConnection=Data Source=/app/data/credito.db;

# RabbitMQ
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5002
ASPNETCORE_HTTP_PORT=5002
```

---

## 🐳 Docker

### Compilar Imagem Docker

```bash
docker build -t validacao-credito:latest .
```

### Executar Container

```bash
docker run -d \
  --name credito-api \
  -p 5002:5002 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JWT_SECRET="sua_chave_secreta" \
  -e RABBITMQ_HOST=rabbitmq \
  validacao-credito:latest
```

### Docker Compose

Criar arquivo `docker-compose.yml`:

```yaml
version: "3.9"

services:
  credito-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: credito-api
    ports:
      - "5002:5002"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      JWT_SECRET: "sua_chave_super_secreta_com_minimo_32_caracteres"
      RABBITMQ_HOST: rabbitmq
      RABBITMQ_PORT: 5672
      RABBITMQ_USER: guest
      RABBITMQ_PASSWORD: guest
    depends_on:
      - rabbitmq
    networks:
      - credito-net

  rabbitmq:
    image: rabbitmq:4.2-management
    container_name: rabbitmq-credito
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    networks:
      - credito-net

networks:
  credito-net:
    driver: bridge
```

Executar:
```bash
docker-compose up -d
```

---

## 🧪 Testes

### Executar Testes Unitários

```bash
# Todos os testes
dotnet test Validacao.Credito.sln

# Apenas um projeto
dotnet test Test.XUnit/Test.XUnit.csproj

# Com coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Estrutura de Testes

```
Test.XUnit/
├── Services/           # Testes de serviços
├── Repositories/       # Testes de repositórios
├── Controllers/        # Testes de controllers
└── Fixtures/          # Dados de teste
```

---

## 📊 Logs e Monitoramento

### Serilog Configuration

Os logs são configurados em `Driving.Api/Extensions/SerilogExtensions.cs`:

**Destinos:**
- 📺 **Console**: Saída padrão
- 📁 **Arquivo**: `logs/credito-api-YYYY-MM-DD.txt` (rolling diário)
- 📊 **Structured Logging**: Informações contextuais

**Exemplo de Log:**
```
[2024-11-03 14:21:10 GMT] [INF] Request starting HTTP/1.1 GET http://localhost:5202/swagger
[2024-11-03 14:21:10 GMT] [INF] Request finished HTTP/1.1 GET http://localhost:5202/swagger - 200
```

### Health Checks

Verificar saúde da aplicação:

```bash
curl http://localhost:5202/health
```

**Resposta:**
```json
{
  "status": "Healthy",
  "checks": {
    "Database": "Healthy",
    "RabbitMQ": "Healthy"
  }
}
```

---

## 🔒 Segurança

### JWT (JSON Web Tokens)

- **Algoritmo**: HS256 (HMAC-SHA256)
- **Duração**: 60 minutos (configurável)
- **Emitente**: CadastroClientesApi
- **Audiência**: CadastroClientesApp

**Exemplo de Token Decodificado:**
```json
{
  "sub": "user123",
  "iat": 1699008070,
  "exp": 1699011670,
  "iss": "CadastroClientesApi",
  "aud": "CadastroClientesApp"
}
```

### CORS

Configurado em `Program.cs` para permitir acesso de múltiplas origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### HTTPS

- Em **Desenvolvimento**: HTTPS com certificado auto-assinado (porta 7215)
- Em **Produção**: Configurar certificado SSL/TLS válido

---

## 🛠️ Troubleshooting

### Problema: Swagger não carrega

**Solução:**
1. Verificar se a API está rodando em `http://localhost:5202`
2. Acessar diretamente: `http://localhost:5202/swagger`
3. Limpar cache do navegador (Ctrl+Shift+Delete)

### Problema: RabbitMQ desconectado

**Aviso esperado:**
```
⚠️  Aviso: RabbitMQ não foi inicializado. Falha ao conectar ao RabbitMQ após 3 tentativas
A aplicação continuará funcionando sem mensageria.
```

**Solução:**
1. Iniciar RabbitMQ: `docker-compose up -d rabbitmq`
2. Reiniciar a API

### Problema: Token JWT inválido

**Erro:**
```
401 Unauthorized: Invalid token
```

**Soluções:**
1. Verificar se o token foi enviado no header `Authorization: Bearer <token>`
2. Verificar expiração do token
3. Gerar novo token via `/api/Auth/login`

### Problema: Banco de dados bloqueado

**Erro:**
```
SQLite: database is locked
```

**Soluções:**
1. Fechar todas as conexões abertas
2. Deletar arquivo `credito.db-wal` e `credito.db-shm`
3. Reiniciar a aplicação

---

## 📝 Contribuição

### Padrões de Código

- **Naming**: PascalCase para classes, camelCase para variáveis
- **Format**: Seguir `EditorConfig` do projeto
- **Commits**: Usar mensagens descritivas em português

### Exemplo de Commit

```bash
git add .
git commit -m "feat: adicionar endpoint de validação de crédito"
git push origin main
```

### Pull Request

1. Criar branch: `git checkout -b feature/nova-feature`
2. Fazer commits: `git commit -m "mensagem descritiva"`
3. Push: `git push origin feature/nova-feature`
4. Abrir Pull Request no GitHub

---

## 📞 Suporte

Para dúvidas ou issues:

1. Verificar a [seção Troubleshooting](#troubleshooting)
2. Abrir uma issue no GitHub
3. Contatar o time de desenvolvimento

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

---

## 🚀 Roadmap

- [ ] Integração com Serviço de Validação de Documentos
- [ ] Dashboard de Analytics
- [ ] Rate Limiting por API Key
- [ ] GraphQL Endpoint
- [ ] Migrarem para SQL Server
- [ ] Cache distribuído com Redis
- [ ] Testes de Carga e Performance

---

**Última atualização**: 03/11/2024 | **Versão**: 1.0.0 | **Status**: Em Produção
