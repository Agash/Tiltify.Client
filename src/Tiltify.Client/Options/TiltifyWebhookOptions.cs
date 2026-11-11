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

    /// <summary>
    /// How far the <c>X-Tiltify-Timestamp</c> may be from now before a delivery is refused as a replay. Tiltify's
    /// docs say within the last minute; the default allows for clock skew and slow delivery. Null turns the check off.
    /// </summary>
    public TimeSpan? MaxTimestampAge { get; set; } = TimeSpan.FromMinutes(5);
}
