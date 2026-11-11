using Microsoft.Kiota.Http.HttpClientLibrary;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Authentication;
using Tiltify.Client.Generated;
using Tiltify.Client.Options;

namespace Tiltify.Client;

/// <summary>
/// Default implementation of <see cref="ITiltifyClientFactory"/>.
/// Creates <see cref="TiltifyApiClient"/> instances backed by an auto-refreshing token provider. Every client shares
/// one <see cref="HttpClient"/>: the one passed in, or one with Kiota's default middleware that the factory owns.
/// </summary>
public sealed class TiltifyClientFactory : ITiltifyClientFactory, IDisposable
{
    private readonly TiltifyTokenProvider _tokenProvider;
    private readonly TiltifyClientOptions _options;
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyClientFactory"/>.
    /// </summary>
    /// <param name="tokenProvider">The token provider; the factory owns and disposes it.</param>
    /// <param name="options">The client options.</param>
    /// <param name="httpClient">
    /// The client every API call goes through, owned by the caller; it should carry Kiota's middleware
    /// (<see cref="KiotaClientFactory.CreateDefaultHandlers"/>). Null creates one the factory owns.
    /// </param>
    public TiltifyClientFactory(
        TiltifyTokenProvider tokenProvider,
        TiltifyClientOptions options,
        HttpClient? httpClient = null
    )
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _ownsHttp = httpClient is null;
        _http = httpClient ?? KiotaClientFactory.Create();
    }

    /// <inheritdoc />
    public TiltifyApiClient CreateApiClient()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        TiltifyAuthenticationProvider authProvider = new(_tokenProvider, _options);
        HttpClientRequestAdapter adapter = new(authProvider, httpClient: _http)
        {
            BaseUrl = _options.BaseUrl,
        };
        return new TiltifyApiClient(adapter);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _tokenProvider.Dispose();
        if (_ownsHttp)
        {
            _http.Dispose();
        }
    }
}
