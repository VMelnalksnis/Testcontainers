// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using Docker.DotNet.Models;

using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;

namespace VMelnalksnis.Testcontainers.Paperless;

/// <inheritdoc />
public sealed class PaperlessBuilder : ContainerBuilder<PaperlessBuilder, PaperlessContainer, PaperlessConfiguration>
{
	/// <summary>The Paperless image name.</summary>
	public const string PaperlessImage = "ghcr.io/paperless-ngx/paperless-ngx";

	/// <summary>The default Paperless image version.</summary>
	[Obsolete("This constant is obsolete and will be removed in the future. Use the constructor with the image parameter instead: https://github.com/testcontainers/testcontainers-dotnet/discussions/1470#discussioncomment-15185721.")]
	public const string DefaultVersion = "1.9.2";

	/// <summary>The default Paperless image name with version tag.</summary>
	[Obsolete("This constant is obsolete and will be removed in the future. Use the constructor with the image parameter instead: https://github.com/testcontainers/testcontainers-dotnet/discussions/1470#discussioncomment-15185721.")]
	public const string DefaultImage = $"{PaperlessImage}:{DefaultVersion}";

	/// <summary>The port on which Paperless is listening on within the container.</summary>
	public const int PaperlessPort = 8000;

	/// <summary>The default Paperless admin username.</summary>
	public const string DefaultUsername = "admin";

	/// <summary>The default Paperless admin password.</summary>
	public const string DefaultPassword = "admin";

	/// <summary>Initializes a new instance of the <see cref="PaperlessBuilder"/> class.</summary>
	[Obsolete("This parameterless constructor is obsolete and will be removed. Use the constructor with the image parameter instead: https://github.com/testcontainers/testcontainers-dotnet/discussions/1470#discussioncomment-15185721.")]
	[ExcludeFromCodeCoverage]
	public PaperlessBuilder()
		: this(DefaultImage)
	{
		DockerResourceConfiguration = Init().DockerResourceConfiguration;
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessBuilder"/> class.</summary>
	/// <param name="image">The full Docker image name, including the image repository and tag (e.g., <see cref="DefaultImage"/>).</param>
	/// <remarks>Docker image tags available at <see href="https://github.com/paperless-ngx/paperless-ngx/pkgs/container/paperless-ngx/versions?filters%5Bversion_type%5D=tagged"/>.</remarks>
	public PaperlessBuilder(string image)
		: this(new DockerImage(image))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessBuilder"/> class.</summary>
	/// <param name="image">An <see cref="IImage"/> instance that specified the Docker image to be used for the container builder configuration.</param>
	/// <remarks>Docker image tags available at <see href="https://github.com/paperless-ngx/paperless-ngx/pkgs/container/paperless-ngx/versions?filters%5Bversion_type%5D=tagged"/>.</remarks>
	public PaperlessBuilder(IImage image)
		: this(new PaperlessConfiguration())
	{
		DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
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
		.WithPortBinding(PaperlessPort, true)
		.WithUsername(DefaultUsername)
		.WithPassword(DefaultPassword)
		.WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(PaperlessPort));

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
	protected override PaperlessBuilder Clone(
		IResourceConfiguration<CreateContainerParameters> resourceConfiguration) =>
		Merge(DockerResourceConfiguration, new(resourceConfiguration));

	/// <inheritdoc />
	protected override PaperlessBuilder Clone(IContainerConfiguration resourceConfiguration) =>
		Merge(DockerResourceConfiguration, new(resourceConfiguration));

	/// <inheritdoc />
	protected override PaperlessBuilder Merge(PaperlessConfiguration oldValue, PaperlessConfiguration newValue) =>
		new(new PaperlessConfiguration(oldValue, newValue));
}
