// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// Note on integration-test coverage for AgentEndpoints:
// The handler delegates (ListAgents, GetAgent, CreateAgent, UpdateAgent, DeleteAgent)
// are declared as private static methods and are registered as minimal-API route
// delegates at startup. Direct unit testing is therefore not possible without either:
//
//   A) Making the handlers internal and adding InternalsVisibleTo in the production
//      assembly, OR
//   B) Standing up the full ASP.NET Core pipeline via WebApplicationFactory<Program>
//      (complex because Program.cs wires up Blazor WASM hosting, cookie/OAuth auth,
//      multi-provider EF Core, API versioning, and more).
//
// These tests cover the public request/response record types that the handlers
// consume and produce, providing compilation-level safety and behavioural
// documentation without requiring a running host.

using Sannel.OpenNet.Api.Features.Agents;
using Sannel.OpenNet.Core.AI;

namespace Sannel.OpenNet.Api.Tests.Features.Agents;

public class AgentEndpointsTests
{
	// ---------------------------------------------------------------------------
	// CreateAgentRequest record
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateAgentRequest_ConstructedWithAllFields_HasExpectedValues()
	{
		// Arrange
		var name = "My Agent";
		var description = "A helpful agent";
		var systemPrompt = "You are a helpful assistant.";
		var provider = AgentProviderEnum.Ollama;
		var modelId = "llama3.2";
		Guid? parentId = null;

		// Act
		var request = new CreateAgentRequest(name, description, systemPrompt, provider, modelId, parentId);

		// Assert
		Assert.Equal(name, request.Name);
		Assert.Equal(description, request.Description);
		Assert.Equal(systemPrompt, request.SystemPrompt);
		Assert.Equal(provider, request.Provider);
		Assert.Equal(modelId, request.ModelId);
		Assert.Null(request.ParentAgentId);
	}

	[Fact]
	public void CreateAgentRequest_WithNullDescription_DescriptionIsNull()
	{
		// Arrange & Act
		var request = new CreateAgentRequest(
			"Agent",
			null,
			"System prompt",
			AgentProviderEnum.Ollama,
			"llama3.2",
			null);

		// Assert
		Assert.Null(request.Description);
	}

	[Fact]
	public void CreateAgentRequest_WithParentAgentId_ParentAgentIdIsSet()
	{
		// Arrange
		var parentId = Guid.NewGuid();

		// Act
		var request = new CreateAgentRequest(
			"Child Agent",
			null,
			"System prompt",
			AgentProviderEnum.Ollama,
			"llama3.2",
			parentId);

		// Assert
		Assert.Equal(parentId, request.ParentAgentId);
	}

	[Fact]
	public void CreateAgentRequest_WithAzureAIProvider_ProviderIsPreserved()
	{
		// Arrange & Act
		var request = new CreateAgentRequest(
			"Azure Agent",
			null,
			"System prompt",
			AgentProviderEnum.AzureAI,
			"gpt-4o",
			null);

		// Assert
		Assert.Equal(AgentProviderEnum.AzureAI, request.Provider);
	}

	// ---------------------------------------------------------------------------
	// UpdateAgentRequest record
	// ---------------------------------------------------------------------------

	[Fact]
	public void UpdateAgentRequest_ConstructedWithAllFields_HasExpectedValues()
	{
		// Arrange
		var name = "Updated Agent";
		var description = "Updated description";
		var systemPrompt = "Updated system prompt.";
		var provider = AgentProviderEnum.AzureAI;
		var modelId = "gpt-4";
		var parentId = Guid.NewGuid();

		// Act
		var request = new UpdateAgentRequest(name, description, systemPrompt, provider, modelId, parentId);

		// Assert
		Assert.Equal(name, request.Name);
		Assert.Equal(description, request.Description);
		Assert.Equal(systemPrompt, request.SystemPrompt);
		Assert.Equal(provider, request.Provider);
		Assert.Equal(modelId, request.ModelId);
		Assert.Equal(parentId, request.ParentAgentId);
	}

	[Fact]
	public void UpdateAgentRequest_WithNullDescription_DescriptionIsNull()
	{
		// Arrange & Act
		var request = new UpdateAgentRequest(
			"Agent",
			null,
			"System prompt",
			AgentProviderEnum.Ollama,
			"llama3.2",
			null);

		// Assert
		Assert.Null(request.Description);
	}

	[Fact]
	public void UpdateAgentRequest_WithNullParentAgentId_ParentAgentIdIsNull()
	{
		// Arrange & Act
		var request = new UpdateAgentRequest(
			"Agent",
			"Desc",
			"System prompt",
			AgentProviderEnum.Ollama,
			"llama3.2",
			null);

		// Assert
		Assert.Null(request.ParentAgentId);
	}

	// ---------------------------------------------------------------------------
	// AgentResponse record
	// ---------------------------------------------------------------------------

	[Fact]
	public void AgentResponse_ConstructedWithAllFields_HasExpectedValues()
	{
		// Arrange
		var id = Guid.NewGuid();
		var name = "Agent One";
		var description = "First agent";
		var systemPrompt = "Be helpful.";
		var provider = AgentProviderEnum.Ollama;
		var modelId = "llama3.2";
		var parentAgentId = (Guid?)null;
		var createdAt = DateTimeOffset.Now;
		var updatedAt = DateTimeOffset.Now;

		// Act
		var response = new AgentResponse(
			id, name, description, systemPrompt,
			provider, modelId, parentAgentId,
			createdAt, updatedAt);

		// Assert
		Assert.Equal(id, response.Id);
		Assert.Equal(name, response.Name);
		Assert.Equal(description, response.Description);
		Assert.Equal(systemPrompt, response.SystemPrompt);
		Assert.Equal(provider, response.Provider);
		Assert.Equal(modelId, response.ModelId);
		Assert.Null(response.ParentAgentId);
		Assert.Equal(createdAt, response.CreatedAt);
		Assert.Equal(updatedAt, response.UpdatedAt);
	}

	[Fact]
	public void AgentResponse_WithParentAgentId_ParentAgentIdIsSet()
	{
		// Arrange
		var parentId = Guid.NewGuid();
		var now = DateTimeOffset.Now;

		// Act
		var response = new AgentResponse(
			Guid.NewGuid(),
			"Child Agent",
			null,
			"System prompt",
			AgentProviderEnum.Ollama,
			"llama3.2",
			parentId,
			now,
			now);

		// Assert
		Assert.Equal(parentId, response.ParentAgentId);
	}

	[Fact]
	public void AgentResponse_WithNullDescription_DescriptionIsNull()
	{
		// Arrange
		var now = DateTimeOffset.Now;

		// Act
		var response = new AgentResponse(
			Guid.NewGuid(),
			"Agent",
			null,
			"System prompt",
			AgentProviderEnum.AzureAI,
			"gpt-4",
			null,
			now,
			now);

		// Assert
		Assert.Null(response.Description);
	}

	[Fact]
	public void AgentResponse_CreatedAtAndUpdatedAtPreservedAsDateTimeOffset()
	{
		// Arrange — explicit offsets to verify DateTimeOffset fidelity (not DateTime)
		var createdAt = new DateTimeOffset(2025, 1, 15, 9, 0, 0, TimeSpan.FromHours(-5));
		var updatedAt = new DateTimeOffset(2025, 6, 1, 12, 30, 0, TimeSpan.FromHours(1));
		var now = DateTimeOffset.Now;

		// Act
		var response = new AgentResponse(
			Guid.NewGuid(),
			"Agent",
			null,
			"Prompt",
			AgentProviderEnum.Ollama,
			"llama3.2",
			null,
			createdAt,
			updatedAt);

		// Assert
		Assert.Equal(createdAt, response.CreatedAt);
		Assert.Equal(updatedAt, response.UpdatedAt);
	}

	// ---------------------------------------------------------------------------
	// Record equality (structural)
	// ---------------------------------------------------------------------------

	[Fact]
	public void CreateAgentRequest_TwoIdenticalInstances_AreEqual()
	{
		// Arrange
		var a = new CreateAgentRequest("Name", "Desc", "Prompt", AgentProviderEnum.Ollama, "model", null);
		var b = new CreateAgentRequest("Name", "Desc", "Prompt", AgentProviderEnum.Ollama, "model", null);

		// Assert — C# records use value equality
		Assert.Equal(a, b);
	}

	[Fact]
	public void UpdateAgentRequest_TwoIdenticalInstances_AreEqual()
	{
		// Arrange
		var a = new UpdateAgentRequest("Name", null, "Prompt", AgentProviderEnum.AzureAI, "gpt-4", null);
		var b = new UpdateAgentRequest("Name", null, "Prompt", AgentProviderEnum.AzureAI, "gpt-4", null);

		// Assert
		Assert.Equal(a, b);
	}
}
