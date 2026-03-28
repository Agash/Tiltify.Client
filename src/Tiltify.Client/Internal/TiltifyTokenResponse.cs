using System.Text.Json.Serialization;

namespace Tiltify.Client.Internal;

/// <summary>
/// Represents the OAuth token response from the Tiltify token endpoint.
/// </summary>
internal sealed class TiltifyTokenResponse
{
    /// <summary>Gets the access token.</summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Gets the token type (typically "Bearer").</summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    /// <summary>Gets the number of seconds until the access token expires.</summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>Gets the optional refresh token.</summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>Gets the granted scopes.</summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    /// <summary>Gets the creation timestamp.</summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; init; }
}
