using System.Text.Json;
using Agash.Webhook.Abstractions;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Events;
using Tiltify.Client.Internal;
using Tiltify.Client.Models;
using Tiltify.Client.Options;

namespace Tiltify.Client.Webhooks;

/// <summary>
/// Default transport-neutral implementation of <see cref="ITiltifyWebhookHandler"/>.
/// Verifies the Tiltify webhook signature and deserializes the payload into a typed event.
/// </summary>
public sealed class TiltifyWebhookHandler : ITiltifyWebhookHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TiltifyWebhookSignatureVerifier _signatureVerifier;

    /// <summary>
    /// Initializes a new instance of <see cref="TiltifyWebhookHandler"/>.
    /// </summary>
    public TiltifyWebhookHandler(TiltifyWebhookSignatureVerifier signatureVerifier)
    {
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
    }

    /// <inheritdoc />
    public Task<WebhookHandleResult<TiltifyWebhookEvent>> HandleAsync(
        WebhookRequest request,
        TiltifyWebhookOptions options,
        CancellationToken cancellationToken = default)
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
            return Task.FromResult(Failure(400, false, false, "Expected application/json content type."));
        }

        string? timestamp = request.GetFirstHeaderValue(TiltifyWebhookSignatureVerifier.TimestampHeaderName);
        string? signature = request.GetFirstHeaderValue(TiltifyWebhookSignatureVerifier.SignatureHeaderName);

        if (!_signatureVerifier.Verify(request.Body, timestamp, signature, options.SigningSecret))
        {
            return Task.FromResult(Failure(401, false, false,
                $"Invalid or missing Tiltify webhook signature (headers: {TiltifyWebhookSignatureVerifier.TimestampHeaderName}, {TiltifyWebhookSignatureVerifier.SignatureHeaderName})."));
        }

        TiltifyWebhookEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<TiltifyWebhookEnvelope>(request.Body, _jsonOptions);
        }
        catch (JsonException ex)
        {
            return Task.FromResult(Failure(400, true, false, $"Failed to parse Tiltify webhook JSON: {ex.Message}"));
        }

        if (envelope is null)
        {
            return Task.FromResult(Failure(400, true, false, "Tiltify webhook payload could not be deserialized."));
        }

        TiltifyWebhookEvent evt = MapEvent(envelope);
        bool isKnown = evt is not TiltifyUnknownWebhookEvent;

        return Task.FromResult(new WebhookHandleResult<TiltifyWebhookEvent>
        {
            Response = WebhookResponse.Empty(200),
            IsAuthenticated = true,
            IsKnownEvent = isKnown,
            Event = evt,
            FailureReason = null,
        });
    }

    private static TiltifyWebhookEvent MapEvent(TiltifyWebhookEnvelope envelope)
    {
        string eventName = envelope.Meta.EventName;

        switch (eventName)
        {
            case TiltifyWebhookEventNames.DirectDonationUpdated:
            case TiltifyWebhookEventNames.IndirectDonationUpdated:
                {
                    TiltifyDonation? donation = TryDeserialize<TiltifyDonation>(envelope.Data);
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
                        IsDirect = eventName == TiltifyWebhookEventNames.DirectDonationUpdated,
                    };
                }

            case TiltifyWebhookEventNames.DirectFactUpdated:
            case TiltifyWebhookEventNames.IndirectFactUpdated:
                {
                    TiltifyFact? fact = TryDeserialize<TiltifyFact>(envelope.Data);
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
                        IsDirect = eventName == TiltifyWebhookEventNames.DirectFactUpdated,
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

    private static T? TryDeserialize<T>(JsonElement element)
    {
        try
        {
            return element.Deserialize<T>(_jsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static WebhookHandleResult<TiltifyWebhookEvent> Failure(
        int statusCode, bool isAuthenticated, bool isKnownEvent, string reason)
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
