// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

using JetBrains.Annotations;

using Testcontainers.Redis;

namespace VMelnalksnis.Testcontainers.Paperless.Tests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PaperlessFixture : IAsyncLifetime
{
	private readonly RedisContainer _redis;
	private readonly INetwork _network;

	public PaperlessFixture()
	{
		const string redis = "redis";

		_network = new NetworkBuilder().Build();

		_redis = new RedisBuilder()
			.WithImage("docker.io/library/redis:7.0.11")
			.WithNetwork(_network)
			.WithNetworkAliases(redis)
			.Build();

		Paperless = new PaperlessBuilder()
			.WithNetwork(_network)
			.DependsOn(_redis)
			.WithRedis($"redis://{redis}:{RedisBuilder.RedisPort}")
			.Build();
	}

	internal PaperlessContainer Paperless { get; }

	/// <inheritdoc />
	public Task InitializeAsync() => Paperless.StartAsync();

	/// <inheritdoc />
	public async Task DisposeAsync()
	{
		await Paperless.DisposeAsync();
		await _redis.DisposeAsync();
		await _network.DisposeAsync();
	}
}
