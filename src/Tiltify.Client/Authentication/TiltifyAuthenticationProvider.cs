using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Tiltify.Client.Authentication;

/// <summary>Signs each API request with the credentials' bearer token and Client-Id header.</summary>
internal sealed class TiltifyAuthenticationProvider(
    TiltifyTokenCache tokens,
    TiltifyCredentials credentials
) : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        string token = await tokens
            .GetTokenAsync(credentials, cancellationToken)
            .ConfigureAwait(false);
        _ = request.Headers.TryAdd("Authorization", $"Bearer {token}");
        _ = request.Headers.TryAdd("Client-Id", credentials.ClientId);
    }
}
