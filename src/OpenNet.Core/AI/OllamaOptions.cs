// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Sannel.OpenNet.Core.AI;

public class OllamaOptions
{
	public const string SectionName = "AI:Ollama";

	public string Endpoint { get; set; } = "http://localhost:11434";

	public string ModelId { get; set; } = "llama3.2";
}
