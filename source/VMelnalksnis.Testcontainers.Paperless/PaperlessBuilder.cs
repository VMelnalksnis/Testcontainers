// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using Docker.DotNet.Models;

using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace VMelnalksnis.Testcontainers.Paperless;

/// <inheritdoc />
public sealed class PaperlessBuilder : ContainerBuilder<PaperlessBuilder, PaperlessContainer, PaperlessConfiguration>
{
	/// <summary>The Paperless image name.</summary>
	public const string PaperlessImage = "ghcr.io/paperless-ngx/paperless-ngx";

	/// <summary>The default Paperless image version.</summary>
	public const string DefaultVersion = "1.9.2";

	/// <summary>The default Paperless image name with version tag.</summary>
	public const string DefaultImage = $"{PaperlessImage}:{DefaultVersion}";

	/// <summary>The port on which Paperless is listening on within the container.</summary>
	public const int PaperlessPort = 8000;

	/// <summary>The default Paperless admin username.</summary>
	public const string DefaultUsername = "admin";

	/// <summary>The default Paperless admin password.</summary>
	public const string DefaultPassword = "admin";

	/// <summary>
	/// Initializes a new instance of the <see cref="PaperlessBuilder"/> class.
	/// </summary>
	public PaperlessBuilder()
		: this(new())
	{
		DockerResourceConfiguration = Init().DockerResourceConfiguration;
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessBuilder"/> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	private PaperlessBuilder(PaperlessConfiguration resourceConfiguration)
		: base(resourceConfiguration)
	{
		DockerResourceConfiguration = resourceConfiguration;
	}

	/// <inheritdoc />
	protected override PaperlessConfiguration DockerResourceConfiguration { get; }

	/// <summary>Sets the Paperless username.</summary>
	/// <param name="username">The Paperless username.</param>
	/// <returns>A configured instance of <see cref="PaperlessBuilder"/>.</returns>
	public PaperlessBuilder WithUsername(string username) =>
		Merge(DockerResourceConfiguration, new(username: username))
			.WithEnvironment("PAPERLESS_ADMIN_USER", username);

	/// <summary>Sets the Paperless password.</summary>
	/// <param name="password">The Paperless password.</param>
	/// <returns>A configured instance of <see cref="PaperlessBuilder"/>.</returns>
	public PaperlessBuilder WithPassword(string password) =>
		Merge(DockerResourceConfiguration, new(password: password))
			.WithEnvironment("PAPERLESS_ADMIN_PASSWORD", password);

	/// <summary>/// Sets the Redis connection string for Paperless.</summary>
	/// <param name="redisConnectionString">The Redis connection string to use.</param>
	/// <returns>A configured instance of <see cref="PaperlessBuilder"/>.</returns>
	public PaperlessBuilder WithRedis(string redisConnectionString) =>
		Merge(DockerResourceConfiguration, new(redisConnectionString: redisConnectionString))
			.WithEnvironment("PAPERLESS_REDIS", redisConnectionString);

	/// <inheritdoc />
	public override PaperlessContainer Build()
	{
		Validate();
		return new(DockerResourceConfiguration);
	}

	/// <inheritdoc />
	protected override PaperlessBuilder Init() => base
		.Init()
		.WithImage(DefaultImage)
		.WithPortBinding(PaperlessPort, true)
		.WithUsername(DefaultUsername)
		.WithPassword(DefaultPassword)
		.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PaperlessPort));

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

		_ = Guard.Argument(
				DockerResourceConfiguration.RedisConnectionString,
				nameof(DockerResourceConfiguration.RedisConnectionString))
			.NotNull()
			.NotEmpty();
	}

	/// <inheritdoc />
	protected override PaperlessBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) =>
		Merge(DockerResourceConfiguration, new(resourceConfiguration));

	/// <inheritdoc />
	protected override PaperlessBuilder Clone(IContainerConfiguration resourceConfiguration) =>
		Merge(DockerResourceConfiguration, new(resourceConfiguration));

	/// <inheritdoc />
	protected override PaperlessBuilder Merge(PaperlessConfiguration oldValue, PaperlessConfiguration newValue) =>
		new(new(oldValue, newValue));
}
