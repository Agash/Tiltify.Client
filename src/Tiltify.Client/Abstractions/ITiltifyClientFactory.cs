using Tiltify.Client.Authentication;
using Tiltify.Client.Generated;

namespace Tiltify.Client.Abstractions;

/// <summary>Creates authenticated <see cref="TiltifyApiClient"/> instances.</summary>
public interface ITiltifyClientFactory
{
    /// <summary>
    /// Creates a client that authenticates as <paramref name="credentials"/>. Clients are cheap; tokens are cached per
    /// client ID across them.
    /// </summary>
    TiltifyApiClient CreateApiClient(TiltifyCredentials credentials);
}
