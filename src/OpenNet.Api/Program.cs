// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using OpenNet.Api.Features.System;
using OpenNet.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
var dbProvider = builder.Configuration["Database:Provider"] ?? "sqlite";
var connectionString = builder.Configuration["Database:ConnectionString"] ?? "Data Source=opennet.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	switch (dbProvider.ToLowerInvariant())
	{
		case "sqlserver":
			options.UseSqlServer(connectionString);
			break;
		case "postgres":
			options.UseNpgsql(connectionString);
			break;
		default:
			options.UseSqlite(connectionString);
			break;
	}
});

// Authentication
var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie();

var githubConfig = builder.Configuration.GetSection("Authentication:GitHub");
if (githubConfig.Exists() && !string.IsNullOrEmpty(githubConfig["ClientId"]))
{
	authBuilder.AddOpenIdConnect("GitHub", options =>
	{
		options.ClientId = githubConfig["ClientId"]!;
		options.ClientSecret = githubConfig["ClientSecret"]!;
		options.Authority = "https://github.com";
	});
}

var googleConfig = builder.Configuration.GetSection("Authentication:Google");
if (googleConfig.Exists() && !string.IsNullOrEmpty(googleConfig["ClientId"]))
{
	authBuilder.AddOpenIdConnect("Google", options =>
	{
		options.ClientId = googleConfig["ClientId"]!;
		options.ClientSecret = googleConfig["ClientSecret"]!;
		options.Authority = "https://accounts.google.com";
	});
}

var entraConfig = builder.Configuration.GetSection("Authentication:EntraId");
if (entraConfig.Exists() && !string.IsNullOrEmpty(entraConfig["ClientId"]))
{
	authBuilder.AddOpenIdConnect("EntraId", options =>
	{
		options.ClientId = entraConfig["ClientId"]!;
		options.ClientSecret = entraConfig["ClientSecret"]!;
		options.Authority = $"https://login.microsoftonline.com/{entraConfig["TenantId"]}/v2.0";
	});
}

// Health checks
builder.Services.AddHealthChecks();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new ApiVersion(1, 0);
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.ReportApiVersions = true;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapSystemEndpoints();

app.MapFallbackToFile("index.html");

await app.RunAsync();
