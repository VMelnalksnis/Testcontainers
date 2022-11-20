// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

using Xunit.Abstractions;

namespace VMelnalksnis.Testcontainers.Keycloak.Tests;

public sealed class KeycloakTestcontainerTests : IAsyncLifetime
{
	private readonly ITestOutputHelper _testOutput;
	private readonly MemoryStream _standardStream = new();
	private readonly MemoryStream _errorStream = new();
	private readonly KeycloakTestcontainer _keycloak;
	private readonly Client _client;

	public KeycloakTestcontainerTests(ITestOutputHelper testOutput)
	{
		_testOutput = testOutput;
		var services = new ServiceCollection().AddLogging(builder => builder.AddXUnit(_testOutput));
		var provider = services.BuildServiceProvider();
		TestcontainersSettings.Logger = provider.GetRequiredService<ILogger<KeycloakTestcontainer>>();

		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper");
		_client = new("demoapp", new("http://localhost:8000/*"))
		{
			Secret = Guid.NewGuid().ToString(),
			ServiceAccountsEnabled = true,
			Mappers = new[] { mapper },
		};
		var user = new User("john.doe", "password123");
		var realmConfiguration = new RealmConfiguration("demorealm", new List<Client> { _client }, new List<User> { user });
		var keycloakConfiguration = new KeycloakTestcontainerConfiguration { Realms = new[] { realmConfiguration } };

		_keycloak = new TestcontainersBuilder<KeycloakTestcontainer>()
			.WithKeycloak(keycloakConfiguration)
			.WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(_standardStream, _errorStream))
			.Build();
	}

	public Task InitializeAsync() => _keycloak.StartAsync();

	[Fact]
	public async Task RealmShouldExist()
	{
		var response = await new HttpClient().GetAsync(_keycloak.Realms.Single().Metadata);
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync();
		_testOutput.WriteLine(content);
	}

	[Fact]
	public async Task ShouldAddExpectedAudience()
	{
		var realmUri = _keycloak.Realms.Single().ServerRealm;
		using var httpClient = new HttpClient();
		var requestContent = new FormUrlEncodedContent(new KeyValuePair<string?, string?>[]
		{
			new("client_id", _client.Name),
			new("client_secret", _client.Secret),
			new("scope", "openid profile offline_access"),
			new("grant_type", "client_credentials"),
		});

		var response = await httpClient.PostAsync($"{realmUri}/protocol/openid-connect/token", requestContent);
		var content = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
#if NET5_0_OR_GREATER

			throw new HttpRequestException(content, null, response.StatusCode);
#else
			throw new HttpRequestException(content);
#endif
		}

		var responseJson = JsonNode.Parse(content);
		var accessTokenValue = responseJson?["access_token"]?.GetValue<string>();
		var token = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenValue);

		using (new AssertionScope())
		{
			token.Issuer.Should().Be(realmUri.ToString());
			token.Audiences.Should().Contain(_client.Name);
		}
	}

	public async Task DisposeAsync()
	{
		_standardStream.Position = 0;
		var standardReader = new StreamReader(_standardStream);
		var standardText = await standardReader.ReadToEndAsync();
		_testOutput.WriteLine(standardText);

		_errorStream.Position = 0;
		var errorReader = new StreamReader(_errorStream);
		var errorText = await errorReader.ReadToEndAsync();
		_testOutput.WriteLine(errorText);

		await _keycloak.DisposeAsync();
	}
}
