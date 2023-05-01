// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using Docker.DotNet.Models;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace VMelnalksnis.Testcontainers.Paperless;

/// <inheritdoc />
public sealed class PaperlessConfiguration : ContainerConfiguration
{
	/// <summary>Initializes a new instance of the <see cref="PaperlessConfiguration" /> class.</summary>
	/// <param name="username">The Paperless username.</param>
	/// <param name="password">The Paperless password.</param>
	/// <param name="redisConnectionString">The Redis connection string to use for Paperless.</param>
	public PaperlessConfiguration(
		string username = null!,
		string password = null!,
		string redisConnectionString = null!)
	{
		Username = username;
		Password = password;
		RedisConnectionString = redisConnectionString;
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessConfiguration" /> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	public PaperlessConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
		: base(resourceConfiguration)
	{
		// Passes the configuration upwards to the base implementations to create an updated immutable copy.
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessConfiguration" /> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	public PaperlessConfiguration(IContainerConfiguration resourceConfiguration)
		: base(resourceConfiguration)
	{
		// Passes the configuration upwards to the base implementations to create an updated immutable copy.
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessConfiguration" /> class.</summary>
	/// <param name="resourceConfiguration">The Docker resource configuration.</param>
	public PaperlessConfiguration(PaperlessConfiguration resourceConfiguration)
		: this(new(), resourceConfiguration)
	{
		// Passes the configuration upwards to the base implementations to create an updated immutable copy.
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessConfiguration" /> class.</summary>
	/// <param name="oldValue">The old Docker resource configuration.</param>
	/// <param name="newValue">The new Docker resource configuration.</param>
	public PaperlessConfiguration(PaperlessConfiguration oldValue, PaperlessConfiguration newValue)
		: base(oldValue, newValue)
	{
		Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
		Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
		RedisConnectionString =
			BuildConfiguration.Combine(oldValue.RedisConnectionString, newValue.RedisConnectionString);
	}

	/// <summary>Gets the Paperless username.</summary>
	public string Username { get; } = null!;

	/// <summary>Gets the Paperless password.</summary>
	public string Password { get; } = null!;

	/// <summary>Gets the Redis connection string.</summary>
	public string RedisConnectionString { get; } = null!;
}
