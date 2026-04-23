// Copyright (c) Sannel LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace OpenNet.Api.Features.System;

public record VersionInfo(string Version, string Environment, DateTimeOffset ServerTime);
