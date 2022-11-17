// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

	private Realm[] _realms = Array.Empty<Realm>();

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
	private KeycloakTestcontainer(ITestcontainersConfiguration configuration, ILogger logger)
		: base(configuration, logger)
	{
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

	private static void HandleResult(ExecResult result, Action<string>? log)
	{
		log?.Invoke($"Stdout: {result.Stdout}");
		log?.Invoke($"Stderr: {result.Stderr}");
		if (result.ExitCode is not 0)
		{
			throw new ApplicationException(result.Stderr);
		}
	}

	private async Task ConfigureRealm(RealmConfiguration realmConfiguration, Action<string>? log = null)
	{
		var result = await LogIn().ConfigureAwait(false);
		HandleResult(result, log);

		result = await CreateRealm(realmConfiguration).ConfigureAwait(false);
		HandleResult(result, log);

		foreach (var client in realmConfiguration.Clients)
		{
			result = await CreateClient(realmConfiguration, client).ConfigureAwait(false);
			HandleResult(result, log);

			var id = result.Stderr.Split('\'').Select(s => s.Trim()).Last(s => !string.IsNullOrWhiteSpace(s));

			foreach (var mapper in client.Mappers)
			{
				result = await CreateMapper(realmConfiguration, client, id, mapper).ConfigureAwait(false);
				HandleResult(result, log);
			}
		}

		foreach (var user in realmConfiguration.Users)
		{
			result = await CreateUser(realmConfiguration, user).ConfigureAwait(false);
			HandleResult(result, log);

			result = await SetUserPassword(realmConfiguration, user).ConfigureAwait(false);
			HandleResult(result, log);
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
		};

		if (client.Secret is { } clientSecret)
		{
			command.AddRange(new[]
			{
				"-s", "clientAuthenticatorType=client-secret",
				"-s", $"secret={clientSecret}",
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
		"-s", $"config.\"included.client.audience\"={client.Name}'",
		"-s", $"config.\"id.token.claim\"={mapper.AddToIdToken}",
		"-s", $"config.\"access.token.claim\"={mapper.AddToAccessToken}",
	});

	private Task<ExecResult> CreateUser(RealmConfiguration realmConfiguration, User user) => ExecAsync(new List<string>
	{
		_adminCommand, "create", "users",
		"-r", realmConfiguration.Name,
		"-s", $"username={user.Username}",
		"-s", "enabled=true",
	});

	private Task<ExecResult> SetUserPassword(RealmConfiguration realmConfiguration, User user) => ExecAsync(
		new List<string>
		{
			_adminCommand, "set-password",
			"-r", realmConfiguration.Name,
			"--username", user.Username,
			"--new-password", user.Password,
		});
}
