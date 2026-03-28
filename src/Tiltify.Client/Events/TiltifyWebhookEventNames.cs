namespace Tiltify.Client.Events;

/// <summary>
/// Contains constants for known Tiltify v5 webhook event names.
/// </summary>
public static class TiltifyWebhookEventNames
{
    /// <summary>Donation updated on a campaign you are directly subscribed to.</summary>
    public const string DirectDonationUpdated = "public:direct:donation_updated";

    /// <summary>Donation updated on a sub-campaign or team campaign you follow indirectly.</summary>
    public const string IndirectDonationUpdated = "public:indirect:donation_updated";

    /// <summary>Fact (milestone/target) updated on a campaign you are directly subscribed to.</summary>
    public const string DirectFactUpdated = "public:direct:fact_updated";

    /// <summary>Fact (milestone/target) updated on a sub-campaign or team campaign you follow indirectly.</summary>
    public const string IndirectFactUpdated = "public:indirect:fact_updated";
}
