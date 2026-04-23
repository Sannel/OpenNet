// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Sannel.OpenNet.Core.AI;

namespace Sannel.OpenNet.Core.Agents;

public class Agent
{
	public Guid Id { get; set; }

	public required string Name { get; set; }

	public string? Description { get; set; }

	public required string SystemPrompt { get; set; }

	public AgentProviderEnum Provider { get; set; }

	public required string ModelId { get; set; }

	public Guid? ParentAgentId { get; set; }

	public Agent? ParentAgent { get; set; }

	public ICollection<Agent> SubAgents { get; set; } = [];

	public ICollection<AgentSession> Sessions { get; set; } = [];

	public ICollection<AgentMemory> Memories { get; set; } = [];

	public DateTimeOffset CreatedAt { get; set; }

	public DateTimeOffset UpdatedAt { get; set; }
}
