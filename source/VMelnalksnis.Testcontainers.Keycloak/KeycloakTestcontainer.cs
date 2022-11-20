// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak;

/// <summary>An extended configured <see cref="HostedServiceContainer"/> for keycloak.</summary>
public sealed class KeycloakTestcontainer : HostedServiceContainer
{
	private const string _adminCommand = "/opt/keycloak/bin/kcadm.sh";

	private readonly ILogger _logger;

	private Realm[] _realms = Array.Empty<Realm>();

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
	private KeycloakTestcontainer(ITestcontainersConfiguration configuration, ILogger logger)
		: base(configuration, logger)
	{
		_logger = logger;
		RealmConfigurations = Array.Empty<RealmConfiguration>();
	}

	/// <summary>Gets the realms configured in this keycloak instance.</summary>
	public IEnumerable<Realm> Realms => _realms;

	internal IEnumerable<RealmConfiguration> RealmConfigurations { get; set; }

	/// <inheritdoc />
	public override async Task StartAsync(CancellationToken ct = default)
	{
		await base.StartAsync(ct).ConfigureAwait(false);
		foreach (var realm in RealmConfigurations)
		{
			await ConfigureRealm(realm).ConfigureAwait(false);
		}

		_realms = RealmConfigurations.Select(realm => new Realm(realm, GetMappedPublicPort(ContainerPort))).ToArray();
	}

	private void HandleResult(ExecResult result)
	{
		_logger.LogInformation("Stdout {Stdout}", result.Stdout);
		_logger.LogInformation("Stderr {Stderr}", result.Stderr);
		if (result.ExitCode is not 0)
		{
			throw new ApplicationException(result.Stderr);
		}
	}

	private async Task ConfigureRealm(RealmConfiguration realmConfiguration)
	{
		var result = await LogIn().ConfigureAwait(false);
		HandleResult(result);

		result = await CreateRealm(realmConfiguration).ConfigureAwait(false);
		HandleResult(result);

		foreach (var client in realmConfiguration.Clients)
		{
			result = await CreateClient(realmConfiguration, client).ConfigureAwait(false);
			HandleResult(result);

			var id = result.Stderr.Split('\'').Select(s => s.Trim()).Last(s => !string.IsNullOrWhiteSpace(s));

			foreach (var mapper in client.Mappers)
			{
				result = await CreateMapper(realmConfiguration, client, id, mapper).ConfigureAwait(false);
				HandleResult(result);

				if (client.ServiceAccountsEnabled is true && client.ServiceAccountUser is { } serviceUser)
				{
					result = await GetServiceAccountUser(realmConfiguration, id).ConfigureAwait(false);
					HandleResult(result);

					var serviceUserId = JsonNode.Parse(result.Stdout)?["id"]?.GetValue<string>() ??
						throw new InvalidOperationException("Failed to get service account user id");
					result = await UpdateUser(realmConfiguration, serviceUser, serviceUserId).ConfigureAwait(false);
					HandleResult(result);
				}

				result = await GetClient(realmConfiguration, id).ConfigureAwait(false);
				HandleResult(result);
			}
		}

		foreach (var user in realmConfiguration.Users)
		{
			result = await CreateUser(realmConfiguration, user).ConfigureAwait(false);
			HandleResult(result);

			result = await SetUserPassword(realmConfiguration, user).ConfigureAwait(false);
			HandleResult(result);
		}
	}

	private Task<ExecResult> LogIn() => ExecAsync(new List<string>
	{
		_adminCommand, "config", "credentials",
		"--server", "http://localhost:8080/",
		"--realm", "master",
		"--user", Username,
		"--password", Password,
	});

	private Task<ExecResult> CreateRealm(RealmConfiguration realmConfiguration) => ExecAsync(new List<string>
	{
		_adminCommand, "create", "realms",
		"-s", $"realm={realmConfiguration.Name}",
		"-s", "enabled=true",
		"-o",
	});

	private Task<ExecResult> CreateClient(RealmConfiguration realmConfiguration, Client client)
	{
		var command = new List<string>
		{
			_adminCommand, "create", "clients",
			"-r", realmConfiguration.Name,
			"-s", $"clientId={client.Name}",
			"-s", $"redirectUris=[\"{client.RedirectUri}\"]",
			"-s", "protocol=openid-connect",
		};

		if (client.Secret is { } clientSecret)
		{
			command.AddRange(new[]
			{
				"-s", "publicClient=false",
				"-s", "clientAuthenticatorType=client-secret",
				"-s", $"secret={clientSecret}",
			});
		}

		if (client.ServiceAccountsEnabled is { } serviceAccount)
		{
			command.AddRange(new[]
			{
				"-s", $"serviceAccountsEnabled=\"{serviceAccount.ToString().ToLowerInvariant()}\"",
			});
		}

		return ExecAsync(command);
	}

	private Task<ExecResult> CreateMapper(
		RealmConfiguration realmConfiguration,
		Client client,
		string clientId,
		ClientProtocolMapper mapper) => ExecAsync(new List<string>
	{
		_adminCommand, "create", $"clients/{clientId}/protocol-mappers/models",
		"-r", realmConfiguration.Name,
		"-s", $"name={mapper.Name}",
		"-s", $"protocol={mapper.Protocol}",
		"-s", $"protocolMapper={mapper.ProtocolMapper}",
		"-s", $"consentRequired={mapper.ConsentRequired}",
		"-s", $"config.\"included.client.audience\"=\"{client.Name}\"",
		"-s", $"config.\"id.token.claim\"=\"{mapper.AddToIdToken.ToString().ToLowerInvariant()}\"",
		"-s", $"config.\"access.token.claim\"=\"{mapper.AddToAccessToken.ToString().ToLowerInvariant()}\"",
	});

	private Task<ExecResult> GetServiceAccountUser(RealmConfiguration realmConfiguration, string clientId) => ExecAsync(
		new List<string>
		{
			_adminCommand, "get", $"clients/{clientId}/service-account-user",
			"-r", realmConfiguration.Name,
		});

	private Task<ExecResult> GetClient(RealmConfiguration realmConfiguration, string clientId) => ExecAsync(
		new List<string>
		{
			_adminCommand, "get", $"clients/{clientId}/",
			"-r", realmConfiguration.Name,
		});

	private Task<ExecResult> CreateUser(RealmConfiguration realmConfiguration, User user)
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
			command.AddRange(new[] { "-s", $"email={email}" });
		}

		if (user.EmailVerified is { } emailVerified)
		{
			command.AddRange(new[] { "-s", $"emailVerified={emailVerified.ToString().ToLowerInvariant()}" });
		}

		if (user.FirstName is { } firstName)
		{
			command.AddRange(new[] { "-s", $"firstName={firstName}" });
		}

		if (user.LastName is { } lastName)
		{
			command.AddRange(new[] { "-s", $"lastName={lastName}" });
		}

		return ExecAsync(command);
	}

	private Task<ExecResult> UpdateUser(RealmConfiguration realmConfiguration, User user, string id)
	{
		var command = new List<string>
		{
			_adminCommand, "update", $"users/{id}",
			"-r", realmConfiguration.Name,
		};

		if (user.Email is { } email)
		{
			command.AddRange(new[] { "-s", $"email={email}" });
		}

		if (user.EmailVerified is { } emailVerified)
		{
			command.AddRange(new[] { "-s", $"emailVerified={emailVerified.ToString().ToLowerInvariant()}" });
		}

		if (user.FirstName is { } firstName)
		{
			command.AddRange(new[] { "-s", $"firstName={firstName}" });
		}

		if (user.LastName is { } lastName)
		{
			command.AddRange(new[] { "-s", $"lastName={lastName}" });
		}

		return ExecAsync(command);
	}

	private Task<ExecResult> SetUserPassword(RealmConfiguration realmConfiguration, User user) => ExecAsync(
		new List<string>
		{
			_adminCommand, "set-password",
			"-r", realmConfiguration.Name,
			"--username", user.Username,
			"--new-password", user.Password,
		});
}
