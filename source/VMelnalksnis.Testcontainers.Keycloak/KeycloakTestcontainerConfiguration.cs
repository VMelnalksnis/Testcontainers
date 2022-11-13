// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

using VMelnalksnis.Testcontainers.Keycloak.Configuration;

namespace VMelnalksnis.Testcontainers.Keycloak;

/// <summary><see cref="HostedServiceConfiguration"/> with defaults and additional options for keycloak.</summary>
public sealed class KeycloakTestcontainerConfiguration : HostedServiceConfiguration
{
	/// <summary>The name of the keycloak docker image without a version.</summary>
	public const string KeycloakImage = "quay.io/keycloak/keycloak";

	private const string _defaultImage = $"{KeycloakImage}:19.0.1";
	private const int _defaultPort = 8080;
	private const string _defaultUsername = "admin";

	/// <summary>Initializes a new instance of the <see cref="KeycloakTestcontainerConfiguration"/> class.</summary>
	public KeycloakTestcontainerConfiguration()
		: this(_defaultImage)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="KeycloakTestcontainerConfiguration"/> class.</summary>
	/// <param name="image">The docker image.</param>
	/// <param name="defaultPort">The container port.</param>
	/// <param name="port">The host port.</param>
	public KeycloakTestcontainerConfiguration(string image, int defaultPort = _defaultPort, int port = 0)
		: base(image, defaultPort, port)
	{
	}

	/// <summary>Gets or sets the realm to create in keycloak.</summary>
	public RealmConfiguration[]? Realms { get; set; }

	/// <inheritdoc />
	public override string Username { get; set; } = _defaultUsername;

	/// <inheritdoc />
	public override string Password { get; set; } = Guid.NewGuid().ToString("N");

	/// <inheritdoc />
	public override IWaitForContainerOS WaitStrategy => Wait
		.ForUnixContainer()
		.UntilPortIsAvailable(DefaultPort);
}
