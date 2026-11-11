using System.Collections.Concurrent;
using System.Net.Http.Json;
using Tiltify.Client.Internal;
using Tiltify.Client.Options;
using Tiltify.Client.Serialization;

namespace Tiltify.Client.Authentication;

/// <summary>
/// Client-credentials access tokens, one per client ID, fetched on first use and refreshed shortly before they expire.
/// A changed secret for a client ID replaces its token.
/// </summary>
internal sealed class TiltifyTokenCache : IDisposable
{
    private readonly ConcurrentDictionary<string, Slot> _slots = new(StringComparer.Ordinal);
    private readonly IHttpClientFactory _httpClients;
    private readonly TiltifyClientOptions _options;
    private readonly TimeProvider _time;

    public TiltifyTokenCache(
        IHttpClientFactory httpClients,
        TiltifyClientOptions options,
        TimeProvider time
    )
    {
        _httpClients = httpClients;
        _options = options;
        _time = time;
    }

    public async Task<string> GetTokenAsync(
        TiltifyCredentials credentials,
        CancellationToken cancellationToken
    )
    {
        Slot slot = _slots.AddOrUpdate(
            credentials.ClientId,
            static (_, secret) => new Slot(secret),
            static (_, existing, secret) => existing.Secret == secret ? existing : new Slot(secret),
            credentials.ClientSecret
        );

        if (slot.TryGetFresh(_time.GetUtcNow() + _options.TokenRefreshBuffer) is { } cached)
        {
            return cached;
        }

        await slot.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (slot.TryGetFresh(_time.GetUtcNow() + _options.TokenRefreshBuffer) is { } refreshed)
            {
                return refreshed;
            }

            TiltifyTokenResponse token = await FetchAsync(credentials, cancellationToken)
                .ConfigureAwait(false);
            slot.Set(token.AccessToken, _time.GetUtcNow().AddSeconds(token.ExpiresIn));
            return token.AccessToken;
        }
        finally
        {
            _ = slot.Gate.Release();
        }
    }

    public void Dispose()
    {
        foreach (Slot slot in _slots.Values)
        {
            slot.Gate.Dispose();
        }
    }

    private async Task<TiltifyTokenResponse> FetchAsync(
        TiltifyCredentials credentials,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = new(HttpMethod.Post, _options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("client_id", credentials.ClientId),
                new KeyValuePair<string, string>("client_secret", credentials.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", _options.Scope),
            ]),
        };

        HttpClient http = _httpClients.CreateClient(TiltifyClientFactory.HttpClientName);
        using HttpResponseMessage response = await http.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        TiltifyTokenResponse? token = await response
            .Content.ReadFromJsonAsync(
                TiltifyJsonContext.Default.TiltifyTokenResponse,
                cancellationToken
            )
            .ConfigureAwait(false);
        return token is { AccessToken.Length: > 0 }
            ? token
            : throw new InvalidOperationException(
                "Tiltify's token endpoint returned no access token."
            );
    }

    private sealed class Slot(string secret)
    {
        private AccessToken? _token;

        public string Secret { get; } = secret;

        public SemaphoreSlim Gate { get; } = new(1, 1);

        public string? TryGetFresh(DateTimeOffset validUntil) =>
            Volatile.Read(ref _token) is { } token && validUntil < token.Expiry
                ? token.Value
                : null;

        public void Set(string token, DateTimeOffset expiry) =>
            Volatile.Write(ref _token, new AccessToken(token, expiry));

        private sealed record AccessToken(string Value, DateTimeOffset Expiry);
    }
}
