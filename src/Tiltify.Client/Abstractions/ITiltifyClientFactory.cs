using Tiltify.Client.Generated;

namespace Tiltify.Client.Abstractions;

/// <summary>
/// Creates authenticated <see cref="TiltifyApiClient"/> instances.
/// </summary>
public interface ITiltifyClientFactory
{
    /// <summary>
    /// Creates a new <see cref="TiltifyApiClient"/> using the configured OAuth credentials.
    /// </summary>
    TiltifyApiClient CreateApiClient();
}
