using Tiltify.Client.Models;

namespace Tiltify.Client.Events;

/// <summary>
/// Raised when a donation is created or updated on a campaign.
/// Covers both <c>public:direct:donation_updated</c> and <c>public:indirect:donation_updated</c> events.
/// </summary>
public sealed record TiltifyDonationWebhookEvent : TiltifyWebhookEvent
{
    /// <summary>Gets the donation data payload.</summary>
    public required TiltifyDonation Data { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a direct campaign donation (<see langword="true"/>)
    /// or an indirect/sub-campaign donation (<see langword="false"/>).
    /// </summary>
    public bool IsDirect { get; init; }
}
