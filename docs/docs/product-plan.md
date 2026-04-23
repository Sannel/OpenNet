# Product Plan

## Vision

OpenNet is a self-hosted AI agent and automation platform for .NET.
It is OpenClaw-inspired but takes its own direction — optimized for individual operators
running it on personal servers. Agents connect to messaging platforms and AI backends,
can call tools, run on schedules, and delegate to sub-agents.

## Target Operator

Hobbyists and individuals self-hosting on personal hardware or VMs.
Simple to deploy, low operational overhead, no cloud dependency required (via the Ollama path).

---

## Phase 0 — Foundation

> **Goal:** Every project compiles, infrastructure wiring is in place, and CI passes.

- Create solution + all projects: `OpenNet.Api`, `OpenNet.Agent`, `OpenNet.Web`, `OpenNet.Core`
- EF Core setup supporting SQL Server, PostgreSQL, and SQLite via provider-specific registrations; all migrations must be compatible with all three providers
- OAuth/OIDC authentication (GitHub, Google, Entra ID) via ASP.NET Core OIDC middleware
- Blazor WASM hosted inside `OpenNet.Api` (not a standalone app)
- MudBlazor wired up in the Blazor project
- Basic health-check and versioned API endpoints
- Docker Compose file covering the API (which also serves the web UI) and database options
- Windows Service install script (`install-service.ps1`)
- systemd unit file and install script (`install-service.sh`)
- GitHub Actions CI: build and test on push/PR

---

## Phase 1 — Core Agent Infrastructure

> **Goal:** Agents can be defined, configured, and run.

- `Agent` entity: name, description, system prompt, model config, provider, parent agent reference
- Multi-agent framework integration via the **Microsoft AI agent framework** — no direct SDK calls to Azure AI or Ollama
- Ollama provider adapter
- Azure AI provider adapter
- Orchestrator agent: full conversation history persisted to the database per session
- Sub-agent memory API: agents can write and read structured memories via a server REST endpoint
- Agent delegation: orchestrator can spawn and invoke sub-agents and receive results

---

## Phase 2 — Plugin System

> **Goal:** Operators can write custom .NET plugins; agents on remote machines can safely download and run them.

### Server Side (`OpenNet.Api`)

- `IOpenNetPlugin` interface in `OpenNet.Core` — the contract plugin authors implement
- Plugin host service: discover, load, and register plugins at API startup using `AssemblyLoadContext`
- Plugin catalog API: list available plugins with name, version, hash, and publisher metadata
- Plugin upload endpoint (admin only): accept signed plugin packages and reject on failed integrity checks
- Plugin signing verification on upload: reject packages that fail signature or hash checks
- Trusted publisher registry: maintain a list of trusted signing keys/certs; only plugins from trusted publishers are accepted

### Agent Side (`OpenNet.Agent`)

- Plugin download client: fetch plugin packages from the server's catalog API
- Local integrity check before loading: verify hash and signature after download
- Plugin isolation: load plugins in a separate `AssemblyLoadContext` to prevent interference with the host

---

## Phase 3 — Keybase Integration

> **Goal:** Agents can receive messages from Keybase and reply in context.

- Keybase bot listener (poll or webhook)
- Message routing: map Keybase conversation/channel to a configured agent
- Response pipeline: agent reply → Keybase message send
- Admin UI page: configure Keybase bot token and channel-to-agent mappings
- Integration tests using a mock Keybase API client

---

## Phase 4 — Scheduled Automations

> **Goal:** Agents can be triggered on a cron or interval schedule without a human initiating the conversation.

- `Schedule` entity: agent ID, cron expression, enabled flag, last-run and next-run timestamps
- Scheduler background service in `OpenNet.Agent` (Quartz.NET or `IHostedService` with timer)
- Trigger pipeline: scheduler → agent invocation → result stored and/or forwarded to a messaging platform
- Admin UI page: create, edit, enable/disable schedules, and view run history
- Schedule execution logs with status and output preview

---

## Phase 5 — Web UI

> **Goal:** Operators can manage everything and chat with agents from the browser.

- Chat interface: select an agent, send messages, stream responses (SignalR or SSE)
- Agent management page: create, edit, and delete agents; configure provider, model, system prompt, and tools
- Plugin management page: view installed plugins, upload new packages, revoke/disable existing ones
- Schedule management page (ties in with Phase 4)
- Keybase integration settings page (ties in with Phase 3)
- User and auth settings page: manage OIDC provider config
- Dashboard: system status, recent agent activity, and error summary

---

## Non-Goals (v1)

- Telegram, Discord, Slack integrations — deferred to v2+
- MCP (Model Context Protocol) server support
- Multi-tenant / per-user isolation — single operator assumed
- Public plugin marketplace / registry — operators host their own

---

## Technical Decisions

| Concern | Decision |
|---|---|
| Framework | .NET 10 |
| AI framework | Microsoft AI agent framework (no direct provider SDK calls) |
| UI | Blazor WASM + MudBlazor, hosted inside `OpenNet.Api` |
| Database | EF Core; SQL Server, PostgreSQL, and SQLite all first-class |
| Authentication | OAuth/OIDC (GitHub, Google, Entra ID) |
| Messaging (v1) | Keybase only |
| AI backends | Ollama + Azure AI |
| Architecture | Vertical slice in `OpenNet.Api` and `OpenNet.Agent`; no horizontal layers |
| Deployment | Docker Compose + Windows Service install script + systemd install script |
| Plugin loading | `AssemblyLoadContext` isolation; signed and integrity-verified before load |
