# HubMeteorolόgico

API REST em .NET 8 para consulta de registros meteorológicos interpolados de estações espalhadas por fazendas, com suporte geoespacial via PostGIS e cache distribuído com Redis.

---

## Sumário

- [Implementação](#implementação)
  - [Pré-requisitos](#pré-requisitos)
  - [Restauração do banco de dados](#restauração-do-banco-de-dados)
  - [Executando com Docker Compose](#executando-com-docker-compose)
  - [Executando localmente](#executando-localmente)
  - [Endpoint principal](#endpoint-principal)
  - [Testes](#testes)
- [Sobre o Desenvolvimento e Arquitetura Adotada](#sobre-o-desenvolvimento-e-arquitetura-adotada)
  - [Stack tecnológica](#stack-tecnológica)
  - [Estrutura de projetos](#estrutura-de-projetos)
  - [Camadas e responsabilidades](#camadas-e-responsabilidades)
  - [Banco de dados e dados geoespaciais](#banco-de-dados-e-dados-geoespaciais)
  - [Cache com Redis](#cache-com-redis)
  - [Validação e tratamento de erros](#validação-e-tratamento-de-erros)
- [Respostas às Perguntas do Desafio](#respostas-às-perguntas-do-desafio)
  - [1. Busca e persistência de dados externos (estações de terceiros)](#1-busca-e-persistência-de-dados-externos-estações-de-terceiros)
  - [2. Atualização de dados cadastrais dos equipamentos](#2-atualização-de-dados-cadastrais-dos-equipamentos)
  - [3. Performance, escalabilidade e retenção de dados](#3-performance-escalabilidade-e-retenção-de-dados)
  - [4. Autenticação e autorização com OAuth](#4-autenticação-e-autorização-com-oauth)

---

## Implementação

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8)
- [Docker](https://www.docker.com/) e Docker Compose
- PostgreSQL 16 + extensão PostGIS (provida via Docker)
- Redis 7 (provido via Docker)

### Restauração do banco de dados

Com o container PostgreSQL em execução, restaure o dump fornecido:

```bash
# Copia o dump para o container
docker cp hubmeteorologico.dump hubmeteorologico_db_1:/tmp/

# Restaura o dump
docker exec -it hubmeteorologico_db_1 \
  pg_restore -U postgres -d HubMeteorologico /tmp/hubmeteorologico.dump
```

> O banco `HubMeteorologico` é criado automaticamente na primeira inicialização do container via `POSTGRES_DB`.

### Executando com Docker Compose

```bash
docker-compose up --build
```

Serviços disponíveis após a inicialização:

| Serviço    | URL / Porta                        |
|------------|------------------------------------|
| API        | http://localhost:5000               |
| Swagger UI | http://localhost:5000/swagger       |
| PostgreSQL | localhost:5432                      |
| Redis      | localhost:6379                      |

### Executando localmente

Ajuste as connection strings em `HubMeteorologico.API/appsettings.Development.json` para apontar para instâncias locais de PostgreSQL e Redis, depois:

```bash
cd HubMeteorologico.API
dotnet run
```

### Endpoint principal

```
GET /registros-interpolados
```

**Query parameters:**

| Parâmetro       | Tipo     | Obrigatório | Descrição                                           |
|-----------------|----------|-------------|-----------------------------------------------------|
| `fazendaId`     | `int`    | Sim         | Identificador da fazenda                            |
| `codigoLavoura` | `string` | Sim         | Código da lavoura para filtrar e diminuir payload   |
| `dataHora`      | `DateTime` | Sim       | Hora cheia (ex: `2024-01-15T12:00:00`)              |

**Exemplo de requisição:**

```
GET /registros-interpolados?fazendaId=1&codigoLavoura=SOJ01&dataHora=2024-01-15T12:00:00
```

**Exemplo de resposta:**

    ON registros_interpolados (fazenda_id, data_hora)
    INCLUDE (mapa_fazenda_lavoura_id);

-- Index parcial para registros consolidados (caso mais consultado)
CREATE INDEX idx_ri_consolidados
    ON registros_interpolados (fazenda_id, data_hora)
    WHERE consolidada = true;
```

**5. Retenção de dados históricos**

Dados meteorológicos têm valor decrescente com o tempo para operações em tempo real, mas alto valor para análises históricas e machine learning.

**Política de retenção em camadas:**

| Janela | Armazenamento | Resolução |
|---|---|---|
| 0–90 dias | PostgreSQL (partições ativas) | Completa |
| 90 dias–2 anos | PostgreSQL (partições frias, tablespace lento) | Completa |
| 2–5 anos | TimescaleDB / S3 Parquet via `pg_partman` | Agregada por dia |
| > 5 anos | S3 cold storage (Glacier) | Somente sob demanda |

**6. Escalabilidade horizontal da API**

A API é stateless (sem sessão em memória, cache no Redis externo), permitindo escalonamento horizontal com:

- Múltiplas instâncias atrás de load balancer (NGINX / AWS ALB)
- Health check em `/health` para remoção automática de instâncias instáveis
- Workers de ingestão em instâncias separadas, escalando independentemente da API de leitura

**7. Monitoramento e alertas**

- Serilog → OpenTelemetry → Prometheus / Grafana para métricas de throughput, latência e cache hit rate
- Alertas para: taxa de cache miss acima de 30%, latência P99 acima de 500ms, filas RabbitMQ acima de threshold

---

### 4. Autenticação e autorização com OAuth

> Descreva como implementaria autenticação e autorização nas APIs usando OAuth.

#### Arquitetura OAuth 2.0 + OpenID Connect

Para um sistema B2B como o HubMeteorolόgico, a abordagem recomendada é usar um **Authorization Server dedicado** (Keycloak, Auth0, Azure AD B2C) com o fluxo **Client Credentials** para integrações M2M e **Authorization Code + PKCE** para interfaces de usuário.

```
[Frontend / Sistema externo]
        │
        │ 1. POST /oauth/token (client_credentials ou authorization_code)
        ▼
[Authorization Server — Keycloak / Auth0]
        │
        │ 2. JWT (access_token) assinado com RS256
        ▼
[HubMeteorologico API]
        │
        │ 3. Valida assinatura JWT via JWKS endpoint do Auth Server
        │ 4. Verifica claims (scope, fazenda_id, roles)
        ▼
[Recurso protegido — /registros-interpolados]
```

#### Implementação em ASP.NET Core 8

**Configuração do middleware:**

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.hubmeteorologico.com.br/realms/hub";
        options.Audience  = "hubmeteorologico-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            // Chave pública obtida automaticamente via JWKS
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LeituraRegistros", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "registros:read"));

    options.AddPolicy("GestaoEquipamentos", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "equipamentos:write")
              .RequireRole("administrador"));
});
```

**Aplicação nos controllers:**

```csharp
[ApiController]
[Authorize(Policy = "LeituraRegistros")]
[Route("registros-interpolados")]
public class RegistrosInterpoladosController : ControllerBase
{
    // Apenas tokens com scope "registros:read" chegam aqui
}
```

**Autorização baseada em recurso (multi-tenant por fazenda):**

```csharp
// Claim customizada no JWT: "fazenda_ids": [1, 3, 7]
// O serviço verifica se o fazendaId solicitado está nas claims do token

var fazendasPermitidas = User.FindAll("fazenda_ids")
    .Select(c => int.Parse(c.Value))
    .ToList();

if (!fazendasPermitidas.Contains(filtro.FazendaId))
    throw new ForbiddenException("Acesso negado à fazenda solicitada.");
```

#### Fluxos OAuth por tipo de cliente

| Cliente | Fluxo OAuth | Justificativa |
|---|---|---|
| Frontend SPA / Mobile | Authorization Code + PKCE | Sem client_secret exposto; seguro para browsers |
| Worker de ingestão externo | Client Credentials | M2M sem interação humana |
| Dashboard interno | Authorization Code + PKCE | Usuário autenticado com SSO corporativo |
| Webhook de terceiros | API Key + JWT wrapper | Simplicidade para integrações simples |

#### Renovação de tokens e segurança

- **Access token TTL**: 15 minutos (reduz janela de comprometimento)
- **Refresh token TTL**: 24 horas com rotação (cada uso gera novo refresh token)
- **Token revogation**: via `jti` claim + lista negra no Redis para casos de logout imediato
- **Algoritmo**: RS256 (assimétrico) — a API valida com a chave pública, sem precisar da chave privada

#### Escopos definidos

| Escopo | Permissão |
|---|---|
| `registros:read` | Consultar registros interpolados e meteorológicos |
| `equipamentos:read` | Listar equipamentos e fontes |
| `equipamentos:write` | Criar e atualizar cadastro de equipamentos |
| `admin` | Acesso total, incluindo gestão de usuários e fazendas |

---

## Estrutura de diretórios

```
HubMeteorologico/
├── HubMeteorologico.API/
│   ├── Controllers/
│   │   └── RegistrosInterpoladosController.cs
│   ├── ConfigController/
│   │   ├── BaseController.cs
│   │   └── ExceptionMiddleware.cs (via Middleware/)
│   ├── Validators/
│   │   └── RegistrosInterpoladosFilterDtoValidator.cs
│   ├── Program.cs
│   └── appsettings.json
├── HubMeteorologico.Domain/
│   ├── Entities/          # Fazendas, Equipamentos, RegistrosInterpolados...
│   ├── DTOs/              # RegistrosInterpoladosDto, FilterDto
│   ├── Exceptions/        # BadRequestException, NotFoundException...
│   └── ResponseDefault/   # ReturnDefault<T>
├── HubMeteorologico.Application/
│   ├── Interfaces/
│   │   └── IRegistrosInterpoladosService.cs
│   └── Services/
│       └── RegistrosInterpoladosService.cs
├── HubMeteorologico.Infrastructure/
│   ├── Repository/
│   │   ├── Repository.cs         # Repositório genérico com reflection
│   │   ├── DbSession.cs
│   │   ├── UnitOfWork.cs
│   │   └── RegistrosInterpoladosRepository.cs
│   └── Helpers/
│       └── GeographyParameter.cs
├── HubMeteorologico.Tests/
│   ├── Domain/
│   └── Services/
│       └── RegistrosInterpoladosServiceTests.cs
└── docker-compose.yml
