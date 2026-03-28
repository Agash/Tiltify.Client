using System.Text.Json;

namespace Tiltify.Client.Events;

/// <summary>
/// Represents a Tiltify webhook event with an unrecognized or unmapped event type.
/// The raw data payload is preserved for caller-side handling.
/// </summary>
public sealed record TiltifyUnknownWebhookEvent : TiltifyWebhookEvent
{
    /// <summary>Gets the raw JSON data element from the webhook payload.</summary>
    public JsonElement RawData { get; init; }
}
