using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Authentication;
using Tiltify.Client.DependencyInjection;

namespace Tiltify.Client.Tests.Api;

/// <summary>The client factory as an app composes it: AddTiltifyClient with the HTTP client's primary handler swapped.</summary>
[TestClass]
public sealed class TiltifyClientFactoryTests
{
    private static readonly TiltifyCredentials Alpha = new("alpha", "alpha-secret");
    private static readonly TiltifyCredentials Beta = new("beta", "beta-secret");

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task CreateApiClient_AuthenticatesAsTheGivenCredentials()
    {
        using Composition app = new();

        _ = await app
            .Factory.CreateApiClient(Alpha)
            .Api.Public.Campaigns["camp-1"]
            .Polls.GetAsync(cancellationToken: TestContext.CancellationToken);
        _ = await app
            .Factory.CreateApiClient(Beta)
            .Api.Public.Campaigns["camp-1"]
            .Polls.GetAsync(cancellationToken: TestContext.CancellationToken);

        CollectionAssert.AreEqual(
            new[] { "Bearer alpha-1 alpha", "Bearer beta-1 beta" },
            app.Tiltify.ApiCalls
        );
    }

    [TestMethod]
    public async Task CreateApiClient_ReusesATokenUntilItNearsExpiry()
    {
        using Composition app = new();

        await app.ReadPollsAsync(Alpha, TestContext.CancellationToken);
        app.Time.Advance(TimeSpan.FromSeconds(7200 - 61));
        await app.ReadPollsAsync(Alpha, TestContext.CancellationToken);
        app.Time.Advance(TimeSpan.FromSeconds(2));
        await app.ReadPollsAsync(Alpha, TestContext.CancellationToken);

        CollectionAssert.AreEqual(
            new[] { "Bearer alpha-1 alpha", "Bearer alpha-1 alpha", "Bearer alpha-2 alpha" },
            app.Tiltify.ApiCalls
        );
    }

    [TestMethod]
    public async Task CreateApiClient_ChangedSecretFetchesANewToken()
    {
        using Composition app = new();

        await app.ReadPollsAsync(Alpha, TestContext.CancellationToken);
        await app.ReadPollsAsync(
            Alpha with
            {
                ClientSecret = "rotated",
            },
            TestContext.CancellationToken
        );

        CollectionAssert.AreEqual(
            new[] { "Bearer alpha-1 alpha", "Bearer alpha-2 alpha" },
            app.Tiltify.ApiCalls
        );
    }

    [TestMethod]
    public void Credentials_ToStringOmitsTheSecret()
    {
        Assert.DoesNotContain("alpha-secret", Alpha.ToString(), StringComparison.Ordinal);
    }

    private sealed class Composition : IDisposable
    {
        private readonly ServiceProvider _services;

        public Composition()
        {
            ServiceCollection services = new();
            _ = services.AddSingleton<TimeProvider>(Time);
            _ = services.AddTiltifyClient(static options =>
            {
                options.BaseUrl = "https://tiltify.test";
                options.TokenEndpoint = "https://tiltify.test/oauth/token";
            });
            _ = services
                .AddHttpClient(TiltifyClientFactory.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => Tiltify);
            _services = services.BuildServiceProvider();
            Factory = _services.GetRequiredService<ITiltifyClientFactory>();
        }

        public ManualTime Time { get; } = new();

        public StubTiltify Tiltify { get; } = new();

        public ITiltifyClientFactory Factory { get; }

        public async Task ReadPollsAsync(TiltifyCredentials credentials, CancellationToken ct) =>
            _ = await Factory
                .CreateApiClient(credentials)
                .Api.Public.Campaigns["camp-1"]
                .Polls.GetAsync(cancellationToken: ct);

        public void Dispose() => _services.Dispose();
    }

    private sealed class ManualTime : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    /// <summary>Issues numbered tokens per client ID and records each API call's token and Client-Id.</summary>
    private sealed class StubTiltify : HttpMessageHandler
    {
        private readonly Dictionary<string, int> _issued = new(StringComparer.Ordinal);

        public List<string> ApiCalls { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (request.RequestUri!.AbsolutePath == "/oauth/token")
            {
                string form = await request.Content!.ReadAsStringAsync(cancellationToken);
                string clientId = form.Split('&')
                    .Single(static pair => pair.StartsWith("client_id=", StringComparison.Ordinal))[
                    "client_id=".Length..
                ];
                int number = _issued[clientId] = _issued.GetValueOrDefault(clientId) + 1;
                return Json(
                    $$"""{ "access_token": "{{clientId}}-{{number}}", "token_type": "bearer", "expires_in": 7200 }"""
                );
            }

            ApiCalls.Add(
                $"{request.Headers.Authorization} {request.Headers.GetValues("Client-Id").Single()}"
            );
            return Json("""{ "data": [], "metadata": { "limit": 10 } }""");
        }

        private static HttpResponseMessage Json(string body) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
    }
}
