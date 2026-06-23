using Microsoft.Kiota.Http.HttpClientLibrary;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Authentication;
using Tiltify.Client.Generated;
using Tiltify.Client.Options;

namespace Tiltify.Client;

/// <summary>
/// Default implementation of <see cref="ITiltifyClientFactory"/>.
/// Creates <see cref="TiltifyApiClient"/> instances backed by an auto-refreshing token provider.
/// </summary>
public sealed class TiltifyClientFactory : ITiltifyClientFactory, IDisposable
{
    private readonly TiltifyTokenProvider _tokenProvider;
    private readonly TiltifyClientOptions _options;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyClientFactory"/>.
    /// </summary>
    public TiltifyClientFactory(TiltifyTokenProvider tokenProvider, TiltifyClientOptions options)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public TiltifyApiClient CreateApiClient()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        TiltifyAuthenticationProvider authProvider = new(_tokenProvider, _options);
        HttpClientRequestAdapter adapter = new(authProvider)
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
    }
}
