// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace VMelnalksnis.Testcontainers.Paperless;

/// <summary>Methods for configuring paperless containers.</summary>
public static class TestcontainersBuilderExtensions
{
	/// <summary>Applies extended configuration for paperless.</summary>
	/// <param name="builder">The container builder which to configure.</param>
	/// <param name="configuration">The configuration which to apply to the <paramref name="builder"/>.</param>
	/// <param name="redis">The Redis container to be used by the paperless container.</param>
	/// <returns>A builder configured for paperless.</returns>
	public static ITestcontainersBuilder<PaperlessTestcontainer> WithPaperless(
		this ITestcontainersBuilder<PaperlessTestcontainer> builder,
		PaperlessTestcontainerConfiguration configuration,
		RedisTestcontainer redis) => builder
		.WithImage(configuration.Image)
		.WithExposedPort(configuration.DefaultPort)
		.WithPortBinding(configuration.Port, configuration.DefaultPort)
		.WithOutputConsumer(configuration.OutputConsumer)
		.WithWaitStrategy(configuration.WaitStrategy)
		.WithEnvironment("PAPERLESS_REDIS", $"redis://{redis.IpAddress}:{redis.ContainerPort}")
		.WithEnvironment("PAPERLESS_ADMIN_USER", configuration.Username)
		.WithEnvironment("PAPERLESS_ADMIN_PASSWORD", configuration.Password)
		.ConfigureContainer(paperless =>
		{
			paperless.Username = configuration.Username;
			paperless.Password = configuration.Password;
			paperless.ContainerPort = configuration.DefaultPort;
		});
}
