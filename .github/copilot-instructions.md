# OpenNet — Copilot Instructions

## Project

OpenNet is a self-hosted AI agent and automation framework for .NET — a .NET-based alternative to OpenClaw. It connects messaging platforms (Keybase, Telegram) to AI agents backed by Azure AI or Ollama, and exposes a web UI for direct interaction.

Licensed under Apache 2.0.

## Product Plan

The full product plan (phases, technical decisions, non-goals) lives at `docs/docs/product-plan.md`. Always consult it when planning new features or assessing scope.

## Solution Structure

```
src/
  OpenNet.Api/        # ASP.NET Core API — main server (vertical slice architecture)
  OpenNet.Agent/      # Windows/Linux background service — runs AI agents
  OpenNet.Web/        # Blazor WASM frontend using MudBlazor components
  OpenNet.Core/       # Shared domain logic, models, interfaces
docs/
tests/
  OpenNet.Api.Tests/
  OpenNet.Agent.Tests/
  OpenNet.Web.Tests/
  OpenNet.Core.Tests/
```

## Build & Test Commands

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/OpenNet.Api.Tests/OpenNet.Api.Tests.csproj

# Run tests matching a name pattern
dotnet test --filter "FullyQualifiedName~MethodName"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Architecture

- **Vertical slice architecture** in `OpenNet.Api` and `OpenNet.Agent`: each feature owns its request/handler/response in a single folder. Do not use horizontal layers (Controllers/Services/Repositories).
- **Microsoft AI agent framework** for all AI provider communication (Azure AI / Ollama). Do not call AI provider SDKs directly.
- **Messaging integrations**: Keybase and Telegram are the first supported platforms.
- **Blazor WASM** (`OpenNet.Web`) uses **MudBlazor** for all UI components.

## Data / EF Core

- Target **SQL Server, PostgreSQL, and SQLite** — all three must be supported simultaneously via provider-specific registrations.
- Always use **`DateTimeOffset`** (never `DateTime`) for all date/time properties in entities and DTOs.
- Migrations must be compatible with all three providers.

## Coding Conventions

- **Root namespace**: `Sannel.OpenNet` — all namespaces start with `Sannel.OpenNet.*` (e.g. `Sannel.OpenNet.Api`, `Sannel.OpenNet.Core`, `Sannel.OpenNet.Agent`, `Sannel.OpenNet.Web`).
- **Target framework**: .NET 10
- **Nullable reference types** are enabled — all nullability must be explicit; no `#nullable disable`.
- **`async`/`await`** throughout — no `.Result` or `.Wait()` on async code.
- New source files must include the Apache 2.0 license header.
- **One public `class`/`struct`/`record`/`enum` per file.** Internal/private nested types are fine within the same file.

### Formatting (from `.editorconfig`)

- **Tabs** for indentation (not spaces). Exception: YAML files use 2-space indentation.
- **CRLF** line endings; **UTF-8** encoding.
- **Allman brace style**: opening brace on its own line (`csharp_new_line_before_open_brace = all`).
- `else`, `catch`, and `finally` each on their own line.
- Always use braces for control flow — omitting them is a compiler error (`csharp_prefer_braces = true:error`).
- No single-line statements (`csharp_preserve_single_line_statements = false`).
- Indent `case` contents and `switch` labels.

### C# Style (from `.editorconfig`)

- Use `this.` qualification for fields, properties, methods, and events.
- Use predefined language keywords (`int`, `string`, `bool`, …) instead of BCL type names (`Int32`, `String`, `Boolean`, …).
- Prefer `var` for built-in types (warning), and use it when the type is apparent or elsewhere as a suggestion.
- Prefer expression-bodied members (methods, constructors, operators, properties, indexers, accessors) where readable.
- Prefer pattern matching over `is`-with-cast and `as`-with-null-check.
- Use object and collection initializers, null-coalescing (`??`), and null-conditional (`?.`) operators where applicable.
- Prefer `default` over `default(T)` for simple default expressions.
- Use explicit tuple element names (not `Item1`/`Item2`).

## Testing

- **xUnit** for test structure, **Moq** for mocking.
- Test projects mirror the `src/` structure under `tests/`.
- `.gitignore` is the standard Visual Studio template — regenerate from [github/gitignore](https://github.com/github/gitignore) if it needs updating, do not edit manually.

## Git Workflow

- Follow **Gitflow**. The `develop` branch is the main integration branch (not `main`).
- Feature branches: `feature/<name>` off `develop`, merged back to `develop`.
- Release branches: `release/<version>` off `develop`, merged to both `main` and `develop`.
- Hotfix branches: `hotfix/<name>` off `main`, merged to both `main` and `develop`.
