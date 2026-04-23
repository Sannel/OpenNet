// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.AI;

namespace Sannel.OpenNet.Core.AI;

public interface IAgentClientFactory
{
	IChatClient CreateChatClient(Agents.Agent agent);
}
