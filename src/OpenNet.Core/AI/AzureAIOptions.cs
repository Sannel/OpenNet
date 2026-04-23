// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Sannel.OpenNet.Core.AI;

public class AzureAIOptions
{
	public const string SectionName = "AI:AzureAI";

	public string Endpoint { get; set; } = string.Empty;

	public string ApiKey { get; set; } = string.Empty;

	public string ModelId { get; set; } = string.Empty;
}
