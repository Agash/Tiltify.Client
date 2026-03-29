using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Http;
using Tiltify.Client.Events;
using Tiltify.Client.Options;

namespace Tiltify.Client.AspNetCore;

/// <summary>
/// Options for a Tiltify webhook endpoint registered via
/// <see cref="TiltifyEndpointRouteBuilderExtensions"/>.
/// </summary>
public sealed class TiltifyWebhookEndpointOptions
{
    /// <summary>
    /// Gets or sets the delegate that resolves the per-request <see cref="TiltifyWebhookOptions"/>.
    /// </summary>
    public required Func<HttpContext, CancellationToken, Task<TiltifyWebhookOptions>> ResolveWebhookOptionsAsync { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked for each successfully parsed event.
    /// </summary>
    public Func<TiltifyWebhookEvent, HttpContext, CancellationToken, Task>? OnEventAsync { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked after every request with the full handle result.
    /// </summary>
    public Func<WebhookHandleResult<TiltifyWebhookEvent>, HttpContext, CancellationToken, Task>? OnResultAsync { get; set; }
}
