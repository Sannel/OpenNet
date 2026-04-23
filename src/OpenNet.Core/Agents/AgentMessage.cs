// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Sannel.OpenNet.Core.Agents;

public class AgentMessage
{
	public Guid Id { get; set; }

	public Guid SessionId { get; set; }

	public AgentSession Session { get; set; } = null!;

	public MessageRole Role { get; set; }

	public required string Content { get; set; }

	public DateTimeOffset CreatedAt { get; set; }
}
