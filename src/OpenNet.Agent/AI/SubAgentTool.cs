// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Sannel.OpenNet.Core.AI;

namespace Sannel.OpenNet.Agent.AI;

public class SubAgentTool
{
	private readonly AgentSessionService _sessionService;
	private readonly ILogger<SubAgentTool> _logger;

	public SubAgentTool(AgentSessionService sessionService, ILogger<SubAgentTool> logger)
	{
		this._sessionService = sessionService;
		this._logger = logger;
	}

	public AIFunction CreateFunction(Guid subAgentId)
	{
		return AIFunctionFactory.Create(
			async (string message, CancellationToken ct) =>
			{
				this._logger.LogInformation(
					"SubAgentTool invoking sub-agent {SubAgentId} with message length {Length}",
					subAgentId, message.Length);

				var session = await this._sessionService.StartSessionAsync(subAgentId, ct);
				var reply = await this._sessionService.SendMessageAsync(session.Id, message, ct);
				return reply.Content;
			},
			name: $"invoke_agent_{subAgentId:N}",
			description: "Invoke a sub-agent with a message and return its response.");
	}
}
