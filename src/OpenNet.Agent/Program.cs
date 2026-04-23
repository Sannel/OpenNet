// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Sannel.OpenNet.Agent;
using Sannel.OpenNet.Agent.AI;
using Sannel.OpenNet.Core.AI;

var builder = Host.CreateDefaultBuilder(args);

if (OperatingSystem.IsLinux())
{
	builder.UseSystemd();
}

builder.ConfigureServices((context, services) =>
{
	services.AddHostedService<OpenNetWorker>();

	// AI provider options and factory
	services.Configure<OllamaOptions>(context.Configuration.GetSection(OllamaOptions.SectionName));
	services.Configure<AzureAIOptions>(context.Configuration.GetSection(AzureAIOptions.SectionName));
	services.AddScoped<IAgentClientFactory, AgentClientFactory>();
	services.AddScoped<AgentClientFactory>();
	services.AddScoped<AgentSessionService>();
	services.AddSingleton<AgentRunner>();
	services.AddScoped<SubAgentTool>();
});

var host = builder.Build();
await host.RunAsync();
