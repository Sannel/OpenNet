// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace Sannel.OpenNet.Core.AI;

public class AgentClientFactory : IAgentClientFactory
{
	private readonly IOptions<OllamaOptions> _ollamaOptions;
	private readonly IOptions<AzureAIOptions> _azureAIOptions;
	private readonly ILoggerFactory _loggerFactory;

	public AgentClientFactory(
		IOptions<OllamaOptions> ollamaOptions,
		IOptions<AzureAIOptions> azureAIOptions,
		ILoggerFactory loggerFactory)
	{
		this._ollamaOptions = ollamaOptions;
		this._azureAIOptions = azureAIOptions;
		this._loggerFactory = loggerFactory;
	}

	public ChatClientAgent CreateAgent(Agents.Agent agent)
	{
		var chatClient = this.CreateChatClient(agent);
		var options = new ChatClientAgentOptions
		{
			Name = agent.Name,
			Description = agent.Description,
			ChatOptions = new Microsoft.Extensions.AI.ChatOptions { Instructions = agent.SystemPrompt },
			ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions()),
		};
		return chatClient.AsAIAgent(options, this._loggerFactory, null);
	}

	private IChatClient CreateChatClient(Agents.Agent agent)
	{
		return agent.Provider switch
		{
			AgentProviderEnum.Ollama => this.CreateOllamaChatClient(agent.ModelId),
			AgentProviderEnum.AzureAI => this.CreateAzureAIChatClient(agent.ModelId),
			_ => throw new NotSupportedException($"Provider '{agent.Provider}' is not supported."),
		};
	}

	private IChatClient CreateOllamaChatClient(string modelId)
	{
		var options = this._ollamaOptions.Value;
		var effectiveModel = string.IsNullOrWhiteSpace(modelId) ? options.ModelId : modelId;
		return new OllamaApiClient(new Uri(options.Endpoint), effectiveModel);
	}

	private IChatClient CreateAzureAIChatClient(string modelId)
	{
		var options = this._azureAIOptions.Value;

		if (string.IsNullOrWhiteSpace(options.Endpoint))
		{
			throw new InvalidOperationException("AI:AzureAI:Endpoint is required when using the AzureAI provider.");
		}

		if (string.IsNullOrWhiteSpace(options.ApiKey))
		{
			throw new InvalidOperationException("AI:AzureAI:ApiKey is required when using the AzureAI provider.");
		}

		var effectiveModel = string.IsNullOrWhiteSpace(modelId) ? options.ModelId : modelId;
		var clientOptions = new OpenAIClientOptions
		{
			Endpoint = new Uri(options.Endpoint),
		};

		var openAIClient = new OpenAIClient(new ApiKeyCredential(options.ApiKey), clientOptions);

		return openAIClient.GetChatClient(effectiveModel).AsIChatClient();
	}
}
