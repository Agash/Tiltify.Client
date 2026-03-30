using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a single reward claim within a Tiltify donation.
/// Donors may claim multiple rewards per donation; this replaces the deprecated
/// single-reward <c>reward_id</c> field.
/// </summary>
public sealed record TiltifyRewardClaim
{
    /// <summary>Gets the reward claim ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the ID of the fundraiser reward being claimed.</summary>
    [JsonPropertyName("reward_id")]
    public string? RewardId { get; init; }

    /// <summary>Gets the quantity claimed (for limited-quantity rewards).</summary>
    [JsonPropertyName("quantity")]
    public int? Quantity { get; init; }
}
