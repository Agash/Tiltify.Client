using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a donation match associated with a Tiltify donation.
/// Donation matches multiply the donor's impact when active pledges are in effect.
/// </summary>
public sealed record TiltifyDonationMatch
{
    /// <summary>Gets the donation match ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the matched amount (what the match pledger contributes).</summary>
    [JsonPropertyName("amount")]
    public TiltifyAmount? Amount { get; init; }

    /// <summary>Gets the ID of the pledge that backs this match.</summary>
    [JsonPropertyName("pledge_id")]
    public string? PledgeId { get; init; }
}
