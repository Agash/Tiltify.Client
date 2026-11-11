using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Authentication;
using Tiltify.Client.Generated;
using Tiltify.Client.Options;

namespace Tiltify.Client;

/// <summary>
/// Creates <see cref="TiltifyApiClient"/> instances for any number of credential sets. Every request, token and API,
/// goes through the <see cref="HttpClientName"/> client from <see cref="IHttpClientFactory"/>, which
/// <c>AddTiltifyClient</c> configures with Kiota's middleware.
/// </summary>
public sealed class TiltifyClientFactory : ITiltifyClientFactory, IDisposable
{
    /// <summary>The name of the <see cref="HttpClient"/> every Tiltify request goes through.</summary>
    public const string HttpClientName = "Tiltify";

    private readonly IHttpClientFactory _httpClients;
    private readonly TiltifyClientOptions _options;
    private readonly TiltifyTokenCache _tokens;

    /// <summary>Initializes a new instance of <see cref="TiltifyClientFactory"/>.</summary>
    public TiltifyClientFactory(
        IHttpClientFactory httpClients,
        IOptions<TiltifyClientOptions> options,
        TimeProvider timeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(httpClients);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _httpClients = httpClients;
        _options = options.Value;
        _tokens = new TiltifyTokenCache(httpClients, _options, timeProvider);
    }

    /// <inheritdoc />
    public TiltifyApiClient CreateApiClient(TiltifyCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentials.ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentials.ClientSecret);

        HttpClientRequestAdapter adapter = new(
            new TiltifyAuthenticationProvider(_tokens, credentials),
            httpClient: _httpClients.CreateClient(HttpClientName)
        )
        {
            BaseUrl = _options.BaseUrl,
        };
        return new TiltifyApiClient(adapter);
    }

    /// <inheritdoc />
    public void Dispose() => _tokens.Dispose();
}
