namespace Tiltify.Client.Options;

/// <summary>
/// Configuration options for the Tiltify API client.
/// </summary>
public sealed class TiltifyClientOptions
{
    /// <summary>
    /// Gets or sets the Tiltify application client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Tiltify application client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Tiltify API base URL.
    /// </summary>
    /// <value>Defaults to <c>https://v5api.tiltify.com</c>.</value>
    public string BaseUrl { get; set; } = "https://v5api.tiltify.com";

    /// <summary>
    /// Gets or sets the token endpoint URL.
    /// </summary>
    /// <value>Defaults to <c>https://v5api.tiltify.com/oauth/token</c>.</value>
    public string TokenEndpoint { get; set; } = "https://v5api.tiltify.com/oauth/token";

    /// <summary>
    /// Gets or sets the OAuth scopes to request.
    /// </summary>
    /// <value>Defaults to <c>public</c>.</value>
    public string Scope { get; set; } = "public";

    /// <summary>
    /// Gets or sets the number of seconds before token expiry at which to proactively refresh.
    /// </summary>
    /// <value>Defaults to <c>60</c> seconds.</value>
    public int TokenRefreshBufferSeconds { get; set; } = 60;
}
