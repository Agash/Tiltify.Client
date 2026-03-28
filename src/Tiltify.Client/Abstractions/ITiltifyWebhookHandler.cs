using Agash.Webhook.Abstractions;
using Tiltify.Client.Events;
using Tiltify.Client.Options;

namespace Tiltify.Client.Abstractions;

/// <summary>
/// Transport-neutral handler for inbound Tiltify v5 webhook requests.
/// </summary>
public interface ITiltifyWebhookHandler
{
    /// <summary>
    /// Processes a Tiltify webhook request and returns a normalized result.
    /// </summary>
    /// <param name="request">The inbound webhook request.</param>
    /// <param name="options">The webhook options containing the signing secret.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="WebhookHandleResult{TEvent}"/> containing the outcome and normalized event.</returns>
    Task<WebhookHandleResult<TiltifyWebhookEvent>> HandleAsync(
        WebhookRequest request,
        TiltifyWebhookOptions options,
        CancellationToken cancellationToken = default);
}
