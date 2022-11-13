// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace VMelnalksnis.Testcontainers.Paperless.Tests;

public sealed class PaperlessTestcontainerTests : IAsyncLifetime
{
	private readonly RedisTestcontainer _redis;

	public PaperlessTestcontainerTests()
	{
		var redisConfiguration = new RedisTestcontainerConfiguration("docker.io/library/redis:7.0.5");
		_redis = new TestcontainersBuilder<RedisTestcontainer>()
			.WithDatabase(redisConfiguration)
			.Build();
	}

	public Task InitializeAsync() => _redis.StartAsync();

	[Fact]
	public async Task LoginPageShouldWork()
	{
		var paperlessConfiguration = new PaperlessTestcontainerConfiguration();
		await using var paperless = new TestcontainersBuilder<PaperlessTestcontainer>()
			.WithPaperless(paperlessConfiguration, _redis)
			.Build();

		await paperless.StartAsync();

		var token = await paperless.GetAdminToken();
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = paperless.GetBaseAddress();
		httpClient.DefaultRequestHeaders.Authorization = new("Token", token);
		var response = await httpClient.GetAsync("/accounts/login/?next=/");

		using (new AssertionScope())
		{
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypeNames.Text.Html);
			response.Content.Headers.ContentType?.CharSet.Should().Be("utf-8");
		}
	}

	public Task DisposeAsync() => _redis.DisposeAsync().AsTask();
}
