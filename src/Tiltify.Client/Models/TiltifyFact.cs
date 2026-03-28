using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a Tiltify "fact" — a milestone or target in a campaign delivered via webhook.
/// </summary>
public sealed record TiltifyFact
{
    /// <summary>Gets the fact ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the fact name/title.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the fact description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Gets the current/actual value of the fact (e.g. total raised).</summary>
    [JsonPropertyName("fact")]
    public string? Value { get; init; }

    /// <summary>Gets the campaign ID this fact belongs to.</summary>
    [JsonPropertyName("campaign_id")]
    public string? CampaignId { get; init; }

    /// <summary>Gets whether the milestone has been reached.</summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>Gets the timestamp when the fact was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
