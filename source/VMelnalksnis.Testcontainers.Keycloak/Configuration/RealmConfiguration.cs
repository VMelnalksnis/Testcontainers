// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Testcontainers.Keycloak;

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>Configuration for creating a keycloak realm.</summary>
/// <param name="Name">The name of the realm.</param>
/// <param name="Clients">The client applications.</param>
/// <param name="Users">The users.</param>
/// <param name="Port">The port on which Keycloak is available.</param>
public sealed record RealmConfiguration(
	string Name,
	IEnumerable<Client> Clients,
	IEnumerable<User> Users,
	ushort Port = KeycloakBuilder.KeycloakPort)
{
	/// <summary>Gets the configured Keycloak realm.</summary>
	/// <param name="container">The container for which to get the realm info.</param>
	/// <returns>Realm details for the given container.</returns>
	public Realm GetRealm(KeycloakContainer container) => new(this, container.GetMappedPublicPort(Port));
}
