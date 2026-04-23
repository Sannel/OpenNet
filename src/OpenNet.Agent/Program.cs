// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using OpenNet.Agent;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OpenNetWorker>();

var host = builder.Build();
await host.RunAsync();
