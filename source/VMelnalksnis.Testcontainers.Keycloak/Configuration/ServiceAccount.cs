// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>A user which is used when authenticating using client credentials.</summary>
public sealed record ServiceAccount() : User(string.Empty, string.Empty);
