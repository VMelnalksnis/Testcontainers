// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Testcontainers.Keycloak;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak.Tests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class KeycloakFixture : IAsyncLifetime
{
	public KeycloakFixture()
	{
		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper");
		var mapperDesktop =
			new ClientProtocolMapper("audience-mapping-desktop", "openid-connect", "oidc-audience-mapper")
			{
				IncludedClientAudience = "demoapp-desktop",
			};
		Client = new("demoapp", new("http://localhost:8000/*"))
		{
			Secret = Guid.NewGuid().ToString(),
			ServiceAccountsEnabled = true,
			Mappers = new[] { mapper, mapperDesktop },
			ServiceAccountUser = new()
			{
				Email = "service-account@example.com",
				FirstName = "Service",
				LastName = "Account",
				EmailVerified = true,
			},
		};
		var user = new User("john.doe", "password123")
		{
			Email = "john.doe@example.com",
			EmailVerified = true,
			FirstName = "John",
			LastName = "Doe",
		};
		Configuration = new("demorealm", new List<Client> { Client }, new List<User> { user });

		Keycloak = new KeycloakBuilder().WithUsername("admin").WithPassword("admin").Build();
	}

	internal KeycloakContainer Keycloak { get; }

	internal Client Client { get; }

	internal RealmConfiguration Configuration { get; }

	public async Task InitializeAsync()
	{
		await Keycloak.StartAsync();
		await Keycloak.ConfigureRealm(Configuration, "admin", "admin");
	}

	public Task DisposeAsync() => Keycloak.DisposeAsync().AsTask();
}
