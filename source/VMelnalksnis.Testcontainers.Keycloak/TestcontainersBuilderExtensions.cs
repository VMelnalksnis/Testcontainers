// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Builders;

namespace VMelnalksnis.Testcontainers.Keycloak;

/// <summary>Methods for configuring keycloak containers.</summary>
public static class TestcontainersBuilderExtensions
{
	/// <summary>Applies extended configuration for keycloak.</summary>
	/// <param name="builder">The container builder which to configure.</param>
	/// <param name="configuration">The configuration which to apply to the <paramref name="builder"/>.</param>
	/// <returns>A builder configured for keycloak.</returns>
	public static ITestcontainersBuilder<KeycloakTestcontainer> WithKeycloak(
		this ITestcontainersBuilder<KeycloakTestcontainer> builder,
		KeycloakTestcontainerConfiguration configuration) => builder
		.WithImage(configuration.Image)
		.WithExposedPort(configuration.DefaultPort)
		.WithPortBinding(configuration.Port, configuration.DefaultPort)
		.WithOutputConsumer(configuration.OutputConsumer)
		.WithWaitStrategy(configuration.WaitStrategy)
		.WithCommand("start-dev")
		.WithEnvironment("KEYCLOAK_ADMIN", configuration.Username)
		.WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", configuration.Password)
		.ConfigureContainer(keycloak =>
		{
			keycloak.Username = configuration.Username;
			keycloak.Password = configuration.Password;
			keycloak.ContainerPort = configuration.DefaultPort;
			if (configuration.Realms is { } realms)
			{
				keycloak.RealmConfigurations = realms;
			}
		});
}
