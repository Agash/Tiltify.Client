namespace Tiltify.Client.Options;

/// <summary>Where and how <see cref="TiltifyClientFactory"/> reaches Tiltify. Credentials are per client.</summary>
public sealed class TiltifyClientOptions
{
    /// <summary>The API base URL. Defaults to <c>https://v5api.tiltify.com</c>.</summary>
    public string BaseUrl { get; set; } = "https://v5api.tiltify.com";

    /// <summary>The OAuth token endpoint. Defaults to <c>https://v5api.tiltify.com/oauth/token</c>.</summary>
    public string TokenEndpoint { get; set; } = "https://v5api.tiltify.com/oauth/token";

    /// <summary>The OAuth scopes to request. Defaults to <c>public</c>.</summary>
    public string Scope { get; set; } = "public";

    /// <summary>How long before expiry a token is refreshed. Defaults to one minute.</summary>
    public TimeSpan TokenRefreshBuffer { get; set; } = TimeSpan.FromMinutes(1);
}
