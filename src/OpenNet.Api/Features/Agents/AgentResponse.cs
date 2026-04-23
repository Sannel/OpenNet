// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Sannel.OpenNet.Core.AI;

namespace Sannel.OpenNet.Api.Features.Agents;

public record AgentResponse(
	Guid Id,
	string Name,
	string? Description,
	string SystemPrompt,
	AgentProviderEnum Provider,
	string ModelId,
	Guid? ParentAgentId,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt
);
