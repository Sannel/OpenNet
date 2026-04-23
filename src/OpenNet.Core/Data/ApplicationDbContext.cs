// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Sannel.OpenNet.Core.Agents;

namespace Sannel.OpenNet.Core.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<Agent> Agents => Set<Agent>();

	public DbSet<AgentSession> AgentSessions => Set<AgentSession>();

	public DbSet<AgentMessage> AgentMessages => Set<AgentMessage>();

	public DbSet<AgentMemory> AgentMemories => Set<AgentMemory>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Agent>(entity =>
		{
			entity.HasKey(a => a.Id);
			entity.Property(a => a.Id).ValueGeneratedOnAdd();
			entity.Property(a => a.Name).HasMaxLength(256).IsRequired();
			entity.Property(a => a.ModelId).HasMaxLength(256).IsRequired();
			entity.Property(a => a.SystemPrompt).IsRequired();
			entity.Property(a => a.Provider).HasConversion<string>().HasMaxLength(64);

			entity.HasOne(a => a.ParentAgent)
				.WithMany(a => a.SubAgents)
				.HasForeignKey(a => a.ParentAgentId)
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired(false);
		});

		modelBuilder.Entity<AgentSession>(entity =>
		{
			entity.HasKey(s => s.Id);
			entity.Property(s => s.Id).ValueGeneratedOnAdd();

			entity.HasOne(s => s.Agent)
				.WithMany(a => a.Sessions)
				.HasForeignKey(s => s.AgentId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<AgentMessage>(entity =>
		{
			entity.HasKey(m => m.Id);
			entity.Property(m => m.Id).ValueGeneratedOnAdd();
			entity.Property(m => m.Content).IsRequired();
			entity.Property(m => m.Role).HasConversion<string>().HasMaxLength(32);

			entity.HasOne(m => m.Session)
				.WithMany(s => s.Messages)
				.HasForeignKey(m => m.SessionId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<AgentMemory>(entity =>
		{
			entity.HasKey(m => m.Id);
			entity.Property(m => m.Id).ValueGeneratedOnAdd();
			entity.Property(m => m.Key).HasMaxLength(512).IsRequired();
			entity.Property(m => m.Value).IsRequired();

			entity.HasIndex(m => new { m.AgentId, m.Key }).IsUnique();

			entity.HasOne(m => m.Agent)
				.WithMany(a => a.Memories)
				.HasForeignKey(m => m.AgentId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}
