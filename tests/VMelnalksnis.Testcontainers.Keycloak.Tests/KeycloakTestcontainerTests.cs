// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;

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
	}

	public Task InitializeAsync() => Task.CompletedTask;

	[Fact]
	public async Task ShouldCreateRealm()
	{
		var keycloakConfiguration = new KeycloakTestcontainerConfiguration
		{
			Realms = new RealmConfiguration[]
			{
				new(
					"demorealm",
					new List<Client>
					{
						new("demoapp", new("http://localhost:8000/*")) { Secret = Guid.NewGuid().ToString() },
					},
					new List<User>
					{
						new("john.doe", Guid.NewGuid().ToString()),
					}),
			},
		};

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
