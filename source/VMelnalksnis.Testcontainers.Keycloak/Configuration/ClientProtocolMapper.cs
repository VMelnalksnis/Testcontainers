// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>A protocol mapper for a client.</summary>
/// <param name="Name">The name of the mapper.</param>
/// <param name="Protocol">The protocol of the mapper.</param>
/// <param name="ProtocolMapper">The type of the protocol mapper.</param>
/// <param name="ConsentRequired">.</param>
/// <param name="AddToIdToken">Whether to add this claim to the ID token.</param>
/// <param name="AddToAccessToken">Whether to this claim to the access token.</param>
public sealed record ClientProtocolMapper(
	string Name,
	string Protocol,
	string ProtocolMapper,
	bool ConsentRequired = false,
	bool AddToIdToken = false,
	bool AddToAccessToken = true)
{
	/// <summary>Gets the included client audience.</summary>
	public string? IncludedClientAudience { get; init; }
}
