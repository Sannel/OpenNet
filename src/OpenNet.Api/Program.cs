// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Sannel.OpenNet.Api.Features.System;
using Sannel.OpenNet.Core.Data;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

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
	authBuilder.AddOAuth("GitHub", options =>
	{
		options.ClientId = githubConfig["ClientId"]!;
		options.ClientSecret = githubConfig["ClientSecret"]!;
		options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
		options.TokenEndpoint = "https://github.com/login/oauth/access_token";
		options.UserInformationEndpoint = "https://api.github.com/user";
		options.CallbackPath = "/signin-github";
		options.Scope.Add("read:user");
		options.Scope.Add("user:email");
		options.Events.OnCreatingTicket = async context =>
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
			using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
			response.EnsureSuccessStatusCode();
			using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
			var user = json.RootElement;
			if (user.TryGetProperty("id", out var id))
			{
				context.Identity!.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.GetRawText().Trim('"')));
			}
			if (user.TryGetProperty("login", out var login))
			{
				context.Identity!.AddClaim(new Claim(ClaimTypes.Name, login.GetString() ?? string.Empty));
			}
			if (user.TryGetProperty("email", out var email) && email.ValueKind != JsonValueKind.Null)
			{
				context.Identity!.AddClaim(new Claim(ClaimTypes.Email, email.GetString() ?? string.Empty));
			}
		};
	});
}

foreach (var providerSection in builder.Configuration.GetSection("Authentication:OpenIdConnect").GetChildren())
{
	var clientId = providerSection["ClientId"];
	if (string.IsNullOrEmpty(clientId))
	{
		continue;
	}

	var schemeName = providerSection.Key;
	var clientSecret = providerSection["ClientSecret"]!;
	var authority = providerSection["Authority"]!;

	authBuilder.AddOpenIdConnect(schemeName, options =>
	{
		options.ClientId = clientId;
		options.ClientSecret = clientSecret;
		options.Authority = authority;
	});
}

builder.Services.AddAuthorization();

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
