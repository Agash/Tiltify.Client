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
/// token and webhook paths. Contracts are resolved here rather than from a bare
/// <see cref="JsonSerializerOptions"/> so the trimmer can see them and Native AOT can compile them —
/// the reflection-based overloads build contracts at run time and are unusable in either mode.
/// </remarks>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(TiltifyTokenResponse))]
[JsonSerializable(typeof(TiltifyWebhookEnvelope))]
[JsonSerializable(typeof(TiltifyDonation))]
[JsonSerializable(typeof(TiltifyFact))]
internal sealed partial class TiltifyJsonContext : JsonSerializerContext;
