using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>A Tiltify campaign as the public API returns it.</summary>
public sealed record TiltifyCampaign
{
    /// <summary>The id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The URL slug.</summary>
    [JsonPropertyName("slug")]
    public string? Slug { get; init; }

    /// <summary>The description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The public page.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>The donation page.</summary>
    [JsonPropertyName("donate_url")]
    public string? DonateUrl { get; init; }

    /// <summary>The cause the campaign raises money for.</summary>
    [JsonPropertyName("cause_id")]
    public string? CauseId { get; init; }

    /// <summary>Raised by this campaign itself.</summary>
    [JsonPropertyName("amount_raised")]
    public TiltifyAmount? AmountRaised { get; init; }

    /// <summary>Raised including supporting campaigns.</summary>
    [JsonPropertyName("total_amount_raised")]
    public TiltifyAmount? TotalAmountRaised { get; init; }

    /// <summary>The current goal.</summary>
    [JsonPropertyName("goal")]
    public TiltifyAmount? Goal { get; init; }

    /// <summary>The goal the campaign started with.</summary>
    [JsonPropertyName("original_goal")]
    public TiltifyAmount? OriginalGoal { get; init; }

    /// <summary>When the campaign was published.</summary>
    [JsonPropertyName("published_at")]
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>When the campaign was retired, if it was.</summary>
    [JsonPropertyName("retired_at")]
    public DateTimeOffset? RetiredAt { get; init; }
}

/// <summary>A campaign poll: donors vote with donations for one of its options.</summary>
public sealed record TiltifyPoll
{
    /// <summary>The id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Whether it is active.</summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>The amount raised.</summary>
    [JsonPropertyName("amount_raised")]
    public TiltifyAmount? AmountRaised { get; init; }

    /// <summary>The options, each with its own amount raised.</summary>
    [JsonPropertyName("options")]
    public IReadOnlyList<TiltifyPollOption>? Options { get; init; }
}

/// <summary>One option of a <see cref="TiltifyPoll"/>.</summary>
public sealed record TiltifyPollOption
{
    /// <summary>The id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The amount raised.</summary>
    [JsonPropertyName("amount_raised")]
    public TiltifyAmount? AmountRaised { get; init; }
}

/// <summary>A reward donors can claim with a donation.</summary>
public sealed record TiltifyReward
{
    /// <summary>The id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The donation needed to claim it.</summary>
    [JsonPropertyName("amount")]
    public TiltifyAmount? Amount { get; init; }

    /// <summary>Whether it is active.</summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>Whether the campaign highlights it.</summary>
    [JsonPropertyName("highlighted")]
    public bool? Highlighted { get; init; }

    /// <summary>How many there are, or null for no limit.</summary>
    [JsonPropertyName("quantity")]
    public int? Quantity { get; init; }

    /// <summary>How many are left.</summary>
    [JsonPropertyName("quantity_remaining")]
    public int? QuantityRemaining { get; init; }

    /// <summary>When it becomes available.</summary>
    [JsonPropertyName("starts_at")]
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>When it ends.</summary>
    [JsonPropertyName("ends_at")]
    public DateTimeOffset? EndsAt { get; init; }
}

/// <summary>A target: a sub-goal donations count toward, often with a deadline.</summary>
public sealed record TiltifyTarget
{
    /// <summary>The id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The amount.</summary>
    [JsonPropertyName("amount")]
    public TiltifyAmount? Amount { get; init; }

    /// <summary>The amount raised.</summary>
    [JsonPropertyName("amount_raised")]
    public TiltifyAmount? AmountRaised { get; init; }

    /// <summary>Whether it is active.</summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>When it ends.</summary>
    [JsonPropertyName("ends_at")]
    public DateTimeOffset? EndsAt { get; init; }
}

/// <summary>A milestone: an amount the campaign announces something at.</summary>
public sealed record TiltifyMilestone
{
    /// <summary>The id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The amount.</summary>
    [JsonPropertyName("amount")]
    public TiltifyAmount? Amount { get; init; }

    /// <summary>Whether it is active.</summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }
}

/// <summary>The <c>{ "data": ... }</c> wrapper of a single-item response.</summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record TiltifyItemResponse<T>
{
    /// <summary>The payload.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }
}

/// <summary>The <c>{ "data": [...] }</c> wrapper of a list response.</summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record TiltifyListResponse<T>
{
    /// <summary>The payload.</summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<T>? Data { get; init; }
}
