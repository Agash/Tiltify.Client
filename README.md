# Tiltify.Client

[![NuGet](https://img.shields.io/nuget/v/Tiltify.Client.svg?include_prereleases)](https://www.nuget.org/packages/Tiltify.Client/)
[![CI](https://github.com/Agash/Tiltify.Client/actions/workflows/build.yml/badge.svg)](https://github.com/Agash/Tiltify.Client/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A modern, async-first .NET 10 client library for the [Tiltify v5 API](https://v5api.tiltify.com), featuring:

- **Kiota-generated** typed REST client from the official OpenAPI spec
- **OAuth client credentials** token management with automatic refresh
- **Webhook signature verification** (HMAC-SHA256 + Base64 using `Agash.Webhook.Abstractions`)
- **Typed webhook event models** for donations, facts/milestones, and more
- **Dependency injection** extensions for easy registration
- **Native AOT** compatible

## Packages

| Package | Description |
|---|---|
| `Tiltify.Client` | Core client, auth, webhook handler and models |
| `Tiltify.Client.Generated` | Kiota-generated REST client |
| `Tiltify.Client.DependencyInjection` | DI registration helpers |
| `Tiltify.Client.AspNetCore` | ASP.NET Core middleware and endpoint helpers for webhook ingestion |

## Quick Start

```csharp
// Register services
services.AddTiltifyClient(options =>
{
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
});

// Inject and use the factory
var factory = serviceProvider.GetRequiredService<ITiltifyClientFactory>();
using var client = factory.CreateApiClient();
var campaigns = await client.Api.Public.Campaigns.GetAsync();

// Handle webhooks
var handler = serviceProvider.GetRequiredService<ITiltifyWebhookHandler>();
var result = await handler.HandleAsync(webhookRequest, new TiltifyWebhookOptions
{
    SigningSecret = "your-webhook-signing-secret",
});

if (result.IsAuthenticated && result.Event is TiltifyDonationWebhookEvent donation)
{
    Console.WriteLine($"Donation: {donation.Data.DonorName} — {donation.Data.Amount.Value} {donation.Data.Amount.Currency}");
}
```

## Webhook Signature Verification

Tiltify signs webhook deliveries using HMAC-SHA256. The signed string is `{X-Tiltify-Timestamp}.{rawBody}`, and the signature is Base64-encoded. This library verifies that automatically using the configured signing secret.

## License

MIT
