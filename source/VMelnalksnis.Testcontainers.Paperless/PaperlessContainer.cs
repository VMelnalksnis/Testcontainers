// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Logging;

namespace VMelnalksnis.Testcontainers.Paperless;

/// <inheritdoc />
public sealed class PaperlessContainer : DockerContainer
{
	private readonly PaperlessConfiguration _configuration;

	/// <summary>Initializes a new instance of the <see cref="PaperlessContainer"/> class.</summary>
	/// <param name="configuration">The container configuration.</param>
	/// <param name="logger">The logger.</param>
	public PaperlessContainer(PaperlessConfiguration configuration, ILogger logger)
		: base(configuration, logger)
	{
		_configuration = configuration;
	}

	/// <summary>Gets the base address of the paperless instance.</summary>
	/// <returns>The base address of the paperless instance.</returns>
	public Uri GetBaseAddress() => new($"http://localhost:{GetMappedPublicPort(PaperlessBuilder.PaperlessPort)}/");

	/// <summary>Creates a new API token for the default admin user.</summary>
	/// <returns>A new API token for the default admin user.</returns>
	/// <exception cref="HttpRequestException">The request to create the token failed.</exception>
	/// <exception cref="InvalidDataException">The request was successful, but did not contain a token.</exception>
	public async Task<string> GetAdminToken()
	{
		var httpClient = new HttpClient();
		httpClient.BaseAddress = GetBaseAddress();

		var loginContent = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>
		{
			new("username", _configuration.Username),
			new("password", _configuration.Password),
		});

		var response = await httpClient.PostAsync("/api/token/", loginContent).ConfigureAwait(false);
		var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		if (response.StatusCode is not HttpStatusCode.OK)
		{
#if NET5_0_OR_GREATER
			throw new HttpRequestException(content, null, response.StatusCode);
#else
			throw new HttpRequestException(content);
#endif
		}

		var responseJson = JsonNode.Parse(content);
		return responseJson?["token"]?.GetValue<string>() ??
			throw new InvalidDataException("Response did not contain a token");
	}
}
