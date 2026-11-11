using System.Text.Json;
using System.Text.Json.Serialization;
using Tiltify.Client.Internal;
using Tiltify.Client.Models;

namespace Tiltify.Client.Serialization;

/// <summary>
/// Source-generated JSON contracts for the types Tiltify.Client deserializes off the wire.
/// </summary>
/// <remarks>
/// The generated API surface (Kiota) carries its own serialization; this covers the hand-written
/// token, webhook and campaign API paths. Contracts are resolved here rather than from a bare
/// <see cref="JsonSerializerOptions"/> so the trimmer can see them and Native AOT can compile them —
/// the reflection-based overloads build contracts at run time and are unusable in either mode.
/// </remarks>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(TiltifyTokenResponse))]
[JsonSerializable(typeof(TiltifyWebhookEnvelope))]
[JsonSerializable(typeof(TiltifyDonation))]
[JsonSerializable(typeof(TiltifyFact))]
[JsonSerializable(typeof(TiltifyItemResponse<TiltifyCampaign>))]
[JsonSerializable(typeof(TiltifyItemResponse<TiltifyPoll>))]
[JsonSerializable(typeof(TiltifyListResponse<TiltifyPoll>))]
[JsonSerializable(typeof(TiltifyListResponse<TiltifyReward>))]
[JsonSerializable(typeof(TiltifyListResponse<TiltifyTarget>))]
[JsonSerializable(typeof(TiltifyListResponse<TiltifyMilestone>))]
internal sealed partial class TiltifyJsonContext : JsonSerializerContext;
