// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>A keycloak user.</summary>
/// <param name="Username">The username of the user.</param>
/// <param name="Password">The password of the user.</param>
public sealed record User(string Username, string Password);
