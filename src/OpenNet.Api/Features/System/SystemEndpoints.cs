// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Asp.Versioning;
using Microsoft.AspNetCore.Routing;

namespace OpenNet.Api.Features.System;

public static class SystemEndpoints
{
	public static void MapSystemEndpoints(this IEndpointRouteBuilder app)
	{
		var versionedGroup = app.NewVersionedApi();
		var v1 = versionedGroup.MapGroup("/api/v{version:apiVersion}/system").HasApiVersion(1.0);
		v1.MapGet("/info", GetInfo);
	}

	private static VersionInfo GetInfo(IWebHostEnvironment env)
	{
		return new VersionInfo(
			typeof(SystemEndpoints).Assembly.GetName().Version?.ToString() ?? "unknown",
			env.EnvironmentName,
			DateTimeOffset.UtcNow
		);
	}
}
