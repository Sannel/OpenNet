// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Sannel.OpenNet.Core.Agents;
using Sannel.OpenNet.Core.AI;
using Sannel.OpenNet.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Sannel.OpenNet.Agent.AI;

public class AgentRunner
{
	private readonly IServiceProvider _services;
	private readonly ILogger<AgentRunner> _logger;

	public AgentRunner(IServiceProvider services, ILogger<AgentRunner> logger)
	{
		this._services = services;
		this._logger = logger;
	}

	public async Task RunAgentAsync(Guid agentId, string userMessage, CancellationToken ct = default)
	{
		await using var scope = this._services.CreateAsyncScope();
		var sessionService = scope.ServiceProvider.GetRequiredService<AgentSessionService>();

		var session = await sessionService.StartSessionAsync(agentId, ct);
		var reply = await sessionService.SendMessageAsync(session.Id, userMessage, ct);

		this._logger.LogInformation(
			"Agent {AgentId} completed run. Reply: {ReplyLength} chars",
			agentId, reply.Content.Length);
	}
}
