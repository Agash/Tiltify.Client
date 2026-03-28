using System.Text.Json;
using System.Text.Json.Serialization;
using Tiltify.Client.Models;

namespace Tiltify.Client.Internal;

/// <summary>
/// Represents the raw Tiltify v5 webhook payload envelope before type-specific deserialization.
/// </summary>
internal sealed class TiltifyWebhookEnvelope
{
    /// <summary>Gets the webhook metadata.</summary>
    [JsonPropertyName("meta")]
    public TiltifyWebhookMeta Meta { get; init; } = new();

    /// <summary>Gets the raw data element for type-specific deserialization.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}
