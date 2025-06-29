// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using Testcontainers.Keycloak;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

using Xunit.Abstractions;

namespace VMelnalksnis.Testcontainers.Keycloak.Tests;

[Collection("Keycloak")]
public sealed class KeycloakTestcontainerTests
{
	private readonly KeycloakFixture _fixture;
	private readonly ITestOutputHelper _testOutput;
	private readonly KeycloakContainer _keycloak;
	private readonly Client _client;

	public KeycloakTestcontainerTests(KeycloakFixture fixture, ITestOutputHelper testOutput)
	{
		_fixture = fixture;
		_testOutput = testOutput;

		_keycloak = fixture.Keycloak;
		_client = fixture.Client;
	}

	[Fact]
	public async Task RealmShouldExist()
	{
		var response = await new HttpClient().GetAsync(_fixture.Configuration.GetRealm(_keycloak).Metadata);
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync();
		_testOutput.WriteLine(content);
	}

	[Fact]
	public async Task ShouldAddExpectedAudience()
	{
		var realmUri = _fixture.Configuration.GetRealm(_keycloak).ServerRealm;
		using var httpClient = new HttpClient();
		var requestContent = new FormUrlEncodedContent(
		[
			new("client_id", _client.Name),
			new("client_secret", _client.Secret),
			new("scope", "openid profile offline_access"),
			new("grant_type", "client_credentials")
		]);

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

			token.Claims.Should()
				.ContainSingle(claim => claim.Type == "email")
				.Which.Value.Should()
				.Be(_client.ServiceAccountUser?.Email);

			token.Claims.Should()
				.ContainSingle(claim => claim.Type == "name")
				.Which.Value.Should()
				.Be($"{_client.ServiceAccountUser?.FirstName} {_client.ServiceAccountUser?.LastName}");
		}
	}
}
