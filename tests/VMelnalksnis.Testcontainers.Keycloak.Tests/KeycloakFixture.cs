// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;

using JetBrains.Annotations;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak.Tests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class KeycloakFixture : IAsyncLifetime
{
	public KeycloakFixture()
	{
		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper");
		Client = new("demoapp", new("http://localhost:8000/*"))
		{
			Secret = Guid.NewGuid().ToString(),
			ServiceAccountsEnabled = true,
			Mappers = new[] { mapper },
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
		var realmConfiguration = new RealmConfiguration("demorealm", new List<Client> { Client }, new List<User> { user });
		var keycloakConfiguration = new KeycloakTestcontainerConfiguration { Realms = new[] { realmConfiguration } };

		Keycloak = new TestcontainersBuilder<KeycloakTestcontainer>()
			.WithKeycloak(keycloakConfiguration)
			.Build();
	}

	internal KeycloakTestcontainer Keycloak { get; }

	internal Client Client { get; }

	public Task InitializeAsync() => Keycloak.StartAsync();

	public Task DisposeAsync() => Keycloak.DisposeAsync().AsTask();
}
