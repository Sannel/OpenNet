// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Sannel.OpenNet.Agent;

var builder = Host.CreateDefaultBuilder(args);

if (OperatingSystem.IsLinux())
{
	builder.UseSystemd();
}

builder.ConfigureServices(services =>
{
	services.AddHostedService<OpenNetWorker>();
});

var host = builder.Build();
await host.RunAsync();
