# HubMeteorológico

API em **.NET 8** para consulta de registros meteorológicos interpolados em uma grade geoespacial por **Fazenda**, **Lavoura** e **Data/Hora cheia**. O projeto usa **PostgreSQL 16 com PostGIS**, acesso a dados com **Dapper/Npgsql**, cache distribuído com **Redis**, validação com **FluentValidation** e documentação interativa com **Swagger/OpenAPI**.

## 1. Visão do desafio e implementação atual

O HubMeteorológico recebe dados meteorológicos de sistemas externos, relaciona medições com equipamentos georreferenciados e expõe os registros interpolados para construção de mapas de calor no frontend.

### Endpoint funcional desenvolvido

```http
GET /registros-interpolados?FazendaId=1&CodigoLavoura=LAV01&DataHora=2024-01-15T12:00:00Z
```

Parâmetros:

| Parâmetro | Obrigatório | Descrição |
| --- | --- | --- |
| `FazendaId` | Sim | Identificador da fazenda. |
| `CodigoLavoura` | Não | Código da lavoura. Se omitido, retorna todas as lavouras da fazenda no horário informado. |
| `DataHora` | Sim | Hora cheia em formato ISO 8601. Ex.: `2024-01-15T12:00:00Z`. |

### Melhorias aplicadas no código

- **Cancelamento de requisições**: controller, service e repositórios aceitam `CancellationToken`, permitindo cancelar consultas caras quando o cliente desconecta.
- **Filtro de lavoura realmente opcional**: a validação não exige mais `CodigoLavoura`, alinhando o contrato da API com a consulta SQL.
- **Cache mais seguro e previsível**: o resultado é materializado antes de ser salvo em Redis; a chave normaliza o código da lavoura e usa `all` quando o filtro é omitido.
- **Remoção de dependência não utilizada**: o service deixou de depender de `IMapaFazendaLavouraRepository`, reduzindo acoplamento.

## 2. Como executar localmente

### Pré-requisitos

- Docker e Docker Compose.
- .NET SDK 8.0 para execução fora do container.
- Dump do banco do desafio, quando disponível.

### Subir PostgreSQL/PostGIS, Redis e API

```bash
docker compose up -d --build
```

A API ficará disponível no container configurado pelo `docker-compose`, e o Swagger pode ser acessado em:

```text
https://localhost:<porta>/swagger
```

> Ajuste portas conforme `docker-compose.override.yml` ou perfil de execução usado no ambiente.

### Restaurar dump do banco

Quando o arquivo de dump estiver disponível, use um fluxo semelhante:

```bash
docker cp ./dump.sql hubmeteorologico-postgres:/tmp/dump.sql
docker exec -it hubmeteorologico-postgres psql -U postgres -d HubMeteorologico -f /tmp/dump.sql
```

Para dumps customizados (`pg_dump -Fc`):

```bash
docker cp ./dump.backup hubmeteorologico-postgres:/tmp/dump.backup
docker exec -it hubmeteorologico-postgres pg_restore -U postgres -d HubMeteorologico --clean --if-exists /tmp/dump.backup
```

## 3. Tecnologias escolhidas e justificativa

| Camada | Tecnologia | Justificativa profissional |
| --- | --- | --- |
| API | ASP.NET Core 8 | Alta performance, suporte nativo a DI, middleware, OpenAPI e autenticação. |
| Banco | PostgreSQL 16 + PostGIS | Forte suporte geoespacial, índices GiST/SP-GiST/BRIN e particionamento. |
| Acesso a dados | Dapper + Npgsql + NetTopologySuite | Baixa sobrecarga em consultas de alto volume e boa integração com tipos geográficos. |
| Cache | Redis | Reduz pressão em leituras repetidas de mapas de calor por fazenda/lavoura/hora. |
| Validação | FluentValidation | Contratos claros e erros padronizados. |
| Observabilidade | Serilog | Logs estruturados para investigação de gargalos e falhas. |
| Documentação | Swagger/OpenAPI | Facilita testes, integração e contrato com frontend. |
| Testes | xUnit + Moq | Testes unitários de regras de aplicação sem depender do banco. |

## 4. Processo profissional de desenvolvimento

1. **Entendimento de domínio**
   - Mapear entidades: Fazenda, Lavoura, Equipamento, Estação, Pluviômetro, Registro Meteorológico, Registro Interpolado e Grade.
   - Confirmar regras: horas cheias, volume estimado de 25.000 registros interpolados/hora/fazenda e leitura concorrente.

2. **Desenho de contrato**
   - Definir endpoints, filtros, formatos de data e DTOs.
   - Documentar contratos via Swagger e README.

3. **Modelagem de dados e índices**
   - Revisar tabelas, relacionamentos e tipos PostGIS.
   - Criar índices compostos para filtros principais e índices geoespaciais para grade/coordenadas.

4. **Implementação incremental**
   - Criar controller fino, service com regras e repository com SQL parametrizado.
   - Implementar validação, cache, logs e tratamento global de exceções.

5. **Testes e validação de performance**
   - Unit tests para regras de filtro e cache.
   - Testes de integração contra PostgreSQL real com massa representativa.
   - Testes de carga simulando múltiplos usuários buscando a mesma hora/fazenda.

6. **Operação**
   - Métricas de latência, cache hit ratio, conexões do banco, throughput de ingestão e tamanho das partições.
   - Deploy com variáveis de ambiente e secrets fora do código.

## 5. Arquitetura 2 — Busca e persistência de dados externos

Exemplo solicitado:

```http
GET http://localhost:5000/registros-meteorologicos?Equipamento=X&DataHora=Y
```

### Objetivo

Construir um fluxo recorrente e resiliente para buscar dados de estações de terceiros, persistir medições brutas e disparar/permitir a interpolação por hora cheia.

### Componentes propostos

- **MeteorologicalIngestionWorker**: `BackgroundService` que agenda a busca recorrente de horas cheias.
- **IExternalMeteorologicalProvider**: abstração do fornecedor externo; a implementação inicial é simulada e pode ser trocada por client HTTP com autenticação, timeout, retry e circuit breaker.
- **MeteorologicalIngestionService**: orquestra normalização, validação, persistência idempotente e publicação de mensagens.
- **RegistrosMeteorologicosRepository**: grava leituras brutas das estações com `UPSERT`.
- **IInterpolationQueue / ChannelInterpolationQueue**: fila interna em memória para desacoplar ingestão e interpolação no ambiente atual.
- **InterpolationWorker**: consome mensagens da fila e chama o serviço de interpolação.

### Fluxo de ingestão

1. Scheduler identifica janelas pendentes de busca, sempre em horas cheias.
2. Worker chama APIs externas por equipamento/fazenda/janela.
3. Dados são normalizados para unidade padrão do domínio.
4. Sistema valida equipamento ativo, coordenada, data/hora e faixas aceitáveis das medições.
5. Persistência ocorre em transação curta com chave idempotente, por exemplo:
   - `EquipamentoId + DataHora + FonteMeteorologicaId`.
6. Após concluir ingestão de uma janela, o sistema publica um evento `MeteorologicalReadingsImported`.
7. Worker de interpolação consome `InterpolationRequestedMessage`, calcula a grade da fazenda/lavoura e persiste os slots interpolados.
8. Cache de consultas afetadas é invalidado ou expira rapidamente por TTL.

### Implementação entregue neste repositório

- `MeteorologicalIngestionWorker` roda de forma recorrente quando `IngestionWorker:Enabled=true`.
- `MeteorologicalIngestionService` busca equipamentos ativos, chama o provider externo, persiste medições brutas e enfileira uma mensagem por fazenda/hora.
- `RegistrosMeteorologicosRepository` usa `ON CONFLICT` sobre `DataHora + FazendaId + EquipamentoId` para idempotência.
- `ChannelInterpolationQueue` implementa uma fila interna para o desafio; em produção, essa porta pode ser substituída por RabbitMQ, Kafka ou SQS sem alterar a aplicação.
- `InterpolationWorker` consome a fila e delega para `InterpolationService`, que hoje registra o processamento e é o ponto de extensão para o cálculo real do mapa de calor.

Configuração no `appsettings.json`:

```json
"IngestionWorker": {
  "Enabled": false,
  "IntervalMinutes": 5,
  "LookbackHours": 1
}
```

### Endpoint de busca de medições brutas

```http
GET /registros-meteorologicos?Equipamento=EST-001&DataHora=2024-01-15T12:00:00Z
```

Boas práticas:

- Usar `Equipamento` como código externo ou identificador interno, mas documentar claramente.
- Exigir `DataHora` em hora cheia ou permitir intervalo (`DataHoraInicio`, `DataHoraFim`) quando houver necessidade analítica.
- Retornar DTO enxuto, sem expor entidades internas.
- Incluir paginação apenas para consultas exploratórias; para frontend do mapa, entregar todos os registros filtrados conforme requisito.

### Persistência idempotente

A ingestão deve suportar reprocessamento sem duplicidade:

```sql
CREATE UNIQUE INDEX ux_registros_meteorologicos_equipamento_hora_fonte
ON public."RegistrosMeteorologicos" ("EquipamentoId", "DataHora", "FonteMeteorologicaId");
```

Para upsert:

```sql
INSERT INTO public."RegistrosMeteorologicos" (...)
VALUES (...)
ON CONFLICT ("EquipamentoId", "DataHora", "FonteMeteorologicaId")
DO UPDATE SET
  "Temperatura" = EXCLUDED."Temperatura",
  "VolumeChuva" = EXCLUDED."VolumeChuva",
  "UpdatedAt" = now();
```

### Resiliência

- Timeout por fornecedor.
- Retry com backoff exponencial para falhas transitórias.
- Circuit breaker para fornecedor instável.
- Dead-letter queue para mensagens de ingestão/interpolação que falharem repetidamente.
- Correlação por `CorrelationId` e logs estruturados.

## 6. Arquitetura 3 — Atualização cadastral de equipamentos

### Regra do desafio

Existem **Estações Meteorológicas** e **Pluviômetros**. Apenas **Estações Meteorológicas** podem ter o nome alterado.

### Modelagem recomendada

Usaria uma entidade base `Equipamento` com especialização por tipo:

- `Equipamento`
  - `Id`
  - `CodigoExterno`
  - `TipoEquipamento`
  - `Latitude`
  - `Longitude`
  - `Ativo`
  - `FazendaId`
  - `CriadoEm`
  - `AtualizadoEm`
- `EstacaoMeteorologica : Equipamento`
  - `Nome`
  - Dados cadastrais específicos de estação.
- `Pluviometro : Equipamento`
  - Dados cadastrais específicos de pluviômetro.

Em banco relacional, eu escolheria uma destas abordagens:

1. **TPH — Table per Hierarchy** para simplicidade:
   - Uma tabela `Equipamentos` com coluna discriminadora `TipoEquipamento`.
   - Campo `Nome` permitido apenas para estações via regra de domínio e constraint.
2. **TPT — Table per Type** se houver muitos campos específicos:
   - Tabela `Equipamentos` para dados comuns.
   - Tabelas `EstacoesMeteorologicas` e `Pluviometros` para campos específicos.

Para este desafio, **TPH é suficiente**, pois a regra principal é comportamental: somente estação altera nome.

### Entidades e por quê

| Entidade | Motivo |
| --- | --- |
| `Equipamento` | Centraliza identidade, coordenadas e vínculo com fazenda. |
| `EstacaoMeteorologica` | Representa equipamento que mede variáveis meteorológicas e pode alterar nome. |
| `Pluviometro` | Representa equipamento focado em chuva, sem permissão de alteração de nome. |
| `Fazenda` | Agrega equipamentos por área operacional. |
| `HistoricoEquipamento` | Audita alterações cadastrais, especialmente coordenadas e nome. |

### Regra no domínio

A regra não deve ficar apenas no controller. Ela deve estar no domínio/aplicação:

```csharp
public abstract class Equipamento
{
    public int Id { get; protected set; }
    public string CodigoExterno { get; protected set; } = string.Empty;
    public decimal Latitude { get; protected set; }
    public decimal Longitude { get; protected set; }

    public virtual void AlterarNome(string novoNome)
        => throw new RegraDeDominioException("Este tipo de equipamento não permite alteração de nome.");
}

public sealed class EstacaoMeteorologica : Equipamento
{
    public string Nome { get; private set; } = string.Empty;

    public override void AlterarNome(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new RegraDeDominioException("Nome da estação é obrigatório.");

        Nome = novoNome.Trim();
    }
}

public sealed class Pluviometro : Equipamento
{
    // Não sobrescreve AlterarNome; portanto, a regra da base bloqueia a operação.
}
```

### API sugerida

```http
PATCH /equipamentos/{id}/nome
```

Body:

```json
{
  "nome": "Estação Norte"
}
```

Fluxo:

1. Buscar equipamento por `id`.
2. Reconstituir o tipo correto (`EstacaoMeteorologica` ou `Pluviometro`).
3. Chamar `equipamento.AlterarNome(nome)`.
4. Persistir alteração e registrar auditoria.
5. Retornar `204 No Content` em caso de sucesso.

Se o equipamento for pluviômetro, retornar `400 Bad Request` ou `422 Unprocessable Entity` com mensagem de regra de negócio.

## 7. Arquitetura 4 — Performance, escalabilidade e retenção

### Pontos críticos

#### 1. Alto volume de leitura

O frontend pode requisitar muitos slots para renderizar mapas de calor. Como todos os registros filtrados precisam ser entregues, paginação tradicional pode não atender ao requisito do mapa.

Estratégias:

- Índice composto em `RegistrosInterpolados(FazendaId, DataHora, MapaFazendaLavouraId)`.
- Cache Redis por `fazenda + lavoura + hora`.
- Resposta enxuta com apenas campos necessários para o mapa.
- Compressão HTTP (`gzip`/`brotli`).
- CDN/API Gateway cache para janelas históricas imutáveis.

#### 2. Alto volume de escrita recorrente

Com 25.000 registros interpolados por hora por fazenda, a escrita cresce rapidamente.

Estratégias:

- Ingestão em batch com `COPY` ou bulk insert quando aplicável.
- Upsert idempotente para evitar duplicidade.
- Transações curtas.
- Separar ingestão bruta e interpolação em workers assíncronos.
- Usar filas para desacoplar fornecedores externos do processamento interno.

#### 3. Particionamento

Particionar `RegistrosInterpolados` por tempo reduz custo de manutenção e facilita retenção.

Sugestão:

- Partição mensal ou semanal por `DataHora`, dependendo do volume.
- Subpartição por `FazendaId` apenas se houver fazendas muito grandes ou consultas sempre isoladas por fazenda.
- Índices locais nas partições.

#### 4. Índices geoespaciais

Mesmo que o endpoint principal filtre por fazenda/lavoura/hora, cálculos de grade e consultas espaciais se beneficiam de PostGIS.

Sugestões:

```sql
CREATE INDEX ix_registros_interpolados_fazenda_data_lavoura
ON public."RegistrosInterpolados" ("FazendaId", "DataHora", "MapaFazendaLavouraId");

CREATE INDEX ix_registros_interpolados_coordenada_gist
ON public."RegistrosInterpolados"
USING GIST ("Coordenada");

CREATE INDEX ix_registros_interpolados_data_brin
ON public."RegistrosInterpolados"
USING BRIN ("DataHora");
```

#### 5. Retenção de dados

Separar regras por tipo de dado:

| Tipo de dado | Retenção sugerida | Observação |
| --- | --- | --- |
| Medições brutas | Maior retenção | Importante para auditoria e reprocessamento. |
| Interpolados recentes | Retenção quente | Usados pelo frontend com alta frequência. |
| Interpolados históricos | Retenção fria | Podem ser compactados, arquivados ou movidos para storage analítico. |
| Logs técnicos | Retenção curta/média | Conforme necessidade de auditoria e observabilidade. |

Boas práticas:

- Mover partições antigas para storage mais barato.
- Arquivar em Parquet para análise histórica.
- Automatizar drop/detach de partições conforme política.
- Manter capacidade de reprocessar interpolados a partir das medições brutas.

#### 6. Escalabilidade horizontal

- API stateless escalando por réplicas.
- Redis compartilhado entre instâncias.
- Workers escaláveis por fila e chave de particionamento (`FazendaId`, `DataHora`).
- Read replicas do PostgreSQL para consultas históricas, se necessário.
- Connection pooling com Npgsql e limite controlado para não saturar o banco.

#### 7. Observabilidade

Métricas fundamentais:

- Latência p50/p95/p99 por endpoint.
- Taxa de cache hit/miss.
- Tempo de consulta SQL.
- Linhas retornadas por consulta.
- Tamanho e atraso das filas.
- Tempo de interpolação por fazenda/hora.
- Escritas por segundo e conflitos de upsert.

## 8. Arquitetura 5 — Autenticação e autorização com OAuth 2.0

Embora o foco deste README seja nos itens 2, 3 e 4 do desafio, a API deve estar pronta para OAuth 2.0/OpenID Connect.

Estratégia:

- Usar um provedor de identidade como Keycloak, Azure AD, Auth0 ou Duende IdentityServer.
- API como Resource Server validando JWT Bearer.
- Frontend usando Authorization Code Flow com PKCE.
- Máquinas/workers usando Client Credentials Flow.
- Autorização por scopes e policies.

Exemplos de scopes:

| Scope | Permissão |
| --- | --- |
| `meteorologia.read` | Consultar registros interpolados e medições. |
| `meteorologia.write` | Ingerir/persistir dados externos. |
| `equipamentos.write` | Atualizar cadastro de equipamentos. |
| `admin` | Operações administrativas. |

Policies na API:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MeteorologiaRead", policy =>
        policy.RequireClaim("scope", "meteorologia.read"));

    options.AddPolicy("EquipamentosWrite", policy =>
        policy.RequireClaim("scope", "equipamentos.write"));
});
```

Uso:

```csharp
[Authorize(Policy = "MeteorologiaRead")]
[HttpGet("registros-interpolados")]
public async Task<IActionResult> Get(...) { ... }
```

## 9. Próximos passos recomendados

- Criar migrations ou scripts versionados de schema e índices.
- Adicionar testes de integração com PostgreSQL/PostGIS via Testcontainers.
- Implementar worker de ingestão e fila de interpolação.
- Adicionar autenticação JWT Bearer e policies por scope.
- Configurar health checks para PostgreSQL e Redis.
- Adicionar compressão de resposta e métricas Prometheus/OpenTelemetry.
