// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Agents.AI;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Sannel.OpenNet.Core.Agents;
using Sannel.OpenNet.Core.AI;
using Sannel.OpenNet.Core.Data;
using CoreAgentSession = Sannel.OpenNet.Core.Agents.AgentSession;

namespace Sannel.OpenNet.Core.Tests.AI;

public class AgentSessionServiceTests
{
// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

private static async Task<(SqliteConnection Connection, ApplicationDbContext Db)> CreateDbAsync()
{
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseSqlite(connection)
.Options;

var db = new ApplicationDbContext(options);
await db.Database.EnsureCreatedAsync();

return (connection, db);
}

private static Agent CreateSeedAgent()
{
return new Agent
{
Id = Guid.NewGuid(),
Name = "Seed Agent",
Description = "Agent seeded for unit tests",
SystemPrompt = "You are a helpful test assistant.",
ModelId = "llama3.2",
Provider = AgentProviderEnum.Ollama,
CreatedAt = DateTimeOffset.Now,
UpdatedAt = DateTimeOffset.Now,
};
}

private static CoreAgentSession CreateSeedSession(Guid agentId)
{
return new CoreAgentSession
{
Id = Guid.NewGuid(),
AgentId = agentId,
StartedAt = DateTimeOffset.Now,
UpdatedAt = DateTimeOffset.Now,
};
}

private static IAgentClientFactory CreateRealFactory()
{
return new AgentClientFactory(
Options.Create(new OllamaOptions
{
Endpoint = "http://localhost:11434",
ModelId = "llama3.2",
}),
Options.Create(new AzureAIOptions()),
NullLoggerFactory.Instance);
}

private static Mock<IAgentClientFactory> CreateMockFactory(string replyText)
{
	var mockClient = new Mock<IChatClient>();
	mockClient
		.Setup(c => c.GetResponseAsync(
			It.IsAny<IEnumerable<ChatMessage>>(),
			It.IsAny<ChatOptions?>(),
			It.IsAny<CancellationToken>()))
		.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, replyText)));

	var mockFactory = new Mock<IAgentClientFactory>();
	mockFactory
		.Setup(f => f.CreateAgent(It.IsAny<Agent>()))
		.Returns<Agent>(a => new ChatClientAgent(
			mockClient.Object,
			new ChatClientAgentOptions
			{
				Name = a.Name,
				Description = a.Description,
				ChatOptions = new ChatOptions { Instructions = a.SystemPrompt },
				ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions()),
			},
			NullLoggerFactory.Instance,
			null));

	return mockFactory;
}

private void SeedAgent(ApplicationDbContext db, Agent agent)
{
db.Agents.Add(agent);
db.SaveChanges();
}

private void SeedAgentAndSession(ApplicationDbContext db, Agent agent, CoreAgentSession session)
{
db.Agents.Add(agent);
db.AgentSessions.Add(session);
db.SaveChanges();
}

// ---------------------------------------------------------------------------
// StartSessionAsync
// ---------------------------------------------------------------------------

[Fact]
public async Task StartSessionAsync_ValidAgentId_CreatesSessionInDatabase()
{
var (connection, db) = await CreateDbAsync();
using (connection)
await using (db)
{
var agent = CreateSeedAgent();
this.SeedAgent(db, agent);

var service = new AgentSessionService(db, CreateRealFactory(), NullLogger<AgentSessionService>.Instance);

var session = await service.StartSessionAsync(agent.Id);

Assert.NotNull(session);
Assert.Equal(agent.Id, session.AgentId);
Assert.NotEqual(Guid.Empty, session.Id);

var persisted = await db.AgentSessions.FindAsync(session.Id);
Assert.NotNull(persisted);
Assert.Equal(agent.Id, persisted.AgentId);
}
}

[Fact]
public async Task StartSessionAsync_ValidAgentId_SessionHasStartedAtAndUpdatedAtSet()
{
var before = DateTimeOffset.Now;
var (connection, db) = await CreateDbAsync();
using (connection)
await using (db)
{
var agent = CreateSeedAgent();
this.SeedAgent(db, agent);

var service = new AgentSessionService(db, CreateRealFactory(), NullLogger<AgentSessionService>.Instance);

var session = await service.StartSessionAsync(agent.Id);

var after = DateTimeOffset.Now;
Assert.InRange(session.StartedAt, before, after);
Assert.InRange(session.UpdatedAt, before, after);
}
}

[Fact]
public async Task StartSessionAsync_InvalidAgentId_ThrowsInvalidOperationException()
{
var (connection, db) = await CreateDbAsync();
using (connection)
await using (db)
{
var service = new AgentSessionService(db, CreateRealFactory(), NullLogger<AgentSessionService>.Instance);

await Assert.ThrowsAsync<InvalidOperationException>(
() => service.StartSessionAsync(Guid.NewGuid()));
}
}

// ---------------------------------------------------------------------------
// SendMessageAsync
// ---------------------------------------------------------------------------

[Fact]
public async Task SendMessageAsync_InvalidSessionId_ThrowsInvalidOperationException()
{
var (connection, db) = await CreateDbAsync();
using (connection)
await using (db)
{
var service = new AgentSessionService(db, CreateRealFactory(), NullLogger<AgentSessionService>.Instance);

await Assert.ThrowsAsync<InvalidOperationException>(
() => service.SendMessageAsync(Guid.NewGuid(), "Hello"));
}
}

[Fact]
public async Task SendMessageAsync_ValidSession_SavesUserAndAssistantMessagesToDatabase()
{
var (connection, db) = await CreateDbAsync();
using (connection)
await using (db)
{
var agent = CreateSeedAgent();
var session = CreateSeedSession(agent.Id);
this.SeedAgentAndSession(db, agent, session);

var mockFactory = CreateMockFactory("Test reply");
var service = new AgentSessionService(db, mockFactory.Object, NullLogger<AgentSessionService>.Instance);

await service.SendMessageAsync(session.Id, "Hello");

var messages = await db.AgentMessages
.Where(m => m.SessionId == session.Id)
.ToListAsync();

Assert.Equal(2, messages.Count);
Assert.Contains(messages, m => m.Role == MessageRole.User && m.Content == "Hello");
Assert.Contains(messages, m => m.Role == MessageRole.Assistant && m.Content == "Test reply");
}
}

[Fact]
public async Task SendMessageAsync_ValidSession_ReturnsAssistantMessage()
{
var (connection, db) = await CreateDbAsync();
using (connection)
await using (db)
{
var agent = CreateSeedAgent();
var session = CreateSeedSession(agent.Id);
this.SeedAgentAndSession(db, agent, session);

var mockFactory = CreateMockFactory("Assistant response");
var service = new AgentSessionService(db, mockFactory.Object, NullLogger<AgentSessionService>.Instance);

var result = await service.SendMessageAsync(session.Id, "Hello");

Assert.NotNull(result);
Assert.Equal(MessageRole.Assistant, result.Role);
Assert.Equal("Assistant response", result.Content);
		}
	}

	// ---------------------------------------------------------------------------
	// SendMessageAsync — LLM message propagation
	// ---------------------------------------------------------------------------

	[Fact]
	public async Task SendMessageAsync_WithPriorHistory_ReplaysPriorMessagesInCorrectOrder()
	{
		var (connection, db) = await CreateDbAsync();
		using (connection)
		await using (db)
		{
			// Arrange
			var agent = CreateSeedAgent();
			var session = CreateSeedSession(agent.Id);
			this.SeedAgentAndSession(db, agent, session);

			var priorUser = new AgentMessage
			{
				Id = Guid.NewGuid(),
				SessionId = session.Id,
				Role = MessageRole.User,
				Content = "Prior question",
				CreatedAt = DateTimeOffset.Now.AddMinutes(-2),
			};

			var priorAssistant = new AgentMessage
			{
				Id = Guid.NewGuid(),
				SessionId = session.Id,
				Role = MessageRole.Assistant,
				Content = "Prior answer",
				CreatedAt = DateTimeOffset.Now.AddMinutes(-1),
			};

			db.AgentMessages.AddRange(priorUser, priorAssistant);
			await db.SaveChangesAsync();

			IList<ChatMessage>? capturedMessages = null;
			ChatOptions? capturedOptions = null;

			var mockClient = new Mock<IChatClient>();
			mockClient
				.Setup(c => c.GetResponseAsync(
					It.IsAny<IEnumerable<ChatMessage>>(),
					It.IsAny<ChatOptions?>(),
					It.IsAny<CancellationToken>()))
				.Callback<IEnumerable<ChatMessage>, ChatOptions, CancellationToken>((msgs, opts, ct) =>
				{
					capturedMessages = msgs.ToList();
					capturedOptions = opts;
				})
				.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "New answer")));

			var mockFactory = new Mock<IAgentClientFactory>();
			mockFactory
				.Setup(f => f.CreateAgent(It.IsAny<Agent>()))
				.Returns<Agent>(a => new ChatClientAgent(
					mockClient.Object,
					new ChatClientAgentOptions
					{
						Name = a.Name,
						Description = a.Description,
						ChatOptions = new ChatOptions { Instructions = a.SystemPrompt },
						ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions()),
					},
					NullLoggerFactory.Instance,
					null));

			var service = new AgentSessionService(db, mockFactory.Object, NullLogger<AgentSessionService>.Instance);

			// Act
			await service.SendMessageAsync(session.Id, "New question");

			// Assert
			Assert.NotNull(capturedMessages);
			Assert.NotNull(capturedOptions);

			Assert.Equal(3, capturedMessages.Count);

			Assert.Equal(ChatRole.User, capturedMessages[0].Role);
			Assert.Equal("Prior question", capturedMessages[0].Text);

			Assert.Equal(ChatRole.Assistant, capturedMessages[1].Role);
			Assert.Equal("Prior answer", capturedMessages[1].Text);

			Assert.Equal(ChatRole.User, capturedMessages[2].Role);
			Assert.Equal("New question", capturedMessages[2].Text);

			Assert.DoesNotContain(capturedMessages, m => m.Role == ChatRole.System);

			Assert.Equal(agent.SystemPrompt, capturedOptions.Instructions);
		}
	}

	[Fact]
	public async Task SendMessageAsync_FirstMessage_SendsOnlyUserMessageToLlm()
	{
		var (connection, db) = await CreateDbAsync();
		using (connection)
		await using (db)
		{
			// Arrange
			var agent = CreateSeedAgent();
			var session = CreateSeedSession(agent.Id);
			this.SeedAgentAndSession(db, agent, session);

			IList<ChatMessage>? capturedMessages = null;
			ChatOptions? capturedOptions = null;

			var mockClient = new Mock<IChatClient>();
			mockClient
				.Setup(c => c.GetResponseAsync(
					It.IsAny<IEnumerable<ChatMessage>>(),
					It.IsAny<ChatOptions?>(),
					It.IsAny<CancellationToken>()))
				.Callback<IEnumerable<ChatMessage>, ChatOptions, CancellationToken>((msgs, opts, ct) =>
				{
					capturedMessages = msgs.ToList();
					capturedOptions = opts;
				})
				.ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hi there")));

			var mockFactory = new Mock<IAgentClientFactory>();
			mockFactory
				.Setup(f => f.CreateAgent(It.IsAny<Agent>()))
				.Returns<Agent>(a => new ChatClientAgent(
					mockClient.Object,
					new ChatClientAgentOptions
					{
						Name = a.Name,
						Description = a.Description,
						ChatOptions = new ChatOptions { Instructions = a.SystemPrompt },
						ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions()),
					},
					NullLoggerFactory.Instance,
					null));

			var service = new AgentSessionService(db, mockFactory.Object, NullLogger<AgentSessionService>.Instance);

			// Act
			await service.SendMessageAsync(session.Id, "Hello");

			// Assert
			Assert.NotNull(capturedMessages);
			Assert.NotNull(capturedOptions);

			Assert.Equal(1, capturedMessages.Count);

			Assert.Equal(ChatRole.User, capturedMessages[0].Role);
			Assert.Equal("Hello", capturedMessages[0].Text);

			Assert.DoesNotContain(capturedMessages, m => m.Role == ChatRole.System);

			Assert.Equal(agent.SystemPrompt, capturedOptions.Instructions);
		}
	}
}
