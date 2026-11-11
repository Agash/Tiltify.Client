using System.Text;

namespace Tiltify.Client.Authentication;

/// <summary>
/// A Tiltify application's OAuth client credentials. One application can read any public campaign; an app serving
/// several accounts passes each account's credentials to <see cref="Abstractions.ITiltifyClientFactory.CreateApiClient"/>.
/// </summary>
/// <param name="ClientId">The application's client ID.</param>
/// <param name="ClientSecret">The application's client secret; never printed by <see cref="ToString"/>.</param>
public sealed record TiltifyCredentials(string ClientId, string ClientSecret)
{
    private bool PrintMembers(StringBuilder builder)
    {
        _ = builder.Append("ClientId = ").Append(ClientId);
        return true;
    }
}
