using Tiltify.Client.Models;

namespace Tiltify.Client.Events;

/// <summary>
/// Raised when a fact (milestone/target) is updated on a campaign.
/// Covers both <c>public:direct:fact_updated</c> and <c>public:indirect:fact_updated</c> events.
/// </summary>
public sealed record TiltifyFactWebhookEvent : TiltifyWebhookEvent
{
    /// <summary>Gets the fact/milestone data payload.</summary>
    public required TiltifyFact Data { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a direct campaign fact (<see langword="true"/>)
    /// or an indirect/sub-campaign fact (<see langword="false"/>).
    /// </summary>
    public bool IsDirect { get; init; }
}
