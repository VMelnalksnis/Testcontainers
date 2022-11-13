// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>Configuration for creating a keycloak realm.</summary>
/// <param name="Name">The name of the realm.</param>
/// <param name="Clients">The client applications.</param>
/// <param name="Users">The users.</param>
public sealed record RealmConfiguration(string Name, IEnumerable<Client> Clients, IEnumerable<User> Users);
