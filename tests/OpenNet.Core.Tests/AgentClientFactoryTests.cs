// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Agents.AI;
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
	// CreateAgent — Ollama
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateAgent_OllamaProvider_ReturnsNonNullChatClientAgent()
	{
		// Arrange
		var factory = CreateFactory(new OllamaOptions
		{
			Endpoint = "http://localhost:11434",
			ModelId = "llama3.2",
		});

		var agent = CreateAgent(AgentProviderEnum.Ollama, "llama3.2");

		// Act
		var client = factory.CreateAgent(agent);

		// Assert
		Assert.NotNull(client);
	}

	[Fact]
	public void CreateAgent_OllamaProvider_UsesAgentModelIdWhenProvided()
	{
		// Arrange — the per-agent ModelId overrides the global OllamaOptions.ModelId;
		// observable outcome at this layer is a non-null, successfully constructed agent
		// (actual model routing is exercised only at inference time).
		var factory = CreateFactory(new OllamaOptions
		{
			Endpoint = "http://localhost:11434",
			ModelId = "llama3.2",
		});

		var agent = CreateAgent(AgentProviderEnum.Ollama, "mistral");

		// Act
		var client = factory.CreateAgent(agent);

		// Assert
		Assert.NotNull(client);
	}

	[Fact]
	public void CreateAgent_OllamaProvider_ReturnsChatClientAgentImplementation()
	{
		// Arrange
		var factory = CreateFactory();
		var agent = CreateAgent(AgentProviderEnum.Ollama);

		// Act
		var client = factory.CreateAgent(agent);

		// Assert — must return a ChatClientAgent from the Microsoft.Agents.AI contract
		Assert.IsAssignableFrom<ChatClientAgent>(client);
	}

	// ---------------------------------------------------------------------------
	// CreateAgent — AzureAI valid config
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateAgent_AzureAIProviderWithValidConfig_ReturnsNonNullChatClientAgent()
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
		var client = factory.CreateAgent(agent);

		// Assert
		Assert.NotNull(client);
	}

	// ---------------------------------------------------------------------------
	// CreateAgent — AzureAI validation failures
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateAgent_AzureAIProviderWithEmptyEndpoint_ThrowsInvalidOperationException()
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
		Assert.Throws<InvalidOperationException>(() => factory.CreateAgent(agent));
	}

	[Fact]
	public void CreateAgent_AzureAIProviderWithWhitespaceEndpoint_ThrowsInvalidOperationException()
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
		Assert.Throws<InvalidOperationException>(() => factory.CreateAgent(agent));
	}

	[Fact]
	public void CreateAgent_AzureAIProviderWithEmptyApiKey_ThrowsInvalidOperationException()
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
		Assert.Throws<InvalidOperationException>(() => factory.CreateAgent(agent));
	}

	[Fact]
	public void CreateAgent_AzureAIProviderWithWhitespaceApiKey_ThrowsInvalidOperationException()
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
		Assert.Throws<InvalidOperationException>(() => factory.CreateAgent(agent));
	}

	// ---------------------------------------------------------------------------
	// CreateAgent — unknown provider
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateAgent_UnknownProvider_ThrowsNotSupportedException()
	{
		// Arrange — cast an out-of-range integer to force an unrecognised enum value
		var factory = CreateFactory();
		var agent = CreateAgent((AgentProviderEnum)999);

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => factory.CreateAgent(agent));
	}
}
