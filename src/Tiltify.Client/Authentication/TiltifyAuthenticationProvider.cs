using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Tiltify.Client.Options;

namespace Tiltify.Client.Authentication;

/// <summary>
/// Provides Kiota authentication for the Tiltify API using OAuth bearer tokens and the Client-Id header.
/// </summary>
public sealed class TiltifyAuthenticationProvider : IAuthenticationProvider
{
    private readonly TiltifyTokenProvider _tokenProvider;
    private readonly string _clientId;

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyAuthenticationProvider"/>.
    /// </summary>
    /// <param name="tokenProvider">The token provider that supplies valid OAuth access tokens.</param>
    /// <param name="options">The client options supplying the client ID header value.</param>
    public TiltifyAuthenticationProvider(TiltifyTokenProvider tokenProvider, TiltifyClientOptions options)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        ArgumentNullException.ThrowIfNull(options);
        _clientId = options.ClientId;
    }

    /// <inheritdoc />
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.TryAdd("Authorization", $"Bearer {token}");
        request.Headers.TryAdd("Client-Id", _clientId);
    }
}
