// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>A keycloak realm.</summary>
public sealed class Realm
{
	/// <summary>Initializes a new instance of the <see cref="Realm"/> class.</summary>
	/// <param name="configuration">The configuration from which the realm was created.</param>
	/// <param name="port">The port of the keycloak container.</param>
	public Realm(RealmConfiguration configuration, int port)
	{
		Name = configuration.Name;
		ServerRealm = new($"http://localhost:{port}/realms/{Name}");
		Metadata = new($"{ServerRealm}/.well-known/openid-configuration");
	}

	/// <summary>Gets the name of the realm.</summary>
	public string Name { get; }

	/// <summary>Gets the realm Uri.</summary>
	public Uri ServerRealm { get; }

	/// <summary>Gets the realm metadata Uri.</summary>
	public Uri Metadata { get; }
}
