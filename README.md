# Hub Meteorológico

API REST desenvolvida em .NET 8 para consulta de registros meteorológicos interpolados de estações distribuídas em fazendas. A solução utiliza PostgreSQL com PostGIS para dados geoespaciais e Redis para cache distribuído.

---

## Sumário

- [Implementação](#implementação)
  - [Pré-requisitos](#pré-requisitos)
  - [Restauração do banco de dados](#restauração-do-banco-de-dados)
  - [Executando com Docker Compose](#executando-com-docker-compose)
  - [Executando localmente](#executando-localmente)
  - [Endpoint principal](#endpoint-principal)
  - [Testes](#testes)
- [Sobre o desenvolvimento e a arquitetura adotada](#sobre-o-desenvolvimento-e-a-arquitetura-adotada)
  - [Stack tecnológica](#stack-tecnológica)
  - [Estrutura de projetos](#estrutura-de-projetos)
  - [Camadas e responsabilidades](#camadas-e-responsabilidades)
  - [Banco de dados e dados geoespaciais](#banco-de-dados-e-dados-geoespaciais)
  - [Cache com Redis](#cache-com-redis)
  - [Validação e tratamento de erros](#validação-e-tratamento-de-erros)
- [Respostas às perguntas do desafio](#respostas-às-perguntas-do-desafio)
  - [1. Busca e persistência de dados externos](#1-busca-e-persistência-de-dados-externos)
  - [2. Atualização de dados cadastrais dos equipamentos](#2-atualização-de-dados-cadastrais-dos-equipamentos)
  - [3. Performance, escalabilidade e retenção de dados](#3-performance-escalabilidade-e-retenção-de-dados)
  - [4. Autenticação e autorização com OAuth](#4-autenticação-e-autorização-com-oauth)
- [Estrutura de diretórios](#estrutura-de-diretórios)

---

## Implementação

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) e Docker Compose
- PostgreSQL 16 com extensão PostGIS
- Redis 7

> PostgreSQL/PostGIS e Redis podem ser executados via Docker Compose, conforme configuração do projeto.

### Restauração do banco de dados

Com o container do PostgreSQL em execução, restaure o dump fornecido:

```bash
# Copia o dump para dentro do container
docker cp hubmeteorologico.dump hubmeteorologico_db_1:/tmp/hubmeteorologico.dump

# Restaura o dump no banco HubMeteorologico
docker exec -it hubmeteorologico_db_1 \
  pg_restore -U postgres -d HubMeteorologico /tmp/hubmeteorologico.dump
```

> O banco `HubMeteorologico` é criado automaticamente na primeira inicialização do container por meio da variável `POSTGRES_DB`.
>
> Caso o nome do container seja diferente no seu ambiente, execute `docker ps` para identificar o nome correto.

### Executando com Docker Compose

Na raiz do projeto, execute:

```bash
docker compose up --build
```

Serviços disponíveis após a inicialização:

| Serviço    | URL / Porta                  |
|------------|------------------------------|
| API        | `http://localhost:5000`       |
| Swagger UI | `http://localhost:5000/swagger` |
| PostgreSQL | `localhost:5432`             |
| Redis      | `localhost:6379`             |

### Executando localmente

Ajuste as connection strings em `HubMeteorologico.API/appsettings.Development.json` para apontar para suas instâncias locais de PostgreSQL e Redis.

Depois, execute:

```bash
cd HubMeteorologico.API
dotnet run
```

### Endpoint principal

```http
GET /registros-interpolados
```

#### Query parameters

| Parâmetro       | Tipo       | Obrigatório | Descrição                                         |
|-----------------|------------|-------------|---------------------------------------------------|
| `fazendaId`     | `int`      | Sim         | Identificador da fazenda.                         |
| `codigoLavoura` | `string`   | Sim         | Código da lavoura utilizado para filtrar os dados. |
| `dataHora`      | `DateTime` | Sim         | Data e hora cheia da consulta. Exemplo: `2024-01-15T12:00:00`. |

#### Exemplo de requisição

```http
GET /registros-interpolados?fazendaId=1&codigoLavoura=SOJ01&dataHora=2024-01-15T12:00:00
```

#### Exemplo de resposta

```json
[
  {
    "fazendaId": 1,
    "codigoLavoura": "SOJ01",
    "dataHora": "2024-01-15T12:00:00",
    "latitude": -30.0346,
    "longitude": -51.2177,
    "temperatura": 28.4,
    "umidade": 76.2,
    "precipitacao": 0.0
  }
]
```

> O formato exato da resposta deve seguir o DTO retornado pela aplicação, como `RegistrosInterpoladosDto`.

### Testes

Para executar os testes automatizados, rode o comando na raiz da solução:

```bash
dotnet test
```

---

## Sobre o desenvolvimento e a arquitetura adotada

### Stack tecnológica

- **.NET 8**: plataforma principal para construção da API.
- **ASP.NET Core Web API**: exposição dos endpoints REST.
- **PostgreSQL 16**: banco de dados relacional.
- **PostGIS**: suporte a dados geoespaciais.
- **Redis**: cache distribuído para melhorar a performance de consultas repetidas.
- **Dapper**: acesso a dados com foco em performance e controle das queries SQL.
- **FluentValidation**: validação dos parâmetros de entrada.
- **xUnit/Moq**: testes automatizados e mocks de dependências.
- **Docker Compose**: orquestração local da API, banco de dados e Redis.

### Estrutura de projetos

A solução foi separada em camadas para manter baixo acoplamento, facilitar testes e deixar as responsabilidades bem definidas.

| Projeto | Responsabilidade |
|---------|------------------|
| `HubMeteorologico.API` | Camada de entrada da aplicação. Contém controllers, middlewares, configurações e injeção de dependência. |
| `HubMeteorologico.Application` | Regras de aplicação, serviços e contratos utilizados pela API. |
| `HubMeteorologico.Domain` | Entidades, DTOs, exceções e objetos compartilhados do domínio. |
| `HubMeteorologico.Infrastructure` | Implementações de repositórios, acesso ao banco, Unit of Work e helpers de infraestrutura. |
| `HubMeteorologico.Tests` | Testes unitários dos serviços e validações principais. |

### Camadas e responsabilidades

A arquitetura segue uma separação próxima à Clean Architecture, sem tornar o projeto excessivamente complexo para o escopo do desafio.

- **API**: recebe a requisição, valida o contrato HTTP e delega o processamento para a camada de aplicação.
- **Application**: concentra os casos de uso, como a busca de registros interpolados.
- **Domain**: representa as entidades, DTOs e regras compartilhadas.
- **Infrastructure**: implementa acesso a dados, Redis, PostGIS e recursos externos.

Essa divisão facilita a troca de tecnologias de infraestrutura sem impactar diretamente as regras de negócio.

### Banco de dados e dados geoespaciais

O projeto utiliza PostgreSQL com PostGIS para armazenar e consultar informações geográficas das fazendas, lavouras e estações meteorológicas.

Exemplo de criação de índices para melhorar as consultas:

```sql
-- Índice geoespacial para consultas por localização
CREATE INDEX idx_mapa_fazenda_centroide
    ON "MapaFazenda"
    USING GIST ("Centroide");

-- Índice para consultas por fazenda e data/hora
CREATE INDEX idx_registros_interpolados_fazenda_data
    ON registros_interpolados (fazenda_id, data_hora)
    INCLUDE (mapa_fazenda_lavoura_id);

-- Índice parcial para registros consolidados, caso seja o cenário mais consultado
CREATE INDEX idx_registros_interpolados_consolidados
    ON registros_interpolados (fazenda_id, data_hora)
    WHERE consolidada = true;
```

### Cache com Redis

O Redis é utilizado para reduzir consultas repetidas ao banco de dados, principalmente em cenários onde o mesmo conjunto de filtros é consultado diversas vezes.

Uma chave de cache pode ser composta pelos filtros principais da consulta:

```text
registros-interpolados:{fazendaId}:{codigoLavoura}:{dataHora}
```

Fluxo sugerido:

1. A API recebe a requisição.
2. O serviço verifica se o resultado existe no Redis.
3. Se existir, retorna o dado em cache.
4. Se não existir, consulta o PostgreSQL/PostGIS.
5. O resultado é armazenado no Redis com tempo de expiração.
6. A resposta é retornada ao cliente.

### Validação e tratamento de erros

A validação dos parâmetros de entrada é feita com FluentValidation, garantindo que os dados obrigatórios sejam informados antes da execução da regra de negócio.

Exemplos de validações:

- `fazendaId` deve ser maior que zero.
- `codigoLavoura` deve ser informado.
- `dataHora` deve ser uma data válida e representar uma hora cheia.

O tratamento de erros deve retornar respostas padronizadas por meio de objetos como `ReturnDefault<T>`, contendo status HTTP, mensagem e dados quando aplicável.

---

## Respostas às perguntas do desafio

### 1. Busca e persistência de dados externos

> Como buscaria e persistiria dados meteorológicos vindos de estações de terceiros?

A integração com estações de terceiros pode ser feita por meio de workers em background, isolados da API de leitura. Esses workers seriam responsáveis por consultar APIs externas, normalizar os dados recebidos e persistir as informações no banco.

Fluxo sugerido:

```text
[API externa de estação]
        ↓
[Worker de ingestão]
        ↓
[Validação e normalização]
        ↓
[PostgreSQL/PostGIS]
        ↓
[Redis / Cache de leitura]
```

Boas práticas para esse fluxo:

- Utilizar jobs agendados ou mensageria para controlar a frequência de ingestão.
- Criar uma camada anticorrupção para isolar o modelo externo do modelo interno da aplicação.
- Registrar logs de falha por estação/equipamento.
- Aplicar política de retry com backoff exponencial.
- Persistir dados brutos quando necessário para auditoria e reprocessamento.
- Evitar sobrescrever dados consolidados sem controle de versão ou histórico.

### 2. Atualização de dados cadastrais dos equipamentos

> Como manteria atualizados os dados cadastrais dos equipamentos?

Os equipamentos podem ser sincronizados por um processo específico de cadastro, separado da ingestão de medições meteorológicas.

A estratégia recomendada é manter uma tabela de equipamentos com campos como:

- Identificador externo do equipamento.
- Nome ou código da estação.
- Fazenda associada.
- Localização geográfica.
- Status operacional.
- Data da última sincronização.
- Data da última comunicação.

Fluxo sugerido:

1. Buscar os equipamentos na API de terceiros.
2. Comparar o identificador externo com os registros internos.
3. Inserir novos equipamentos quando não existirem.
4. Atualizar dados alterados, como localização, nome, status ou associação com fazenda.
5. Marcar como inativos os equipamentos que não aparecem mais na origem ou que ficaram muito tempo sem comunicação.

Para evitar perda de histórico, alterações relevantes podem ser registradas em uma tabela de auditoria.

### 3. Performance, escalabilidade e retenção de dados

> Como garantiria performance e escalabilidade conforme o volume de dados cresce?

A estratégia de performance deve combinar modelagem adequada, índices, cache e separação entre leitura e escrita.

#### 1. Índices e consultas otimizadas

- Criar índices compostos para os filtros mais utilizados.
- Utilizar índice geoespacial `GIST` para campos PostGIS.
- Evitar `SELECT *` em consultas críticas.
- Retornar apenas os campos necessários para o endpoint.
- Avaliar paginação ou limitação de payload quando o volume for alto.

#### 2. Particionamento de tabelas

Como registros meteorológicos tendem a crescer rapidamente, uma boa estratégia é particionar a tabela por data, por exemplo, mês ou ano.

Exemplo:

```sql
CREATE TABLE registros_interpolados_2024_01
PARTITION OF registros_interpolados
FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
```

#### 3. Cache distribuído

O Redis pode ser utilizado para armazenar consultas frequentes, reduzindo a pressão no banco de dados.

Recomendações:

- Definir TTL de acordo com a frequência de atualização dos dados.
- Invalidar cache quando houver atualização relevante.
- Monitorar taxa de cache hit e cache miss.

#### 4. Retenção de dados históricos

Dados meteorológicos têm valor operacional maior no curto prazo, mas também podem ser úteis para análises históricas, relatórios e modelos preditivos.

Política de retenção sugerida:

| Janela | Armazenamento | Resolução |
|--------|---------------|-----------|
| 0 a 90 dias | PostgreSQL com partições ativas | Completa |
| 90 dias a 2 anos | PostgreSQL com partições frias | Completa |
| 2 a 5 anos | TimescaleDB, S3 ou Parquet | Agregada por dia |
| Acima de 5 anos | S3 Glacier ou armazenamento frio | Sob demanda |

#### 5. Escalabilidade horizontal da API

A API deve ser stateless, sem sessão em memória. Com isso, é possível escalar horizontalmente utilizando múltiplas instâncias atrás de um load balancer.

Recomendações:

- Manter Redis e PostgreSQL como serviços externos.
- Adicionar health checks na API.
- Escalar workers de ingestão separadamente da API de consulta.
- Monitorar latência, throughput, uso de CPU/memória e tempo das queries.

### 4. Autenticação e autorização com OAuth

> Como implementaria autenticação e autorização nas APIs usando OAuth?

Para um sistema B2B como o Hub Meteorológico, a abordagem recomendada é utilizar OAuth 2.0 com OpenID Connect por meio de um Authorization Server dedicado, como Keycloak, Auth0, Azure AD B2C ou outro provedor compatível.

Fluxos recomendados:

| Cliente | Fluxo OAuth | Justificativa |
|---------|-------------|---------------|
| Frontend SPA ou Mobile | Authorization Code + PKCE | Evita exposição de `client_secret` no cliente. |
| Worker externo ou integração M2M | Client Credentials | Adequado para comunicação entre sistemas. |
| Dashboard interno | Authorization Code + PKCE | Permite autenticação com usuário e SSO corporativo. |
| Webhook de terceiros | API Key ou Client Credentials | Simples para integrações controladas. |

Fluxo geral:

```text
[Cliente externo]
        ↓
[Authorization Server]
        ↓ access_token JWT
[HubMeteorologico API]
        ↓
[Recurso protegido]
```

Exemplo de configuração no ASP.NET Core 8:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.hubmeteorologico.com.br/realms/hub";
        options.Audience = "hubmeteorologico-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
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

Exemplo de aplicação no controller:

```csharp
[ApiController]
[Authorize(Policy = "LeituraRegistros")]
[Route("registros-interpolados")]
public class RegistrosInterpoladosController : ControllerBase
{
    // Apenas tokens com o escopo registros:read acessam este recurso.
}
```

Para um cenário multi-tenant, a autorização também deve validar se o usuário ou sistema tem acesso à fazenda solicitada.

Exemplo:

```csharp
var fazendasPermitidas = User.FindAll("fazenda_ids")
    .Select(c => int.Parse(c.Value))
    .ToList();

if (!fazendasPermitidas.Contains(filtro.FazendaId))
{
    throw new ForbiddenException("Acesso negado à fazenda solicitada.");
}
```

Escopos sugeridos:

| Escopo | Permissão |
|--------|-----------|
| `registros:read` | Consultar registros interpolados e meteorológicos. |
| `equipamentos:read` | Listar equipamentos e fontes de dados. |
| `equipamentos:write` | Criar e atualizar cadastro de equipamentos. |
| `admin` | Acesso administrativo completo. |

Recomendações de segurança:

- Utilizar tokens JWT assinados com RS256.
- Manter access tokens com tempo de vida curto.
- Utilizar refresh token com rotação quando houver usuário autenticado.
- Validar `issuer`, `audience`, assinatura e expiração do token.
- Usar claims de `scope`, `roles` e `fazenda_ids` para autorização.
- Registrar auditoria para acessos e alterações sensíveis.

---

## Estrutura de diretórios

```text
HubMeteorologico/
├── HubMeteorologico.API/
│   ├── Controllers/
│   │   └── RegistrosInterpoladosController.cs
│   ├── ConfigController/
│   │   └── BaseController.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── Validators/
│   │   └── RegistrosInterpoladosFilterDtoValidator.cs
│   ├── Program.cs
│   └── appsettings.json
├── HubMeteorologico.Domain/
│   ├── Entities/
│   │   └── Fazendas, Equipamentos, RegistrosInterpolados...
│   ├── DTOs/
│   │   └── RegistrosInterpoladosDto, FilterDto...
│   ├── Exceptions/
│   │   └── BadRequestException, NotFoundException...
│   └── ResponseDefault/
│       └── ReturnDefault<T>
├── HubMeteorologico.Application/
│   ├── Interfaces/
│   │   └── IRegistrosInterpoladosService.cs
│   └── Services/
│       └── RegistrosInterpoladosService.cs
├── HubMeteorologico.Infrastructure/
│   ├── Repository/
│   │   ├── Repository.cs
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
```
