// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Sannel.OpenNet.Core.Agents;
using Sannel.OpenNet.Core.Data;
using AgentsAI = Microsoft.Agents.AI;

namespace Sannel.OpenNet.Core.AI;

public class AgentSessionService
{
	private readonly ApplicationDbContext _db;
	private readonly IAgentClientFactory _factory;
	private readonly ILogger<AgentSessionService> _logger;

	public AgentSessionService(
		ApplicationDbContext db,
		IAgentClientFactory factory,
		ILogger<AgentSessionService> logger)
	{
		this._db = db;
		this._factory = factory;
		this._logger = logger;
	}

	public async Task<AgentSession> StartSessionAsync(Guid agentId, CancellationToken ct = default)
	{
		var agent = await this._db.Agents.FindAsync([agentId], ct)
			?? throw new InvalidOperationException($"Agent '{agentId}' not found.");

		var now = DateTimeOffset.Now;
		var session = new AgentSession
		{
			Id = Guid.NewGuid(),
			AgentId = agentId,
			StartedAt = now,
			UpdatedAt = now,
		};

		this._db.AgentSessions.Add(session);
		await this._db.SaveChangesAsync(ct);

		this._logger.LogInformation("Started session {SessionId} for agent {AgentId} ({AgentName})",
			session.Id, agentId, agent.Name);

		return session;
	}

	public async Task<AgentMessage> SendMessageAsync(
		Guid sessionId,
		string userMessage,
		CancellationToken ct = default)
	{
		var session = await this._db.AgentSessions
			.Include(s => s.Agent)
			.Include(s => s.Messages)
			.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
			?? throw new InvalidOperationException($"Session '{sessionId}' not found.");

		// Sort in memory for cross-provider compatibility (SQLite cannot ORDER BY DateTimeOffset).
		session.Messages = (ICollection<AgentMessage>)session.Messages.OrderBy(m => m.CreatedAt).ToList();

		var agent = session.Agent;

		// Snapshot prior history before adding the new user message — EF Core's relationship
		// fixup would otherwise include userMsg in session.Messages and double-send it.
		var priorHistory = BuildPriorHistory(session.Messages);

		var userMsg = new AgentMessage
		{
			Id = Guid.NewGuid(),
			SessionId = sessionId,
			Role = MessageRole.User,
			Content = userMessage,
			CreatedAt = DateTimeOffset.Now,
		};

		this._db.AgentMessages.Add(userMsg);

		var chatAgent = this._factory.CreateAgent(agent);
		var agentSession = await chatAgent.CreateSessionAsync(ct);

		if (chatAgent.ChatHistoryProvider is not AgentsAI.InMemoryChatHistoryProvider historyProvider)
		{
			throw new InvalidOperationException(
				$"Expected an {nameof(AgentsAI.InMemoryChatHistoryProvider)} but got {chatAgent.ChatHistoryProvider?.GetType().Name}. Cannot seed conversation history.");
		}

		if (priorHistory.Count > 0)
		{
			historyProvider.SetMessages(agentSession, priorHistory);
		}

		var response = await chatAgent.RunAsync(
			new ChatMessage(ChatRole.User, userMessage),
			agentSession,
			options: null,
			ct);

		var reply = response.Text ?? string.Empty;

		var assistantMsg = new AgentMessage
		{
			Id = Guid.NewGuid(),
			SessionId = sessionId,
			Role = MessageRole.Assistant,
			Content = reply,
			CreatedAt = DateTimeOffset.Now,
		};

		this._db.AgentMessages.Add(assistantMsg);

		session.UpdatedAt = DateTimeOffset.Now;

		await this._db.SaveChangesAsync(ct);

		this._logger.LogInformation("Session {SessionId} — agent {AgentName} replied ({Length} chars)",
			sessionId, agent.Name, reply.Length);

		return assistantMsg;
	}

	private static List<ChatMessage> BuildPriorHistory(IEnumerable<AgentMessage> history)
	{
		var messages = new List<ChatMessage>();

		foreach (var msg in history)
		{
			var role = msg.Role switch
			{
				MessageRole.User => ChatRole.User,
				MessageRole.Assistant => ChatRole.Assistant,
				MessageRole.Tool => ChatRole.Tool,
				_ => ChatRole.User,
			};

			messages.Add(new ChatMessage(role, msg.Content));
		}

		return messages;
	}
}
