using System.Text.Json.Serialization;

namespace Tiltify.Client.Models;

/// <summary>
/// Represents a monetary amount with currency information.
/// </summary>
public sealed record TiltifyAmount
{
    /// <summary>Gets the numeric value as a decimal string (e.g. "50.00").</summary>
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;

    /// <summary>Gets the ISO 4217 currency code (e.g. "USD").</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;
}
