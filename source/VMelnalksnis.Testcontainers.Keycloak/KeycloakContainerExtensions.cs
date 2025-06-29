// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using DotNet.Testcontainers.Containers;

using Testcontainers.Keycloak;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak;

/// <summary>Methods for configuring Keycloak containers.</summary>
public static class KeycloakContainerExtensions
{
	private const string _adminCommand = "/opt/keycloak/bin/kcadm.sh";

	/// <summary>Create a Keycloak realm.</summary>
	/// <param name="container">The container in which to create the realm.</param>
	/// <param name="configuration">The realm configuration to apply.</param>
	/// <param name="adminUsername">The admin username.</param>
	/// <param name="adminPassword">The admin password.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public static async Task ConfigureRealm(
		this KeycloakContainer container,
		RealmConfiguration configuration,
		string adminUsername,
		string adminPassword)
	{
		var result = await container.LogIn(adminUsername, adminPassword).ConfigureAwait(false);
		HandleResult(result);

		result = await container.CreateRealm(configuration).ConfigureAwait(false);
		HandleResult(result);

		foreach (var client in configuration.Clients)
		{
			result = await container.CreateClient(configuration, client).ConfigureAwait(false);
			HandleResult(result);

			var id = result.Stderr.Split('\'').Select(s => s.Trim()).Last(s => !string.IsNullOrWhiteSpace(s));

			foreach (var mapper in client.Mappers)
			{
				result = await container.CreateMapper(configuration, client, id, mapper).ConfigureAwait(false);
				HandleResult(result);

				if (client is { ServiceAccountsEnabled: true, ServiceAccountUser: { } serviceUser })
				{
					result = await container.GetServiceAccountUser(configuration, id).ConfigureAwait(false);
					HandleResult(result);

					var serviceUserId = JsonNode.Parse(result.Stdout)?["id"]?.GetValue<string>() ??
						throw new InvalidOperationException("Failed to get service account user id");
					result = await container.UpdateUser(configuration, serviceUser, serviceUserId)
						.ConfigureAwait(false);
					HandleResult(result);
				}

				result = await container.GetClient(configuration, id).ConfigureAwait(false);
				HandleResult(result);
			}
		}

		foreach (var user in configuration.Users)
		{
			result = await container.CreateUser(configuration, user).ConfigureAwait(false);
			HandleResult(result);

			result = await container.SetUserPassword(configuration, user).ConfigureAwait(false);
			HandleResult(result);
		}
	}

	private static Task<ExecResult> LogIn(this KeycloakContainer container, string username, string password) =>
		container.ExecAsync(new List<string>
		{
			_adminCommand, "config", "credentials",
			"--server", "http://localhost:8080/",
			"--realm", "master",
			"--user", username,
			"--password", password,
		});

	private static Task<ExecResult> CreateRealm(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration) => container.ExecAsync(
		new List<string>
		{
			_adminCommand, "create", "realms",
			"-s", $"realm={realmConfiguration.Name}",
			"-s", "enabled=true",
			"-o",
		});

	private static Task<ExecResult> CreateClient(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		Client client)
	{
		var command = new List<string>
		{
			_adminCommand, "create", "clients",
			"-r", realmConfiguration.Name,
			"-s", $"clientId={client.Name}",
			"-s", $"redirectUris=[{string.Join(",", client.RedirectUris.Select(uri => $"\"{uri}\""))}]",
			"-s", "protocol=openid-connect",
		};

		if (client.Secret is { } clientSecret)
		{
			command.AddRange(
			[
				"-s", "publicClient=false",
				"-s", "clientAuthenticatorType=client-secret",
				"-s", $"secret={clientSecret}"
			]);
		}

		if (client.ServiceAccountsEnabled is { } serviceAccount)
		{
			command.AddRange(
			[
				"-s", $"serviceAccountsEnabled=\"{serviceAccount.ToString().ToLowerInvariant()}\""
			]);
		}

		return container.ExecAsync(command);
	}

	private static Task<ExecResult> CreateMapper(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		Client client,
		string clientId,
		ClientProtocolMapper mapper) => container.ExecAsync(new List<string>
	{
		_adminCommand, "create", $"clients/{clientId}/protocol-mappers/models",
		"-r", realmConfiguration.Name,
		"-s", $"name={mapper.Name}",
		"-s", $"protocol={mapper.Protocol}",
		"-s", $"protocolMapper={mapper.ProtocolMapper}",
		"-s", $"consentRequired={mapper.ConsentRequired}",
		"-s", $"config.\"included.client.audience\"=\"{mapper.IncludedClientAudience ?? client.Name}\"",
		"-s", $"config.\"id.token.claim\"=\"{mapper.AddToIdToken.ToString().ToLowerInvariant()}\"",
		"-s", $"config.\"access.token.claim\"=\"{mapper.AddToAccessToken.ToString().ToLowerInvariant()}\"",
	});

	private static Task<ExecResult> GetServiceAccountUser(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		string clientId) => container.ExecAsync(
		new List<string>
		{
			_adminCommand, "get", $"clients/{clientId}/service-account-user",
			"-r", realmConfiguration.Name,
		});

	private static Task<ExecResult> GetClient(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		string clientId) => container.ExecAsync(
		new List<string>
		{
			_adminCommand, "get", $"clients/{clientId}/",
			"-r", realmConfiguration.Name,
		});

	private static Task<ExecResult> CreateUser(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		User user)
	{
		var command = new List<string>
		{
			_adminCommand, "create", "users",
			"-r", realmConfiguration.Name,
			"-s", $"username={user.Username}",
			"-s", "enabled=true",
		};

		if (user.Email is { } email)
		{
			command.AddRange(["-s", $"email={email}"]);
		}

		if (user.EmailVerified is { } emailVerified)
		{
			command.AddRange(["-s", $"emailVerified={emailVerified.ToString().ToLowerInvariant()}"]);
		}

		if (user.FirstName is { } firstName)
		{
			command.AddRange(["-s", $"firstName={firstName}"]);
		}

		if (user.LastName is { } lastName)
		{
			command.AddRange(["-s", $"lastName={lastName}"]);
		}

		return container.ExecAsync(command);
	}

	private static Task<ExecResult> UpdateUser(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		User user,
		string id)
	{
		var command = new List<string>
		{
			_adminCommand, "update", $"users/{id}",
			"-r", realmConfiguration.Name,
		};

		if (user.Email is { } email)
		{
			command.AddRange(["-s", $"email={email}"]);
		}

		if (user.EmailVerified is { } emailVerified)
		{
			command.AddRange(["-s", $"emailVerified={emailVerified.ToString().ToLowerInvariant()}"]);
		}

		if (user.FirstName is { } firstName)
		{
			command.AddRange(["-s", $"firstName={firstName}"]);
		}

		if (user.LastName is { } lastName)
		{
			command.AddRange(["-s", $"lastName={lastName}"]);
		}

		return container.ExecAsync(command);
	}

	private static Task<ExecResult> SetUserPassword(
		this KeycloakContainer container,
		RealmConfiguration realmConfiguration,
		User user) => container.ExecAsync(
		new List<string>
		{
			_adminCommand, "set-password",
			"-r", realmConfiguration.Name,
			"--username", user.Username,
			"--new-password", user.Password,
		});

	private static void HandleResult(ExecResult result)
	{
		if (result.ExitCode is not 0)
		{
			throw new ApplicationException(result.Stderr);
		}
	}
}
