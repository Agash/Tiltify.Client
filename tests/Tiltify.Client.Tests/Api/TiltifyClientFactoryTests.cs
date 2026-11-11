using System.Net;
using System.Text;
using Tiltify.Client.Authentication;
using Tiltify.Client.Options;

namespace Tiltify.Client.Tests.Api;

[TestClass]
public sealed class TiltifyClientFactoryTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task CreateApiClient_SendsThroughTheCallersHttpClientWithAToken()
    {
        using Stub stub = new();
        using HttpClient http = new(stub, disposeHandler: false);
        using TiltifyClientFactory factory = new(
            new TiltifyTokenProvider(
                Microsoft.Extensions.Options.Options.Create(Options()),
                http,
                new ManualTime()
            ),
            Options(),
            http
        );

        _ = await factory
            .CreateApiClient()
            .Api.Public.Campaigns["camp-1"]
            .Polls.GetAsync(cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual("Bearer token-1", stub.LastAuthorization);
        Assert.AreEqual("/api/public/campaigns/camp-1/polls", stub.LastApiPath);
    }

    [TestMethod]
    public async Task GetTokenAsync_RefreshesOnlyNearExpiryOnTheInjectedClock()
    {
        using Stub stub = new();
        using HttpClient http = new(stub, disposeHandler: false);
        ManualTime time = new();
        using TiltifyTokenProvider tokens = new(
            Microsoft.Extensions.Options.Options.Create(Options()),
            http,
            time
        );

        string first = await tokens.GetTokenAsync(TestContext.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(7200 - 61));
        string cached = await tokens.GetTokenAsync(TestContext.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(2));
        string refreshed = await tokens.GetTokenAsync(TestContext.CancellationToken);

        Assert.AreEqual("token-1", first);
        Assert.AreEqual("token-1", cached);
        Assert.AreEqual("token-2", refreshed);
    }

    private static TiltifyClientOptions Options() =>
        new()
        {
            ClientId = "client",
            ClientSecret = "secret",
            BaseUrl = "https://tiltify.test",
            TokenEndpoint = "https://tiltify.test/oauth/token",
        };

    private sealed class ManualTime : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class Stub : HttpMessageHandler
    {
        private int _tokens;

        public string? LastAuthorization { get; private set; }

        public string? LastApiPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (request.RequestUri!.AbsolutePath == "/oauth/token")
            {
                _tokens++;
                return Task.FromResult(
                    Json(
                        $$"""{ "access_token": "token-{{_tokens}}", "token_type": "bearer", "expires_in": 7200 }"""
                    )
                );
            }

            LastAuthorization = request.Headers.Authorization?.ToString();
            LastApiPath = request.RequestUri.AbsolutePath;
            return Task.FromResult(Json("""{ "data": [], "metadata": { "limit": 10 } }"""));
        }

        private static HttpResponseMessage Json(string body) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
    }
}
