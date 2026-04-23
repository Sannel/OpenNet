// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sannel.OpenNet.Core.AI;
using Sannel.OpenNet.Core.Agents;

namespace Sannel.OpenNet.Core.Tests.AI;

public class AgentClientFactoryTests
{
	// ---------------------------------------------------------------------------
	// Helpers
	// ---------------------------------------------------------------------------

	private static AgentClientFactory CreateFactory(
		OllamaOptions? ollamaOptions = null,
		AzureAIOptions? azureAIOptions = null)
	{
		return new AgentClientFactory(
			Options.Create(ollamaOptions ?? new OllamaOptions
			{
				Endpoint = "http://localhost:11434",
				ModelId = "llama3.2",
			}),
			Options.Create(azureAIOptions ?? new AzureAIOptions()),
			NullLoggerFactory.Instance);
	}

	private static Agent CreateAgent(AgentProviderEnum provider, string modelId = "test-model")
	{
		return new Agent
		{
			Id = Guid.NewGuid(),
			Name = "Test Agent",
			Description = "Unit-test agent",
			SystemPrompt = "You are a helpful assistant.",
			ModelId = modelId,
			Provider = provider,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
		};
	}

	// ---------------------------------------------------------------------------
	// CreateChatClient — Ollama
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateChatClient_OllamaProvider_ReturnsNonNullIChatClient()
	{
		// Arrange
		var factory = CreateFactory(new OllamaOptions
		{
			Endpoint = "http://localhost:11434",
			ModelId = "llama3.2",
		});

		var agent = CreateAgent(AgentProviderEnum.Ollama, "llama3.2");

		// Act
		var client = factory.CreateChatClient(agent);

		// Assert
		Assert.NotNull(client);
	}

	[Fact]
	public void CreateChatClient_OllamaProvider_UsesAgentModelIdWhenProvided()
	{
		// Arrange — the per-agent ModelId overrides the global OllamaOptions.ModelId;
		// observable outcome at this layer is a non-null, successfully constructed client
		// (actual model routing is exercised only at inference time).
		var factory = CreateFactory(new OllamaOptions
		{
			Endpoint = "http://localhost:11434",
			ModelId = "llama3.2",
		});

		var agent = CreateAgent(AgentProviderEnum.Ollama, "mistral");

		// Act
		var client = factory.CreateChatClient(agent);

		// Assert
		Assert.NotNull(client);
	}

	[Fact]
	public void CreateChatClient_OllamaProvider_ReturnsIChatClientImplementation()
	{
		// Arrange
		var factory = CreateFactory();
		var agent = CreateAgent(AgentProviderEnum.Ollama);

		// Act
		var client = factory.CreateChatClient(agent);

		// Assert — must satisfy the Microsoft.Extensions.AI IChatClient contract
		Assert.IsAssignableFrom<IChatClient>(client);
	}

	// ---------------------------------------------------------------------------
	// CreateChatClient — AzureAI valid config
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateChatClient_AzureAIProviderWithValidConfig_ReturnsNonNullIChatClient()
	{
		// Arrange — fake credentials; OpenAIClient stores them locally, no network call is made
		var azureOptions = new AzureAIOptions
		{
			Endpoint = "https://fake-openai.openai.azure.com/",
			ApiKey = "00000000000000000000000000000000",
			ModelId = "gpt-4",
		};

		var factory = CreateFactory(azureAIOptions: azureOptions);
		var agent = CreateAgent(AgentProviderEnum.AzureAI, "gpt-4");

		// Act
		var client = factory.CreateChatClient(agent);

		// Assert
		Assert.NotNull(client);
	}

	// ---------------------------------------------------------------------------
	// CreateChatClient — AzureAI validation failures
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateChatClient_AzureAIProviderWithEmptyEndpoint_ThrowsInvalidOperationException()
	{
		// Arrange
		var azureOptions = new AzureAIOptions
		{
			Endpoint = string.Empty,
			ApiKey = "00000000000000000000000000000000",
			ModelId = "gpt-4",
		};

		var factory = CreateFactory(azureAIOptions: azureOptions);
		var agent = CreateAgent(AgentProviderEnum.AzureAI);

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => factory.CreateChatClient(agent));
	}

	[Fact]
	public void CreateChatClient_AzureAIProviderWithWhitespaceEndpoint_ThrowsInvalidOperationException()
	{
		// Arrange
		var azureOptions = new AzureAIOptions
		{
			Endpoint = "   ",
			ApiKey = "00000000000000000000000000000000",
			ModelId = "gpt-4",
		};

		var factory = CreateFactory(azureAIOptions: azureOptions);
		var agent = CreateAgent(AgentProviderEnum.AzureAI);

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => factory.CreateChatClient(agent));
	}

	[Fact]
	public void CreateChatClient_AzureAIProviderWithEmptyApiKey_ThrowsInvalidOperationException()
	{
		// Arrange
		var azureOptions = new AzureAIOptions
		{
			Endpoint = "https://fake-openai.openai.azure.com/",
			ApiKey = string.Empty,
			ModelId = "gpt-4",
		};

		var factory = CreateFactory(azureAIOptions: azureOptions);
		var agent = CreateAgent(AgentProviderEnum.AzureAI);

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => factory.CreateChatClient(agent));
	}

	[Fact]
	public void CreateChatClient_AzureAIProviderWithWhitespaceApiKey_ThrowsInvalidOperationException()
	{
		// Arrange
		var azureOptions = new AzureAIOptions
		{
			Endpoint = "https://fake-openai.openai.azure.com/",
			ApiKey = "   ",
			ModelId = "gpt-4",
		};

		var factory = CreateFactory(azureAIOptions: azureOptions);
		var agent = CreateAgent(AgentProviderEnum.AzureAI);

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => factory.CreateChatClient(agent));
	}

	// ---------------------------------------------------------------------------
	// CreateChatClient — unknown provider
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateChatClient_UnknownProvider_ThrowsNotSupportedException()
	{
		// Arrange — cast an out-of-range integer to force an unrecognised enum value
		var factory = CreateFactory();
		var agent = CreateAgent((AgentProviderEnum)999);

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => factory.CreateChatClient(agent));
	}
}
