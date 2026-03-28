using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a Tiltify donation record delivered via webhook.
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

    /// <summary>Gets the fundraiser reward selected by the donor, if any.</summary>
    [JsonPropertyName("reward_id")]
    public string? RewardId { get; init; }

    /// <summary>Gets the name of the fundraising campaign associated with this donation.</summary>
    [JsonPropertyName("campaign_id")]
    public string? CampaignId { get; init; }

    /// <summary>Gets the cause ID associated with this donation.</summary>
    [JsonPropertyName("cause_id")]
    public string? CauseId { get; init; }

    /// <summary>Gets the fundraising team event ID, if applicable.</summary>
    [JsonPropertyName("fundraising_event_id")]
    public string? FundraisingEventId { get; init; }

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
