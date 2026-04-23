// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Sannel.OpenNet.Core.Agents;

public class AgentSession
{
	public Guid Id { get; set; }

	public Guid AgentId { get; set; }

	public Agent Agent { get; set; } = null!;

	public ICollection<AgentMessage> Messages { get; set; } = [];

	public DateTimeOffset StartedAt { get; set; }

	public DateTimeOffset UpdatedAt { get; set; }
}
