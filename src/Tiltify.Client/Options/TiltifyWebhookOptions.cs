namespace Tiltify.Client.Options;

/// <summary>
/// Configuration options for Tiltify webhook handling.
/// </summary>
public sealed class TiltifyWebhookOptions
{
    /// <summary>
    /// Gets or sets the webhook signing secret configured in the Tiltify dashboard.
    /// </summary>
    /// <remarks>
    /// Used to verify the <c>X-Tiltify-Signature</c> header on inbound webhook deliveries.
    /// The signed string is <c>{X-Tiltify-Timestamp}.{rawBody}</c>, HMAC-SHA256, Base64-encoded.
    /// </remarks>
    public string SigningSecret { get; set; } = string.Empty;
}
