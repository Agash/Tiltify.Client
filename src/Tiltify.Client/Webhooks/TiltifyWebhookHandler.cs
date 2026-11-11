using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Agash.Webhook.Abstractions;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Events;
using Tiltify.Client.Internal;
using Tiltify.Client.Models;
using Tiltify.Client.Options;
using Tiltify.Client.Serialization;

namespace Tiltify.Client.Webhooks;

/// <summary>
/// Default transport-neutral implementation of <see cref="ITiltifyWebhookHandler"/>.
/// Verifies the Tiltify webhook signature and deserializes the payload into a typed event.
/// </summary>
public sealed class TiltifyWebhookHandler : ITiltifyWebhookHandler
{
    private readonly TiltifyWebhookSignatureVerifier _signatureVerifier;
    private readonly TimeProvider _time;

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyWebhookHandler"/>.
    /// </summary>
    public TiltifyWebhookHandler(
        TiltifyWebhookSignatureVerifier signatureVerifier,
        TimeProvider timeProvider
    )
    {
        _signatureVerifier =
            signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        _time = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public Task<WebhookHandleResult<TiltifyWebhookEvent>> HandleAsync(
        WebhookRequest request,
        TiltifyWebhookOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(options.SigningSecret);

        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Failure(405, false, false, "Tiltify webhooks must use POST."));
        }

        if (!request.HasContentType("application/json"))
        {
            return Task.FromResult(
                Failure(400, false, false, "Expected application/json content type.")
            );
        }

        string? timestamp = request.GetFirstHeaderValue(
            TiltifyWebhookSignatureVerifier.TimestampHeaderName
        );
        string? signature = request.GetFirstHeaderValue(
            TiltifyWebhookSignatureVerifier.SignatureHeaderName
        );

        if (!_signatureVerifier.Verify(request.Body, timestamp, signature, options.SigningSecret))
        {
            return Task.FromResult(
                Failure(
                    401,
                    false,
                    false,
                    $"Invalid or missing Tiltify webhook signature (headers: {TiltifyWebhookSignatureVerifier.TimestampHeaderName}, {TiltifyWebhookSignatureVerifier.SignatureHeaderName})."
                )
            );
        }

        // Tiltify asks receivers to check the timestamp is recent, so a captured delivery cannot be replayed later.
        if (
            options.MaxTimestampAge is { } maxAge
            && DateTimeOffset.TryParse(
                timestamp,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal,
                out DateTimeOffset sentAt
            )
            && (_time.GetUtcNow() - sentAt).Duration() > maxAge
        )
        {
            return Task.FromResult(
                Failure(401, false, false, "Tiltify webhook timestamp is too old or in the future.")
            );
        }

        TiltifyWebhookEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize(
                request.Body,
                TiltifyJsonContext.Default.TiltifyWebhookEnvelope
            );
        }
        catch (JsonException ex)
        {
            return Task.FromResult(
                Failure(400, true, false, $"Failed to parse Tiltify webhook JSON: {ex.Message}")
            );
        }

        if (envelope is null)
        {
            return Task.FromResult(
                Failure(400, true, false, "Tiltify webhook payload could not be deserialized.")
            );
        }

        TiltifyWebhookEvent evt = MapEvent(envelope);
        bool isKnown = evt is not TiltifyUnknownWebhookEvent;

        return Task.FromResult(
            new WebhookHandleResult<TiltifyWebhookEvent>
            {
                Response = WebhookResponse.Empty(200),
                IsAuthenticated = true,
                IsKnownEvent = isKnown,
                Event = evt,
                FailureReason = null,
            }
        );
    }

    private static TiltifyWebhookEvent MapEvent(TiltifyWebhookEnvelope envelope)
    {
        string eventName = envelope.Meta.EventType;
        string[] parts = eventName.Split(':');
        bool isPrivate = parts.Length == 3 && parts[0] == "private";
        bool isDirect = parts.Length == 3 && parts[1] == "direct";
        switch (parts.Length == 3 ? parts[2] : string.Empty)
        {
            case "donation_updated":
            {
                TiltifyDonation? donation = TryDeserialize(
                    envelope.Data,
                    TiltifyJsonContext.Default.TiltifyDonation
                );
                if (donation is null)
                {
                    break;
                }

                return new TiltifyDonationWebhookEvent
                {
                    DeliveryId = envelope.Meta.Id,
                    EventName = eventName,
                    GeneratedAt = envelope.Meta.GeneratedAt,
                    Meta = envelope.Meta,
                    Data = donation,
                    IsDirect = isDirect,
                    IsPrivate = isPrivate,
                };
            }

            case "fact_updated":
            {
                TiltifyFact? fact = TryDeserialize(
                    envelope.Data,
                    TiltifyJsonContext.Default.TiltifyFact
                );
                if (fact is null)
                {
                    break;
                }

                return new TiltifyFactWebhookEvent
                {
                    DeliveryId = envelope.Meta.Id,
                    EventName = eventName,
                    GeneratedAt = envelope.Meta.GeneratedAt,
                    Meta = envelope.Meta,
                    Data = fact,
                    IsDirect = isDirect,
                    IsPrivate = isPrivate,
                };
            }

            default:
                break;
        }

        return new TiltifyUnknownWebhookEvent
        {
            DeliveryId = envelope.Meta.Id,
            EventName = eventName,
            GeneratedAt = envelope.Meta.GeneratedAt,
            Meta = envelope.Meta,
            RawData = envelope.Data.Clone(),
        };
    }

    private static T? TryDeserialize<T>(JsonElement element, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return element.Deserialize(typeInfo);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static WebhookHandleResult<TiltifyWebhookEvent> Failure(
        int statusCode,
        bool isAuthenticated,
        bool isKnownEvent,
        string reason
    )
    {
        return new()
        {
            Response = WebhookResponse.Empty(statusCode),
            IsAuthenticated = isAuthenticated,
            IsKnownEvent = isKnownEvent,
            Event = null,
            FailureReason = reason,
        };
    }
}
