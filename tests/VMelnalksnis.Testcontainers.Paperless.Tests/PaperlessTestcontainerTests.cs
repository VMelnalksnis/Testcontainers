// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace VMelnalksnis.Testcontainers.Paperless.Tests;

[Collection("Paperless")]
public sealed class PaperlessTestcontainerTests
{
	private readonly PaperlessContainer _paperless;

	public PaperlessTestcontainerTests(PaperlessFixture paperlessFixture)
	{
		_paperless = paperlessFixture.Paperless;
	}

	[Fact]
	public async Task LoginPageShouldWork()
	{
		var token = await _paperless.GetAdminToken();
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = _paperless.GetBaseAddress();
		httpClient.DefaultRequestHeaders.Authorization = new("Token", token);
		var response = await httpClient.GetAsync("/accounts/login/?next=/");

		using (new AssertionScope())
		{
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypeNames.Text.Html);
			response.Content.Headers.ContentType?.CharSet.Should().Be("utf-8");
		}
	}
}
