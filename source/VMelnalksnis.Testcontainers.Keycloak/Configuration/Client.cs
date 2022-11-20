// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>A keycloak client application.</summary>
/// <param name="Name">The name of the client.</param>
/// <param name="RedirectUri">The redirect uri of the client.</param>
public sealed record Client(string Name, Uri RedirectUri)
{
	/// <summary>Gets the client secret.</summary>
	public string? Secret { get; init; }

	/// <summary>Gets a value indicating whether service accounts are enabled for this client.</summary>
	public bool? ServiceAccountsEnabled { get; init; }

	/// <summary>Gets the protocol mappers for this client.</summary>
	public IEnumerable<ClientProtocolMapper> Mappers { get; init; } = Array.Empty<ClientProtocolMapper>();
}
