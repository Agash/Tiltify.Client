using System.Net;
using System.Text;
using Tiltify.Client.Authentication;
using Tiltify.Client.Models;
using Tiltify.Client.Options;

namespace Tiltify.Client.Tests.Api;

[TestClass]
public sealed class TiltifyCampaignApiTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task GetPollsAsync_ReadsOptionsAndAmounts()
    {
        using StubHandler handler = new(path =>
            path.EndsWith("/polls?limit=100", StringComparison.Ordinal)
                ? """
                    { "data": [ { "id": "poll-1", "name": "by Country", "active": true,
                      "amount_raised": { "value": "150.00", "currency": "USD" },
                      "options": [ { "id": "opt-1", "name": "Japan", "amount_raised": { "value": "100.00", "currency": "USD" } },
                                   { "id": "opt-2", "name": "Canada", "amount_raised": { "value": "50.00", "currency": "USD" } } ] } ],
                      "metadata": { } }
                    """
                : null
        );
        TiltifyCampaignApi api = Create(handler);

        IReadOnlyList<TiltifyPoll> polls = await api.GetPollsAsync(
            "camp-1",
            TestContext.CancellationToken
        );

        Assert.HasCount(1, polls);
        Assert.AreEqual("by Country", polls[0].Name);
        Assert.AreEqual("100.00", polls[0].Options![0].AmountRaised!.Value);
        Assert.AreEqual("Bearer token-1", handler.LastAuthorization);
    }

    [TestMethod]
    public async Task GetCampaignAsync_Missing_ReturnsNull()
    {
        using StubHandler handler = new(_ => null);

        Assert.IsNull(
            await Create(handler).GetCampaignAsync("nope", TestContext.CancellationToken)
        );
    }

    [TestMethod]
    public async Task GetRewardsAndTargets_ReadTheirFields()
    {
        using StubHandler handler = new(path =>
            path.Contains("/rewards", StringComparison.Ordinal)
                ? """{ "data": [ { "id": "r1", "name": "Shout-out", "amount": { "value": "25.00", "currency": "USD" }, "quantity_remaining": 3 } ] }"""
            : path.Contains("/targets", StringComparison.Ordinal)
                ? """{ "data": [ { "id": "t1", "name": "Reach Osaka", "amount": { "value": "1000.00", "currency": "USD" }, "amount_raised": { "value": "400.00", "currency": "USD" } } ] }"""
            : null
        );
        TiltifyCampaignApi api = Create(handler);

        IReadOnlyList<TiltifyReward> rewards = await api.GetRewardsAsync(
            "camp-1",
            TestContext.CancellationToken
        );
        IReadOnlyList<TiltifyTarget> targets = await api.GetTargetsAsync(
            "camp-1",
            TestContext.CancellationToken
        );

        Assert.AreEqual(3, rewards[0].QuantityRemaining);
        Assert.AreEqual("400.00", targets[0].AmountRaised!.Value);
    }

    private static TiltifyCampaignApi Create(StubHandler handler)
    {
        TiltifyClientOptions options = new()
        {
            ClientId = "id",
            ClientSecret = "secret",
            BaseUrl = "https://api.test",
            TokenEndpoint = "https://api.test/oauth/token",
        };
#pragma warning disable CA2000 // The token provider and client share the handler, disposed by the test.
        HttpClient http = new(handler, disposeHandler: false);
        return new TiltifyCampaignApi(
            new TiltifyTokenProvider(Microsoft.Extensions.Options.Options.Create(options), http),
            options,
            http
        );
#pragma warning restore CA2000
    }

    // Answers the token request, then each API path with the body the test returns for it (404 when null).
    private sealed class StubHandler(Func<string, string?> respond) : HttpMessageHandler
    {
        public string? LastAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            string path = request.RequestUri!.PathAndQuery;
            if (path.StartsWith("/oauth/token", StringComparison.Ordinal))
            {
                return Task.FromResult(
                    Json(
                        """{ "access_token": "token-1", "token_type": "bearer", "expires_in": 7200 }"""
                    )
                );
            }

            LastAuthorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(
                respond(path) is { } body
                    ? Json(body)
                    : new HttpResponseMessage(HttpStatusCode.NotFound)
            );
        }

        private static HttpResponseMessage Json(string body) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
    }
}
