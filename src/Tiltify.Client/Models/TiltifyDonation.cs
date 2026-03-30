using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a Tiltify donation record delivered via webhook (v5 API).
/// </summary>
public sealed record TiltifyDonation
{
    /// <summary>Gets the donation ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the donor-visible name.</summary>
    [JsonPropertyName("donor_name")]
    public string? DonorName { get; init; }

    /// <summary>Gets the public donor comment.</summary>
    [JsonPropertyName("donor_comment")]
    public string? DonorComment { get; init; }

    /// <summary>Gets the donation amount.</summary>
    [JsonPropertyName("amount")]
    public TiltifyAmount? Amount { get; init; }

    /// <summary>
    /// Gets the reward claims associated with this donation.
    /// Tiltify v5 replaces the single <see cref="RewardId"/> field with a list of reward claims.
    /// </summary>
    [JsonPropertyName("reward_claims")]
    public IReadOnlyList<TiltifyRewardClaim>? RewardClaims { get; init; }

    /// <summary>
    /// Gets the single reward selected by the donor, if any.
    /// </summary>
    /// <remarks>
    /// Deprecated by Tiltify in favour of <see cref="RewardClaims"/>.
    /// This field may still appear in some webhook payloads for backward compatibility.
    /// </remarks>
    [Obsolete("Use RewardClaims instead. Tiltify deprecated single reward_id in favour of reward_claims in the v5 API.")]
    [JsonPropertyName("reward_id")]
    public string? RewardId { get; init; }

    /// <summary>Gets the poll ID the donor voted for, if any.</summary>
    [JsonPropertyName("poll_id")]
    public string? PollId { get; init; }

    /// <summary>Gets the poll option ID selected by the donor, if any.</summary>
    [JsonPropertyName("poll_option_id")]
    public string? PollOptionId { get; init; }

    /// <summary>Gets the target ID associated with this donation, if any.</summary>
    [JsonPropertyName("target_id")]
    public string? TargetId { get; init; }

    /// <summary>Gets the team event ID associated with this donation, if applicable.</summary>
    [JsonPropertyName("team_event_id")]
    public string? TeamEventId { get; init; }

    /// <summary>Gets the name of the fundraising campaign associated with this donation.</summary>
    [JsonPropertyName("campaign_id")]
    public string? CampaignId { get; init; }

    /// <summary>Gets the cause ID associated with this donation.</summary>
    [JsonPropertyName("cause_id")]
    public string? CauseId { get; init; }

    /// <summary>Gets the fundraising event ID, if applicable.</summary>
    [JsonPropertyName("fundraising_event_id")]
    public string? FundraisingEventId { get; init; }

    /// <summary>
    /// Gets whether this donation is a sustained (recurring) donation.
    /// </summary>
    [JsonPropertyName("sustained")]
    public bool? Sustained { get; init; }

    /// <summary>
    /// Gets the legacy numeric donation ID from Tiltify v3/v4, if available.
    /// Useful for correlating with historical data.
    /// </summary>
    [JsonPropertyName("legacy_id")]
    public long? LegacyId { get; init; }

    /// <summary>
    /// Gets the donation matches triggered by this donation, if any.
    /// Donation matching multiplies the impact of a donor's contribution.
    /// </summary>
    [JsonPropertyName("donation_matches")]
    public IReadOnlyList<TiltifyDonationMatch>? DonationMatches { get; init; }

    /// <summary>Gets the timestamp when the donation was completed.</summary>
    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>Gets the timestamp when the donation was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Gets whether the donation was made anonymously.</summary>
    [JsonPropertyName("donor_anonymous")]
    public bool? DonorAnonymous { get; init; }

    /// <summary>Gets whether the donation is test data.</summary>
    [JsonPropertyName("test")]
    public bool? Test { get; init; }
}
