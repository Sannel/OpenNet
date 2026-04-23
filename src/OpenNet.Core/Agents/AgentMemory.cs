// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Sannel.OpenNet.Core.Agents;

public class AgentMemory
{
	public Guid Id { get; set; }

	public Guid AgentId { get; set; }

	public Agent Agent { get; set; } = null!;

	public required string Key { get; set; }

	public required string Value { get; set; }

	public DateTimeOffset CreatedAt { get; set; }

	public DateTimeOffset UpdatedAt { get; set; }
}
