namespace Tiltify.Client.Events;

/// <summary>
/// Tiltify webhook event types: <c>{scope}:{locality}:{event}</c>. <c>public</c> carries public data, <c>private</c> adds
/// sensitive donation data such as the donor's email. <c>direct</c> is the subscribed fact itself, <c>indirect</c> its
/// children (the campaigns supporting a team campaign, the facts supporting a fundraising event).
/// </summary>
public static class TiltifyWebhookEventNames
{
    /// <summary>A donation on the subscribed fact was made, moderated or refunded.</summary>
    public const string DirectDonationUpdated = "public:direct:donation_updated";

    /// <summary>A donation on a child of the subscribed fact.</summary>
    public const string IndirectDonationUpdated = "public:indirect:donation_updated";

    /// <summary>The subscribed fact's details or amount raised changed.</summary>
    public const string DirectFactUpdated = "public:direct:fact_updated";

    /// <summary>A child fact's details or amount raised changed.</summary>
    public const string IndirectFactUpdated = "public:indirect:fact_updated";

    /// <summary>As <see cref="DirectDonationUpdated"/>, with private donation data.</summary>
    public const string PrivateDirectDonationUpdated = "private:direct:donation_updated";

    /// <summary>As <see cref="IndirectDonationUpdated"/>, with private donation data.</summary>
    public const string PrivateIndirectDonationUpdated = "private:indirect:donation_updated";

    /// <summary>As <see cref="DirectFactUpdated"/>, with private data.</summary>
    public const string PrivateDirectFactUpdated = "private:direct:fact_updated";

    /// <summary>As <see cref="IndirectFactUpdated"/>, with private data.</summary>
    public const string PrivateIndirectFactUpdated = "private:indirect:fact_updated";
}
