// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using Docker.DotNet.Models;

using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak;

/// <inheritdoc />
public sealed class KeycloakBuilder : ContainerBuilder<KeycloakBuilder, KeycloakContainer, KeycloakConfiguration>
{
	/// <summary>The Keycloak image name.</summary>
	public const string KeycloakImage = "quay.io/keycloak/keycloak";

	/// <summary>The default Keycloak image version.</summary>
	public const string DefaultVersion = "21.1.1";

	/// <summary>The default Keycloak image name with version tag.</summary>
	public const string DefaultImage = $"{KeycloakImage}:{DefaultVersion}";

	/// <summary>The port on which Keycloak is listening on within the container.</summary>
	public const int KeycloakPort = 8080;

	/// <summary>The default Keycloak admin username.</summary>
	public const string DefaultUsername = "admin";

	/// <summary>The default Keycloak admin password.</summary>
	public const string DefaultPassword = "admin";

	/// <summary>Initializes a new instance of the <see cref="KeycloakBuilder"/> class.</summary>
	public KeycloakBuilder()
		: this(new())
	{
		DockerResourceConfiguration = Init().DockerResourceConfiguration;
	}

	private KeycloakBuilder(KeycloakConfiguration resourceConfiguration)
		: base(resourceConfiguration)
	{
		DockerResourceConfiguration = resourceConfiguration;
	}

	/// <inheritdoc />
	protected override KeycloakConfiguration DockerResourceConfiguration { get; }

	/// <summary>Sets the Keycloak admin username.</summary>
	/// <param name="username">The Keycloak admin username.</param>
	/// <returns>A configured instance of <see cref="KeycloakBuilder"/>.</returns>
	public KeycloakBuilder WithUsername(string username) =>
		Merge(DockerResourceConfiguration, new(username: username))
			.WithEnvironment("KEYCLOAK_ADMIN", username);

	/// <summary>Sets the Keycloak admin password.</summary>
	/// <param name="password">The Keycloak admin password.</param>
	/// <returns>A configured instance of <see cref="KeycloakBuilder"/>.</returns>
	public KeycloakBuilder WithPassword(string password) =>
		Merge(DockerResourceConfiguration, new(password: password))
			.WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", password);

	/// <summary>Sets the Keycloak realm configuration.</summary>
	/// <param name="realm">The Keycloak realm configuration.</param>
	/// <returns>A configured instance of <see cref="KeycloakBuilder"/>.</returns>
	public KeycloakBuilder WithRealm(RealmConfiguration realm) =>
		Merge(DockerResourceConfiguration, new(realm: realm));

	/// <inheritdoc />
	public override KeycloakContainer Build()
	{
		Validate();
		return new(DockerResourceConfiguration, TestcontainersSettings.Logger);
	}

	/// <inheritdoc />
	protected override KeycloakBuilder Init()
	{
		return base
			.Init()
			.WithImage(DefaultImage)
			.WithCommand("start-dev")
			.WithPortBinding(KeycloakPort, true)
			.WithUsername(DefaultUsername)
			.WithPassword(DefaultPassword)
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(KeycloakPort));
	}

	/// <inheritdoc />
	protected override void Validate()
	{
		base.Validate();

		_ = Guard.Argument(DockerResourceConfiguration.Username, nameof(DockerResourceConfiguration.Username))
			.NotNull()
			.NotEmpty();

		_ = Guard.Argument(DockerResourceConfiguration.Password, nameof(DockerResourceConfiguration.Password))
			.NotNull()
			.NotEmpty();

		_ = Guard.Argument(DockerResourceConfiguration.Realm, nameof(DockerResourceConfiguration.Realm))
			.NotNull();
	}

	/// <inheritdoc />
	protected override KeycloakBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) =>
		Merge(DockerResourceConfiguration, new(resourceConfiguration));

	/// <inheritdoc />
	protected override KeycloakBuilder Clone(IContainerConfiguration resourceConfiguration) =>
		Merge(DockerResourceConfiguration, new(resourceConfiguration));

	/// <inheritdoc />
	protected override KeycloakBuilder Merge(KeycloakConfiguration oldValue, KeycloakConfiguration newValue) =>
		new(new(oldValue, newValue));
}
