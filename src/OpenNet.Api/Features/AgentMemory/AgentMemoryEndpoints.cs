// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Sannel.OpenNet.Core.Data;
using CoreAgentMemory = Sannel.OpenNet.Core.Agents.AgentMemory;

namespace Sannel.OpenNet.Api.Features.AgentMemory;

public static class AgentMemoryEndpoints
{
	public static void MapAgentMemoryEndpoints(this IEndpointRouteBuilder app)
	{
		var versionedGroup = app.NewVersionedApi();
		var v1 = versionedGroup.MapGroup("/api/v{version:apiVersion}/agents/{agentId:guid}/memory").HasApiVersion(1.0);

		v1.MapGet("/", ListMemory);
		v1.MapGet("/{key}", GetMemoryByKey);
		v1.MapPut("/{key}", SetMemory);
		v1.MapDelete("/{key}", DeleteMemory);
	}

	private static async Task<Results<Ok<List<AgentMemoryResponse>>, NotFound>> ListMemory(
		Guid agentId,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		var agentExists = await db.Agents.AnyAsync(a => a.Id == agentId, ct);
		if (!agentExists)
		{
			return TypedResults.NotFound();
		}

		var memories = await db.AgentMemories
			.AsNoTracking()
			.Where(m => m.AgentId == agentId)
			.OrderBy(m => m.Key)
			.Select(m => new AgentMemoryResponse(m.Id, m.AgentId, m.Key, m.Value, m.CreatedAt, m.UpdatedAt))
			.ToListAsync(ct);

		return TypedResults.Ok(memories);
	}

	private static async Task<Results<Ok<AgentMemoryResponse>, NotFound>> GetMemoryByKey(
		Guid agentId,
		string key,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		var memory = await db.AgentMemories
			.AsNoTracking()
			.Where(m => m.AgentId == agentId && m.Key == key)
			.Select(m => new AgentMemoryResponse(m.Id, m.AgentId, m.Key, m.Value, m.CreatedAt, m.UpdatedAt))
			.FirstOrDefaultAsync(ct);

		if (memory is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(memory);
	}

	private static async Task<Results<Ok<AgentMemoryResponse>, NotFound, ValidationProblem>> SetMemory(
		Guid agentId,
		string key,
		SetMemoryRequest request,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				["key"] = ["Key is required."]
			});
		}

		var agentExists = await db.Agents.AnyAsync(a => a.Id == agentId, ct);
		if (!agentExists)
		{
			return TypedResults.NotFound();
		}

		var existing = await db.AgentMemories
			.Where(m => m.AgentId == agentId && m.Key == key)
			.FirstOrDefaultAsync(ct);

		AgentMemoryResponse response;

		if (existing is not null)
		{
			existing.Value = request.Value;
			existing.UpdatedAt = DateTimeOffset.Now;
			await db.SaveChangesAsync(ct);
			response = new AgentMemoryResponse(existing.Id, existing.AgentId, existing.Key, existing.Value, existing.CreatedAt, existing.UpdatedAt);
		}
		else
		{
			var now = DateTimeOffset.Now;
			var memory = new CoreAgentMemory
			{
				Id = Guid.NewGuid(),
				AgentId = agentId,
				Key = key,
				Value = request.Value,
				CreatedAt = now,
				UpdatedAt = now,
			};
			db.AgentMemories.Add(memory);
			await db.SaveChangesAsync(ct);
			response = new AgentMemoryResponse(memory.Id, memory.AgentId, memory.Key, memory.Value, memory.CreatedAt, memory.UpdatedAt);
		}

		return TypedResults.Ok(response);
	}

	private static async Task<Results<NoContent, NotFound>> DeleteMemory(
		Guid agentId,
		string key,
		ApplicationDbContext db,
		CancellationToken ct)
	{
		var memory = await db.AgentMemories
			.Where(m => m.AgentId == agentId && m.Key == key)
			.FirstOrDefaultAsync(ct);

		if (memory is null)
		{
			return TypedResults.NotFound();
		}

		db.AgentMemories.Remove(memory);
		await db.SaveChangesAsync(ct);

		return TypedResults.NoContent();
	}
}
