// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using Docker.DotNet.Models;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak;

/// <inheritdoc />
public sealed class KeycloakConfiguration : ContainerConfiguration
{
	/// <summary>Initializes a new instance of the <see cref="KeycloakConfiguration"/> class.</summary>
	/// <param name="username">The admin username.</param>
	/// <param name="password">The admin password.</param>
	/// <param name="realm">The Keycloak realm configuration.</param>
	public KeycloakConfiguration(
		string username = null!,
		string password = null!,
		RealmConfiguration realm = null!)
	{
		Username = username;
		Password = password;
		Realm = realm;
	}

	/// <summary>Initializes a new instance of the <see cref="KeycloakConfiguration"/> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	public KeycloakConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
		: base(resourceConfiguration)
	{
		// Passes the configuration upwards to the base implementations to create an updated immutable copy.
	}

	/// <summary>Initializes a new instance of the <see cref="KeycloakConfiguration"/> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	public KeycloakConfiguration(IContainerConfiguration resourceConfiguration)
		: base(resourceConfiguration)
	{
		// Passes the configuration upwards to the base implementations to create an updated immutable copy.
	}

	/// <summary>Initializes a new instance of the <see cref="KeycloakConfiguration"/> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	public KeycloakConfiguration(KeycloakConfiguration resourceConfiguration)
		: this(new(), resourceConfiguration)
	{
		// Passes the configuration upwards to the base implementations to create an updated immutable copy.
	}

	/// <summary>Initializes a new instance of the <see cref="KeycloakConfiguration"/> class.</summary>
	/// <param name="oldValue">The old Docker resource configuration.</param>
	/// <param name="newValue">The new Docker resource configuration.</param>
	public KeycloakConfiguration(KeycloakConfiguration oldValue, KeycloakConfiguration newValue)
		: base(oldValue, newValue)
	{
		Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
		Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
		Realm = BuildConfiguration.Combine(oldValue.Realm, newValue.Realm);
	}

	/// <summary>Gets the admin username.</summary>
	public string Username { get; } = null!;

	/// <summary>Gets the admin password.</summary>
	public string Password { get; } = null!;

	/// <summary>Gets the Keycloak realm configuration.</summary>
	public RealmConfiguration Realm { get; } = null!;
}
