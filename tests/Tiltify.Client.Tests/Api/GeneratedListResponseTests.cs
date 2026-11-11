using System.Net;
using System.Text;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Tiltify.Client.Generated;
using Tiltify.Client.Generated.Models;

namespace Tiltify.Client.Tests.Api;

/// <summary>
/// The generated client reads list endpoints with typed items. Tiltify's spec leaves them untyped; these tests guard
/// scripts/patch-openapi.cs, which types them before generation.
/// </summary>
[TestClass]
public sealed class GeneratedListResponseTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Polls_ReadOptionsAndAmounts()
    {
        using StubHandler handler = new(
            """
            { "data": [ { "id": "6d9b0b0e-6a0f-4ad3-9a1f-0e9c1f2e2a11", "name": "by Country", "active": true,
              "amount_raised": { "value": "150.00", "currency": "USD" },
              "options": [ { "id": "3f2c4a61-4f1c-4d3c-8a51-4b6f0f6d2c01", "name": "Japan", "amount_raised": { "value": "100.00", "currency": "USD" } } ] } ],
              "metadata": { "after": null, "before": null, "limit": 10 } }
            """
        );

        var page = await Client(handler)
            .Api.Public.Campaigns["camp-1"]
            .Polls.GetAsync(cancellationToken: TestContext.CancellationToken);

        Poll poll = page!.Data!.Single();
        Assert.AreEqual("by Country", poll.Name);
        Assert.AreEqual("Japan", poll.Options!.Single().Name);
        Assert.AreEqual("100.00", poll.Options!.Single().AmountRaised!.Value);
        Assert.AreEqual(10, page.Metadata!.Limit);
    }

    [TestMethod]
    public async Task Targets_ReadAmountRaised()
    {
        using StubHandler handler = new(
            """
            { "data": [ { "id": "1b8e3f0a-2c4d-4e5f-8a9b-0c1d2e3f4a5b", "name": "Reach Osaka", "active": true,
              "amount": { "value": "1000.00", "currency": "USD" }, "amount_raised": { "value": "400.00", "currency": "USD" } } ] }
            """
        );

        var page = await Client(handler)
            .Api.Public.Campaigns["camp-1"]
            .Targets.GetAsync(cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual("400.00", page!.Data!.Single().AmountRaised!.Value);
    }

    private static TiltifyApiClient Client(StubHandler handler)
    {
#pragma warning disable CA2000 // The adapter takes the client; the test disposes the handler.
        HttpClientRequestAdapter adapter = new(
            new AnonymousAuthenticationProvider(),
            httpClient: new HttpClient(handler, disposeHandler: false)
        )
        {
            BaseUrl = "https://api.test",
        };
#pragma warning restore CA2000
        return new TiltifyApiClient(adapter);
    }

    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            );
    }
}
