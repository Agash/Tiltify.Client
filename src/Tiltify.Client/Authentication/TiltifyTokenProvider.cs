using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using Tiltify.Client.Internal;
using Tiltify.Client.Options;

namespace Tiltify.Client.Authentication;

/// <summary>
/// Manages OAuth access tokens for the Tiltify API using the client credentials grant.
/// Tokens are cached and proactively refreshed before expiry.
/// </summary>
public sealed class TiltifyTokenProvider : IDisposable
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TiltifyClientOptions _options;
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyTokenProvider"/> using a shared <see cref="HttpClient"/>.
    /// </summary>
    public TiltifyTokenProvider(IOptions<TiltifyClientOptions> options, HttpClient httpClient)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttp = false;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyTokenProvider"/> that owns its own <see cref="HttpClient"/>.
    /// </summary>
    public TiltifyTokenProvider(TiltifyClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _http = new HttpClient();
        _ownsHttp = true;
    }

    /// <summary>
    /// Returns a valid access token, fetching or refreshing as necessary.
    /// </summary>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        int bufferSeconds = _options.TokenRefreshBufferSeconds;
        if (_cachedToken is not null && DateTimeOffset.UtcNow.AddSeconds(bufferSeconds) < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow.AddSeconds(bufferSeconds) < _tokenExpiry)
                return _cachedToken;

            (_cachedToken, _tokenExpiry) = await FetchTokenAsync(cancellationToken).ConfigureAwait(false);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<(string Token, DateTimeOffset Expiry)> FetchTokenAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", _options.Scope),
        ]);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        TiltifyTokenResponse? tokenResponse = await response.Content
            .ReadFromJsonAsync<TiltifyTokenResponse>(s_jsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Tiltify token endpoint returned an empty or invalid token response.");
        }

        DateTimeOffset expiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        return (tokenResponse.AccessToken, expiry);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Dispose();
        if (_ownsHttp) _http.Dispose();
    }
}
