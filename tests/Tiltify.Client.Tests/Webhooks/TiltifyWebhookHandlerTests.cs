using System.Security.Cryptography;
using System.Text;
using Agash.Webhook.Abstractions;
using Tiltify.Client.Events;
using Tiltify.Client.Options;
using Tiltify.Client.Webhooks;
using Xunit;

namespace Tiltify.Client.Tests.Webhooks;

public sealed class TiltifyWebhookHandlerTests
{
    private const string Secret = "test-signing-secret";

    private readonly TiltifyWebhookSignatureVerifier _verifier = new();
    private readonly TiltifyWebhookHandler _handler;

    public TiltifyWebhookHandlerTests()
    {
        _handler = new TiltifyWebhookHandler(_verifier);
    }

    private static (string timestamp, string signature) Sign(string body, string secret)
    {
        string timestamp = "1711634400";
        byte[] signedString = Encoding.UTF8.GetBytes($"{timestamp}.{body}");
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
        string sig = Convert.ToBase64String(hmac.ComputeHash(signedString));
        return (timestamp, sig);
    }

    private static WebhookRequest BuildRequest(
        string body,
        string secret,
        string method = "POST",
        string contentType = "application/json")
    {
        (string timestamp, string signature) = Sign(body, secret);

        return new WebhookRequest
        {
            Method = method,
            Path = "/webhooks/tiltify",
            ContentType = contentType,
            Body = Encoding.UTF8.GetBytes(body),
            Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [TiltifyWebhookSignatureVerifier.TimestampHeaderName] = [timestamp],
                [TiltifyWebhookSignatureVerifier.SignatureHeaderName] = [signature],
                ["Content-Type"] = [contentType],
            },
        };
    }

    private static TiltifyWebhookOptions Options => new() { SigningSecret = Secret };

    [Fact]
    public async Task HandleAsync_ValidDonationEvent_ReturnsDonationWebhookEvent()
    {
        const string body = """
            {
              "meta": {
                "id": "delivery-abc",
                "event_name": "public:direct:donation_updated",
                "generated_at": "2024-03-28T10:00:00Z",
                "subscription_source_id": "sub-1",
                "subscription_target_id": "tgt-1",
                "attempt_number": 1
              },
              "data": {
                "id": "don-1",
                "donor_name": "TestDonor",
                "donor_comment": "For the cause!",
                "amount": { "value": "25.00", "currency": "USD" },
                "campaign_id": "camp-1",
                "cause_id": "cause-1",
                "completed_at": "2024-03-28T10:00:00Z",
                "donor_anonymous": false,
                "test": false
              }
            }
            """;

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret), Options);

        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsKnownEvent);
        Assert.Equal(200, result.Response.StatusCode);

        TiltifyDonationWebhookEvent donation = Assert.IsType<TiltifyDonationWebhookEvent>(result.Event);
        Assert.Equal("delivery-abc", donation.DeliveryId);
        Assert.True(donation.IsDirect);
        Assert.Equal("don-1", donation.Data.Id);
        Assert.Equal("TestDonor", donation.Data.DonorName);
        Assert.Equal("25.00", donation.Data.Amount?.Value);
        Assert.Equal("USD", donation.Data.Amount?.Currency);
    }

    [Fact]
    public async Task HandleAsync_IndirectDonationEvent_ReturnsDonationEventWithIsDirectFalse()
    {
        const string body = """
            {
              "meta": {
                "id": "delivery-xyz",
                "event_name": "public:indirect:donation_updated",
                "generated_at": "2024-03-28T10:00:00Z",
                "subscription_source_id": "sub-2",
                "subscription_target_id": "tgt-2",
                "attempt_number": 1
              },
              "data": {
                "id": "don-2",
                "donor_name": "IndirectDonor",
                "amount": { "value": "10.00", "currency": "USD" },
                "test": true
              }
            }
            """;

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret), Options);

        TiltifyDonationWebhookEvent donation = Assert.IsType<TiltifyDonationWebhookEvent>(result.Event);
        Assert.False(donation.IsDirect);
        Assert.Equal("don-2", donation.Data.Id);
    }

    [Fact]
    public async Task HandleAsync_ValidFactEvent_ReturnsFactWebhookEvent()
    {
        const string body = """
            {
              "meta": {
                "id": "delivery-fact-1",
                "event_name": "public:direct:fact_updated",
                "generated_at": "2024-03-28T10:00:00Z",
                "subscription_source_id": "sub-3",
                "subscription_target_id": "tgt-3",
                "attempt_number": 1
              },
              "data": {
                "id": "fact-1",
                "name": "Speedrun",
                "description": "Finish game any%",
                "value": "1500.00",
                "campaign_id": "camp-1",
                "active": true,
                "updated_at": "2024-03-28T09:00:00Z"
              }
            }
            """;

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret), Options);

        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsKnownEvent);

        TiltifyFactWebhookEvent factEvent = Assert.IsType<TiltifyFactWebhookEvent>(result.Event);
        Assert.True(factEvent.IsDirect);
        Assert.Equal("fact-1", factEvent.Data.Id);
        Assert.Equal("Speedrun", factEvent.Data.Name);
        Assert.True(factEvent.Data.Active);
    }

    [Fact]
    public async Task HandleAsync_UnknownEventName_ReturnsUnknownWebhookEvent()
    {
        const string body = """
            {
              "meta": {
                "id": "delivery-unk",
                "event_name": "public:direct:reward_claimed",
                "generated_at": "2024-03-28T10:00:00Z",
                "subscription_source_id": "sub-4",
                "subscription_target_id": "tgt-4",
                "attempt_number": 1
              },
              "data": { "reward_id": "rew-1" }
            }
            """;

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret), Options);

        Assert.True(result.IsAuthenticated);
        Assert.False(result.IsKnownEvent);

        TiltifyUnknownWebhookEvent unknown = Assert.IsType<TiltifyUnknownWebhookEvent>(result.Event);
        Assert.Equal("public:direct:reward_claimed", unknown.EventName);
    }

    [Fact]
    public async Task HandleAsync_InvalidSignature_Returns401Unauthenticated()
    {
        const string body = """{"meta":{"id":"x","event_name":"public:direct:donation_updated","generated_at":"2024-01-01T00:00:00Z","subscription_source_id":"s","subscription_target_id":"t","attempt_number":1},"data":{}}""";
        (string timestamp, _) = Sign(body, Secret);

        WebhookRequest request = new()
        {
            Method = "POST",
            Path = "/webhooks/tiltify",
            ContentType = "application/json",
            Body = Encoding.UTF8.GetBytes(body),
            Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [TiltifyWebhookSignatureVerifier.TimestampHeaderName] = [timestamp],
                [TiltifyWebhookSignatureVerifier.SignatureHeaderName] = ["invalid-signature"],
                ["Content-Type"] = ["application/json"],
            },
        };

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.False(result.IsAuthenticated);
        Assert.Equal(401, result.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_NonPostMethod_Returns405()
    {
        const string body = "{}";
        WebhookRequest request = BuildRequest(body, Secret, method: "GET");

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.Equal(405, result.Response.StatusCode);
        Assert.False(result.IsAuthenticated);
    }

    [Fact]
    public async Task HandleAsync_NonJsonContentType_Returns400()
    {
        const string body = "not json";
        WebhookRequest request = BuildRequest(body, Secret, contentType: "text/plain");

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.Equal(400, result.Response.StatusCode);
        Assert.False(result.IsAuthenticated);
    }

    [Fact]
    public async Task HandleAsync_MalformedJson_Returns400AfterAuth()
    {
        // Authenticate with valid signature but body isn't valid JSON for the envelope.
        const string body = "not-json-at-all";
        WebhookRequest request = BuildRequest(body, Secret);

        WebhookHandleResult<TiltifyWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.Equal(400, result.Response.StatusCode);
        // Auth fails too because content-type check happens first, but body is "not-json-at-all"
        // which doesn't have application/json content type from BuildRequest.
        // Actually BuildRequest defaults to application/json, so content-type passes, but JSON parsing fails.
        Assert.True(result.IsAuthenticated);
    }
}
