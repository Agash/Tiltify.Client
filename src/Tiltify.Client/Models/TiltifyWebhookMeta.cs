using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>The <c>meta</c> block of a Tiltify webhook delivery, as Tiltify's webhook documentation shows it.</summary>
public sealed record TiltifyWebhookMeta
{
    /// <summary>The delivery id; the same id on a retried delivery.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The event type, <c>{scope}:{locality}:{event}</c> (e.g. <c>public:direct:donation_updated</c>).</summary>
    [JsonPropertyName("event_type")]
    public string EventType { get; init; } = string.Empty;

    /// <summary>When Tiltify generated the event.</summary>
    [JsonPropertyName("generated_at")]
    public string GeneratedAt { get; init; } = string.Empty;

    /// <summary>When this delivery was attempted.</summary>
    [JsonPropertyName("attempted_at")]
    public string? AttemptedAt { get; init; }

    /// <summary>The id of what the subscription is on (the fact).</summary>
    [JsonPropertyName("subscription_source_id")]
    public string? SubscriptionSourceId { get; init; }

    /// <summary>What kind of thing the subscription is on; <c>test</c> for a test delivery.</summary>
    [JsonPropertyName("subscription_source_type")]
    public string? SubscriptionSourceType { get; init; }
}
