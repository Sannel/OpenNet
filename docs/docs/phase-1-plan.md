# Phase 1 — Core Agent Infrastructure

> **Branch:** `feature/phase-1`
> **Goal:** Agents can be defined, configured, and run using the Microsoft Agent Framework.

---

## Approach

- All entities live in `OpenNet.Core` (EF Core, three-provider migrations).
- The runtime (background worker that runs agents) lives in `OpenNet.Agent`.
- REST endpoints for managing agents and memory live in `OpenNet.Api` (vertical slice per feature).
- AI provider connectivity uses **Microsoft Agent Framework** (`Microsoft.Agents.AI`) for the agent abstraction, with **OllamaSharp** (implements `IChatClient`) for Ollama and **`Microsoft.Extensions.AI.OpenAI`** for Azure OpenAI — both wired as `IChatClient` instances that the framework's `ChatClientAgent` consumes.
- Provider selection is purely config-driven (`AI:Provider`, `AI:Ollama:*`, `AI:AzureAI:*`); no custom adapter classes are written.

---

## Checklist

### Docs fix
- [x] Exclude `OpenNet.Web` from `docfx.json` metadata glob (Blazor WASM is not supported by DocFX).

### Entities & Migrations

- [x] **`Agent` entity** (`OpenNet.Core/Agents/Agent.cs`) — Id (Guid), Name, Description, SystemPrompt, Provider (enum), ModelId, ParentAgentId (nullable self-FK), CreatedAt, UpdatedAt.
- [x] **`AgentSession` entity** (`OpenNet.Core/Agents/AgentSession.cs`) — Id (Guid), AgentId (FK), StartedAt, UpdatedAt.
- [x] **`AgentMessage` entity** (`OpenNet.Core/Agents/AgentMessage.cs`) — Id (Guid), SessionId (FK), Role (enum: System/User/Assistant/Tool), Content, CreatedAt.
- [x] **`AgentMemory` entity** (`OpenNet.Core/Agents/AgentMemory.cs`) — Id (Guid), AgentId (FK), Key, Value, CreatedAt, UpdatedAt.
- [x] **Register all entities** in `ApplicationDbContext` and configure via `OnModelCreating`.
- [x] **EF migrations** — one migration per provider (SQLite, SqlServer, PostgreSQL) in separate assembly projects (`OpenNet.Migrations.Sqlite`, `OpenNet.Migrations.SqlServer`, `OpenNet.Migrations.Postgres`).

### Packages

- [x] Add `Microsoft.Agents.AI` to `OpenNet.Core` (core agent abstraction).
- [x] Add `Microsoft.Agents.AI.Workflows` to `OpenNet.Core` (multi-agent orchestration).
- [x] Add `OllamaSharp` to `OpenNet.Core` (Ollama `IChatClient` implementation).
- [x] Add `Microsoft.Extensions.AI.OpenAI` to `OpenNet.Core` (Azure OpenAI `IChatClient` implementation).

### Provider Configuration

- [x] **Options classes** in `OpenNet.Core/AI/` — `OllamaOptions` (Endpoint, ModelId) and `AzureAIOptions` (Endpoint, ModelId, ApiKey).
- [x] **`AgentProviderEnum`** — `Ollama`, `AzureAI`.
- [x] **`AgentClientFactory`** service (`OpenNet.Core/AI/AgentClientFactory.cs`) — resolves the correct `IChatClient` from config, exposes `CreateAgent(Agent entity) → ChatClientAgent`.
- [x] Register options and factory in `OpenNet.Api/Program.cs` and `OpenNet.Agent/Program.cs`.
- [x] Update `appsettings.json` (both projects) with commented-out Ollama and Azure AI config sections.

### Agent REST API (`OpenNet.Api`)

- [x] **`GET /api/v1/agents`** — list all agents.
- [x] **`GET /api/v1/agents/{id}`** — get agent by id.
- [x] **`POST /api/v1/agents`** — create agent.
- [x] **`PUT /api/v1/agents/{id}`** — update agent.
- [x] **`DELETE /api/v1/agents/{id}`** — delete agent.
- [x] Endpoints registered in `OpenNet.Api/Features/Agents/AgentEndpoints.cs`.

### Orchestrator & Conversation History (`OpenNet.Agent`)

- [x] **`AgentRunner`** service (`OpenNet.Agent/AI/AgentRunner.cs`) — creates a session, sends a message, logs the reply.
- [x] **`AgentSessionService`** (`OpenNet.Core/AI/AgentSessionService.cs`) — creates/retrieves `AgentSession` rows; loads history from DB and feeds it to the agent on each turn.

### Sub-agent Memory REST API (`OpenNet.Api`)

- [x] **`GET /api/v1/agents/{agentId}/memory`** — list memory entries for an agent.
- [x] **`GET /api/v1/agents/{agentId}/memory/{key}`** — get memory entry by key.
- [x] **`PUT /api/v1/agents/{agentId}/memory/{key}`** — upsert memory entry.
- [x] **`DELETE /api/v1/agents/{agentId}/memory/{key}`** — delete memory entry.
- [x] Endpoints registered in `OpenNet.Api/Features/AgentMemory/AgentMemoryEndpoints.cs`.

### Agent Delegation (`OpenNet.Agent`)

- [x] **`SubAgentTool`** (`OpenNet.Agent/AI/SubAgentTool.cs`) — an `AIFunction` that the orchestrator agent can call; it looks up the child `Agent` entity, creates a session via `AgentSessionService`, runs it, and returns the result.
- [x] Register `SubAgentTool` in `OpenNet.Agent/Program.cs`.

### Tests

- [ ] `AgentClientFactory` unit tests (mock config, verify correct `IChatClient` created).
- [ ] `AgentSessionService` unit tests (in-memory SQLite DB, mocked factory).
- [ ] Agent CRUD endpoint integration tests (`OpenNet.Api.Tests`).
