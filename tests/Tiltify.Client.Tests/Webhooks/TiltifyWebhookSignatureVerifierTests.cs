using System.Security.Cryptography;
using System.Text;
using Tiltify.Client.Webhooks;
using Xunit;

namespace Tiltify.Client.Tests.Webhooks;

public sealed class TiltifyWebhookSignatureVerifierTests
{
    private readonly TiltifyWebhookSignatureVerifier _verifier = new();
    private const string Secret = "test-signing-secret";

    private static (byte[] body, string timestamp, string signature) MakeValidSignature(
        string body, string timestamp, string secret)
    {
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
        byte[] signedString = Encoding.UTF8.GetBytes($"{timestamp}.{body}");
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
        string sig = Convert.ToBase64String(hmac.ComputeHash(signedString));
        return (bodyBytes, timestamp, sig);
    }

    [Fact]
    public void Verify_ValidSignature_ReturnsTrue()
    {
        const string body = "{\"meta\":{\"event_name\":\"public:direct:donation_updated\"}}";
        const string timestamp = "1711634400";
        (byte[] bodyBytes, string ts, string sig) = MakeValidSignature(body, timestamp, Secret);

        bool result = _verifier.Verify(bodyBytes, ts, sig, Secret);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WrongSecret_ReturnsFalse()
    {
        const string body = "{\"meta\":{}}";
        const string timestamp = "1711634400";
        (byte[] bodyBytes, string ts, string sig) = MakeValidSignature(body, timestamp, "correct-secret");

        bool result = _verifier.Verify(bodyBytes, ts, sig, "wrong-secret");

        Assert.False(result);
    }

    [Fact]
    public void Verify_TamperedBody_ReturnsFalse()
    {
        const string originalBody = "{\"meta\":{\"id\":\"abc\"}}";
        const string tamperedBody = "{\"meta\":{\"id\":\"xyz\"}}";
        const string timestamp = "1711634400";
        (_, string ts, string sig) = MakeValidSignature(originalBody, timestamp, Secret);

        bool result = _verifier.Verify(Encoding.UTF8.GetBytes(tamperedBody), ts, sig, Secret);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null, "sig")]
    [InlineData("ts", null)]
    [InlineData("", "sig")]
    [InlineData("ts", "")]
    public void Verify_MissingHeader_ReturnsFalse(string? timestamp, string? signature)
    {
        bool result = _verifier.Verify(Encoding.UTF8.GetBytes("{}"), timestamp, signature, Secret);

        Assert.False(result);
    }

    [Fact]
    public void Verify_EmptyBody_ValidSignature_ReturnsTrue()
    {
        const string body = "";
        const string timestamp = "1711634400";
        (byte[] bodyBytes, string ts, string sig) = MakeValidSignature(body, timestamp, Secret);

        bool result = _verifier.Verify(bodyBytes, ts, sig, Secret);

        Assert.True(result);
    }
}
