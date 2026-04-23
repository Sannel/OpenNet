// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Sannel.OpenNet.Core.Agents;
using Sannel.OpenNet.Core.Data;

namespace Sannel.OpenNet.Api.Features.Agents;

public static class AgentEndpoints
{
	public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
	{
		var versionedGroup = app.NewVersionedApi();
		var v1 = versionedGroup.MapGroup("/api/v{version:apiVersion}/agents").HasApiVersion(1.0);

		v1.MapGet("/", ListAgents);
		v1.MapGet("/{id:guid}", GetAgent);
		v1.MapPost("/", CreateAgent);
		v1.MapPut("/{id:guid}", UpdateAgent);
		v1.MapDelete("/{id:guid}", DeleteAgent);
	}

	private static async Task<Ok<List<AgentResponse>>> ListAgents(ApplicationDbContext db, CancellationToken ct)
	{
		var agents = await db.Agents
			.AsNoTracking()
			.OrderBy(a => a.Name)
			.Select(a => new AgentResponse(
				a.Id,
				a.Name,
				a.Description,
				a.SystemPrompt,
				a.Provider,
				a.ModelId,
				a.ParentAgentId,
				a.CreatedAt,
				a.UpdatedAt))
			.ToListAsync(ct);

		return TypedResults.Ok(agents);
	}

	private static async Task<Results<Ok<AgentResponse>, NotFound>> GetAgent(
		Guid id,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		var agent = await db.Agents
			.AsNoTracking()
			.Where(a => a.Id == id)
			.Select(a => new AgentResponse(
				a.Id,
				a.Name,
				a.Description,
				a.SystemPrompt,
				a.Provider,
				a.ModelId,
				a.ParentAgentId,
				a.CreatedAt,
				a.UpdatedAt))
			.FirstOrDefaultAsync(ct);

		if (agent is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(agent);
	}

	private static async Task<Results<Created<AgentResponse>, ValidationProblem>> CreateAgent(
		CreateAgentRequest request,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["Name"] = ["Name is required."]
			});
		}

		if (string.IsNullOrWhiteSpace(request.SystemPrompt))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["SystemPrompt"] = ["SystemPrompt is required."]
			});
		}

		if (string.IsNullOrWhiteSpace(request.ModelId))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["ModelId"] = ["ModelId is required."]
			});
		}

		var now = DateTimeOffset.Now;
		var agent = new Agent
		{
			Id = Guid.NewGuid(),
			Name = request.Name,
			Description = request.Description,
			SystemPrompt = request.SystemPrompt,
			Provider = request.Provider,
			ModelId = request.ModelId,
			ParentAgentId = request.ParentAgentId,
			CreatedAt = now,
			UpdatedAt = now,
		};

		db.Agents.Add(agent);
		await db.SaveChangesAsync(ct);

		var response = new AgentResponse(
			agent.Id,
			agent.Name,
			agent.Description,
			agent.SystemPrompt,
			agent.Provider,
			agent.ModelId,
			agent.ParentAgentId,
			agent.CreatedAt,
			agent.UpdatedAt);

		return TypedResults.Created($"/api/v1/agents/{agent.Id}", response);
	}

	private static async Task<Results<Ok<AgentResponse>, NotFound, ValidationProblem>> UpdateAgent(
		Guid id,
		UpdateAgentRequest request,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["Name"] = ["Name is required."]
			});
		}

		if (string.IsNullOrWhiteSpace(request.SystemPrompt))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["SystemPrompt"] = ["SystemPrompt is required."]
			});
		}

		if (string.IsNullOrWhiteSpace(request.ModelId))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["ModelId"] = ["ModelId is required."]
			});
		}

		var agent = await db.Agents.FindAsync([id], ct);

		if (agent is null)
		{
			return TypedResults.NotFound();
		}

		agent.Name = request.Name;
		agent.Description = request.Description;
		agent.SystemPrompt = request.SystemPrompt;
		agent.Provider = request.Provider;
		agent.ModelId = request.ModelId;
		agent.ParentAgentId = request.ParentAgentId;
		agent.UpdatedAt = DateTimeOffset.Now;

		await db.SaveChangesAsync(ct);

		return TypedResults.Ok(new AgentResponse(
			agent.Id,
			agent.Name,
			agent.Description,
			agent.SystemPrompt,
			agent.Provider,
			agent.ModelId,
			agent.ParentAgentId,
			agent.CreatedAt,
			agent.UpdatedAt));
	}

	private static async Task<Results<NoContent, NotFound>> DeleteAgent(
		Guid id,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		var agent = await db.Agents.FindAsync([id], ct);

		if (agent is null)
		{
			return TypedResults.NotFound();
		}

		db.Agents.Remove(agent);
		await db.SaveChangesAsync(ct);

		return TypedResults.NoContent();
	}
}
