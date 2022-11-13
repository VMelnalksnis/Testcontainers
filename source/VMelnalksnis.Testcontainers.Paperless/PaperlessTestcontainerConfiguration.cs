// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace VMelnalksnis.Testcontainers.Paperless;

/// <summary><see cref="HostedServiceConfiguration"/> with defaults from paperless.</summary>
public sealed class PaperlessTestcontainerConfiguration : HostedServiceConfiguration
{
	/// <summary>The name of the paperless-ngx docker image without a version.</summary>
	public const string PaperlessImage = "ghcr.io/paperless-ngx/paperless-ngx";

	private const string _defaultImage = $"{PaperlessImage}:1.9.2";
	private const int _paperlessPort = 8000;
	private const string _defaultUsername = "admin";

	/// <summary>Initializes a new instance of the <see cref="PaperlessTestcontainerConfiguration"/> class.</summary>
	public PaperlessTestcontainerConfiguration()
		: this(_defaultImage)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="PaperlessTestcontainerConfiguration"/> class.</summary>
	/// <param name="image">The docker image.</param>
	/// <param name="defaultPort">The container port.</param>
	/// <param name="port">The host port.</param>
	public PaperlessTestcontainerConfiguration(string image, int defaultPort = _paperlessPort, int port = 0)
		: base(image, defaultPort, port)
	{
	}

	/// <inheritdoc />
	public override string Username { get; set; } = _defaultUsername;

	/// <inheritdoc />
	public override string Password { get; set; } = Guid.NewGuid().ToString("N");

	/// <inheritdoc />
	public override IWaitForContainerOS WaitStrategy => Wait
		.ForUnixContainer()
		.UntilPortIsAvailable(DefaultPort);
}
