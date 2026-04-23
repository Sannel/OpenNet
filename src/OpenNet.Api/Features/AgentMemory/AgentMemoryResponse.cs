// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Sannel.OpenNet.Api.Features.AgentMemory;

public record AgentMemoryResponse(
	Guid Id,
	Guid AgentId,
	string Key,
	string Value,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt
);
