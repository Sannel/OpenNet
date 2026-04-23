// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace OpenNet.Agent;

public class OpenNetWorker : BackgroundService
{
	private readonly ILogger<OpenNetWorker> logger;

	public OpenNetWorker(ILogger<OpenNetWorker> logger)
	{
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			this.logger.LogInformation("OpenNet Agent running at: {time}", DateTimeOffset.UtcNow);
			await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
		}
	}
}
