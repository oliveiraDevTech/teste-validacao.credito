# Quick Start - Sistema de Validação de Crédito

Guia rápido para colocar o projeto rodando em 5 minutos.

## ⚡ Início Rápido (5 minutos)

### 1️⃣ Clone & Configure

```bash
# Clone o repositório
git clone <seu-repositorio>
cd Credito

# Restaure as dependências
dotnet restore
```

### 2️⃣ Rode a API

```bash
cd Driving.Api
dotnet run --configuration Debug
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5202
      Now listening on: https://localhost:7215
```

### 3️⃣ Acesse o Swagger

Abra seu navegador em:
```
http://localhost:5202/swagger
```

Você verá a documentação interativa com todos os endpoints.

---

## 🔑 Próximos Passos (2 minutos)

### Fazer Login

No Swagger, abra `POST /api/Auth/login` e execute:

```json
{
  "usuario": "user",
  "senha": "password"
}
```

Você receberá um token JWT:

```json
{
  "sucesso": true,
  "dados": {
    "token": "eyJhbGc...",
    "expiracaoEm": "2024-11-03T15:30:00Z"
  }
}
```

### Copiar Token

1. Copie o valor do campo `token`
2. No Swagger, clique no botão verde "Authorize"
3. Cole: `Bearer seu_token_aqui`
4. Clique "Authorize"

Agora todos os endpoints autenticados estarão disponíveis!

---

## 🐳 Com Docker (2 minutos)

### Opção 1: Apenas API

```bash
# Compilar imagem
docker build -t credito-api .

# Rodar
docker run -p 5002:5002 credito-api
```

Acesse: `http://localhost:5002/swagger`

### Opção 2: API + RabbitMQ

```bash
# Usar docker-compose
docker-compose up -d

# Logs em tempo real
docker-compose logs -f credito-api
```

Acesse: `http://localhost:5002/swagger`

---

## 🧪 Testar Endpoints (5 minutos)

### Usando cURL

```bash
# 1. Login
RESPONSE=$(curl -X POST "http://localhost:5202/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "user",
    "senha": "password"
  }')

echo $RESPONSE | jq .

# 2. Extrair token
TOKEN=$(echo $RESPONSE | jq -r '.dados.token')

# 3. Usar token em requisição autenticada
curl -X GET "http://localhost:5202/api/health" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Usando Postman

1. Download: https://www.postman.com/downloads/
2. Crie uma nova requisição `POST`
3. URL: `http://localhost:5202/api/Auth/login`
4. Body (JSON):
```json
{
  "usuario": "user",
  "senha": "password"
}
```
5. Send
6. Copie o token da resposta
7. Em Headers adicione: `Authorization: Bearer <token>`

---

## 🗄️ Banco de Dados

### Criar/Atualizar Banco

```bash
# Entrar na pasta Driving.Api
cd Driving.Api

# Aplicar migrações
dotnet ef database update -p ../Driven.SqlLite

# Verificar status
dotnet ef migrations list -p ../Driven.SqlLite
```

### Arquivo do Banco

O SQLite cria um arquivo local:
```
Driving.Api/credito.db
Driving.Api/credito.db-wal
Driving.Api/credito.db-shm
```

**Deletar banco e recriar:**
```bash
# Remover arquivo
rm credito.db

# Recriar
dotnet ef database update -p ../Driven.SqlLite
```

---

## 📊 Health Check

Verificar se API está saudável:

```bash
curl http://localhost:5202/health
```

Resposta esperada:
```json
{
  "status": "Healthy",
  "checks": {
    "Database": "Healthy",
    "RabbitMQ": "Degraded"
  }
}
```

---

## 🔧 Troubleshooting Rápido

### Erro: "Address already in use"

```bash
# Windows - Encontrar e matar processo na porta 5202
netstat -ano | findstr :5202
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5202 | xargs kill -9
```

### Erro: "SQLite database is locked"

```bash
# Deletar arquivos WAL
rm credito.db-wal
rm credito.db-shm
```

### RabbitMQ "Connection refused"

Esperado em desenvolvimento sem RabbitMQ:
```
⚠️  Aviso: RabbitMQ não foi inicializado
A aplicação continuará funcionando sem mensageria.
```

Para ativar RabbitMQ:
```bash
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:4.2-management
```

### Swagger em branco

1. Limpar cache: `Ctrl+Shift+Delete`
2. Força atualizar: `Ctrl+F5`
3. Verificar console: `F12` → `Console`

---

## 📝 Estrutura de Pastas

```
Credito/
├── Driving.Api/              # API REST
├── Core.Application/         # Serviços
├── Core.Domain/              # Modelos
├── Core.Infra/               # Infraestrutura
├── Driven.SqlLite/           # Banco de dados
├── Driven.RabbitMQ/          # Mensageria
├── Test.XUnit/               # Testes
├── README.md                 # Documentação completa
├── DEVELOPMENT.md            # Guia de desenvolvimento
├── docker-compose.yml        # Docker Compose
├── Dockerfile                # Docker
├── .gitignore                # Git ignore
└── Validacao.Credito.sln    # Solution
```

---

## 🚀 Próximas Etapas

1. Leia **[README.md](README.md)** para documentação completa
2. Leia **[DEVELOPMENT.md](DEVELOPMENT.md)** para padrões de código
3. Explore os Controllers em `Driving.Api/Controllers/`
4. Crie novos endpoints usando o padrão estabelecido

---

## 💻 Comandos Úteis

```bash
# Compilar
dotnet build

# Rodar testes
dotnet test

# Limpar build
dotnet clean

# Restaurar packages
dotnet restore

# Criar migração
dotnet ef migrations add NomeMigracao -p Driven.SqlLite -s Driving.Api

# Atualizar banco
dotnet ef database update -p Driven.SqlLite -s Driving.Api

# Rodar com debug
dotnet run -c Debug

# Rodar com release
dotnet run -c Release
```

---

## 📞 Precisa de Ajuda?

- Swagger interativo: http://localhost:5202/swagger
- Logs: `Driving.Api/logs/`
- GitHub Issues: Abra uma issue no repositório

---

**Pronto! Você está rodando a API! 🚀**

Acesse: http://localhost:5202/swagger
