using System.Security.Cryptography;
using System.Text;
using Tiltify.Client.Internal;

namespace Tiltify.Client.Webhooks;

/// <summary>
/// Verifies Tiltify v5 webhook signatures.
/// </summary>
/// <remarks>
/// Tiltify signs webhook deliveries using HMAC-SHA256. The signed string is
/// <c>{X-Tiltify-Timestamp}.{rawBody}</c> and the digest is Base64-encoded.
/// The signature is transmitted in the <c>X-Tiltify-Signature</c> header.
/// </remarks>
public sealed class TiltifyWebhookSignatureVerifier
{
    /// <summary>The header containing the delivery timestamp.</summary>
    public const string TimestampHeaderName = "X-Tiltify-Timestamp";

    /// <summary>The header containing the Base64-encoded HMAC-SHA256 signature.</summary>
    public const string SignatureHeaderName = "X-Tiltify-Signature";

    /// <summary>The header containing the webhook API version (e.g. <c>v5</c>).</summary>
    public const string VersionHeaderName = "X-Tiltify-Version";

    /// <summary>The header containing the webhook endpoint identifier.</summary>
    public const string EndpointHeaderName = "X-Tiltify-Endpoint";

    /// <summary>
    /// Verifies the Tiltify webhook signature.
    /// </summary>
    /// <param name="body">The raw request body bytes.</param>
    /// <param name="timestamp">The value of the <c>X-Tiltify-Timestamp</c> header.</param>
    /// <param name="providedSignature">The value of the <c>X-Tiltify-Signature</c> header.</param>
    /// <param name="signingSecret">The webhook signing secret configured in the Tiltify dashboard.</param>
    /// <returns>
    /// <see langword="true"/> when the signature matches the computed digest; otherwise <see langword="false"/>.
    /// </returns>
    public bool Verify(byte[] body, string? timestamp, string? providedSignature, string signingSecret)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentException.ThrowIfNullOrEmpty(signingSecret);

        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(providedSignature))
        {
            return false;
        }

        // Signed string: "{timestamp}.{rawBody}"
        byte[] timestampBytes = Encoding.UTF8.GetBytes(timestamp + ".");
        byte[] signedBytes = new byte[timestampBytes.Length + body.Length];
        timestampBytes.CopyTo(signedBytes, 0);
        body.CopyTo(signedBytes, timestampBytes.Length);

        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(signingSecret));
        byte[] digest = hmac.ComputeHash(signedBytes);
        string expectedSignature = Convert.ToBase64String(digest);

        return ConstantTimeStringComparer.Equals(expectedSignature, providedSignature);
    }
}
