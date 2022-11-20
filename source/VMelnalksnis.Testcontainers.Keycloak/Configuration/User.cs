// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

namespace VMelnalksnis.Testcontainers.Keycloak.Configuration;

/// <summary>A keycloak user.</summary>
/// <param name="Username">The username of the user.</param>
/// <param name="Password">The password of the user.</param>
public record User(string Username, string Password)
{
	/// <summary>Gets the email of the user.</summary>
	public string? Email { get; init; }

	/// <summary>Gets a value indicating whether the users' email is verified.</summary>
	public bool? EmailVerified { get; init; }

	/// <summary>Gets the first name of the user.</summary>
	public string? FirstName { get; init; }

	/// <summary>Gets the last name of the user.</summary>
	public string? LastName { get; init; }
}
