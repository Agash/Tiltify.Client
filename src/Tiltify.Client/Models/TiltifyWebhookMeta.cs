using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents the metadata envelope present on every Tiltify v5 webhook delivery.
/// </summary>
public sealed record TiltifyWebhookMeta
{
    /// <summary>Gets the unique delivery identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the event name (e.g. <c>public:direct:donation_updated</c>).</summary>
    [JsonPropertyName("event_name")]
    public string EventName { get; init; } = string.Empty;

    /// <summary>Gets the ISO 8601 timestamp when the event was generated.</summary>
    [JsonPropertyName("generated_at")]
    public string GeneratedAt { get; init; } = string.Empty;

    /// <summary>
    /// Gets the campaign or cause ID that is the source of the subscription that triggered this event.
    /// </summary>
    [JsonPropertyName("subscription_source_id")]
    public string? SubscriptionSourceId { get; init; }

    /// <summary>
    /// Gets the webhook endpoint ID that received this delivery.
    /// </summary>
    [JsonPropertyName("subscription_target_id")]
    public string? SubscriptionTargetId { get; init; }

    /// <summary>Gets the attempt number for this delivery (1-based).</summary>
    [JsonPropertyName("attempt_number")]
    public int AttemptNumber { get; init; }
}
