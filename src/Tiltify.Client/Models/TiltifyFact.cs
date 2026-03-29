using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a Tiltify "fact" — a fundraising campaign (individual, team, or cause)
/// delivered as part of a <c>public:direct:fact_updated</c> or
/// <c>public:indirect:fact_updated</c> webhook event.
/// </summary>
public sealed record TiltifyFact
{
    /// <summary>Gets the fact/campaign ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the campaign name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the campaign slug used in URLs.</summary>
    [JsonPropertyName("slug")]
    public string? Slug { get; init; }

    /// <summary>Gets a short description of the campaign.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the campaign status.
    /// Typical values: <c>active</c>, <c>inactive</c>, <c>published</c>, <c>unpublished</c>.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Gets the type of entity running this campaign.
    /// Typical values: <c>user</c>, <c>team</c>, <c>cause</c>.
    /// </summary>
    [JsonPropertyName("supporting_type")]
    public string? SupportingType { get; init; }

    /// <summary>Gets the total amount raised directly by this campaign (excludes sub-campaigns).</summary>
    [JsonPropertyName("amount_raised")]
    public TiltifyAmount? AmountRaised { get; init; }

    /// <summary>Gets the total amount raised including all supporting sub-campaigns.</summary>
    [JsonPropertyName("total_amount_raised")]
    public TiltifyAmount? TotalAmountRaised { get; init; }

    /// <summary>Gets the current fundraising goal.</summary>
    [JsonPropertyName("goal")]
    public TiltifyAmount? Goal { get; init; }

    /// <summary>Gets the original fundraising goal before any updates.</summary>
    [JsonPropertyName("original_goal")]
    public TiltifyAmount? OriginalGoal { get; init; }

    /// <summary>Gets the fundraiser-facing public URL for this campaign.</summary>
    [JsonPropertyName("fact_url")]
    public string? FactUrl { get; init; }

    /// <summary>Gets the direct donation URL for this campaign.</summary>
    [JsonPropertyName("donate_url")]
    public string? DonateUrl { get; init; }

    /// <summary>Gets the ID of the parent cause this campaign belongs to.</summary>
    [JsonPropertyName("cause_id")]
    public string? CauseId { get; init; }

    /// <summary>Gets the ID of the parent fundraising event, if any.</summary>
    [JsonPropertyName("fundraising_event_id")]
    public string? FundraisingEventId { get; init; }

    /// <summary>Gets whether this campaign is currently accepting donations.</summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when the campaign is scheduled to start.</summary>
    [JsonPropertyName("starts_at")]
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when the campaign is scheduled to end.</summary>
    [JsonPropertyName("ends_at")]
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when the campaign was first created.</summary>
    [JsonPropertyName("inserted_at")]
    public DateTimeOffset? InsertedAt { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when the campaign was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
