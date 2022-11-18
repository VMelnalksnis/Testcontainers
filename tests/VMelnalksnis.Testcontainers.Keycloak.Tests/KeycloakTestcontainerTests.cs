// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

	public KeycloakTestcontainerTests(ITestOutputHelper testOutput)
	{
		_testOutput = testOutput;
		var services = new ServiceCollection().AddLogging(builder => builder.AddXUnit(_testOutput));
		var provider = services.BuildServiceProvider();
		TestcontainersSettings.Logger = provider.GetRequiredService<ILogger<KeycloakTestcontainer>>();
	}

	public Task InitializeAsync() => Task.CompletedTask;

	[Fact]
	public async Task ShouldCreateRealm()
	{
		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper");
		var client = new Client("demoapp", new("http://localhost:8000/*"))
		{
			Secret = "client_secret",
			Mappers = new[] { mapper },
		};
		var user = new User("john.doe", "password123");
		var realmConfiguration = new RealmConfiguration("demorealm", new List<Client> { client }, new List<User> { user });
		var keycloakConfiguration = new KeycloakTestcontainerConfiguration { Realms = new[] { realmConfiguration } };

		var keycloak = new TestcontainersBuilder<KeycloakTestcontainer>()
			.WithKeycloak(keycloakConfiguration)
			.WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(_standardStream, _errorStream))
			.Build();

		await using (keycloak)
		{
			await keycloak.StartAsync();

			var response = await new HttpClient().GetAsync(keycloak.Realms.Single().Metadata);
			var content = await response.Content.ReadAsStringAsync();
			_testOutput.WriteLine(content);
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
	}
}
