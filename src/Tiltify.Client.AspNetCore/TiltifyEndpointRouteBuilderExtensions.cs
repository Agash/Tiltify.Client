using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Events;
using Tiltify.Client.Options;

namespace Tiltify.Client.AspNetCore;

/// <summary>
/// Provides endpoint mapping extensions for exposing <see cref="ITiltifyWebhookHandler"/>
/// through ASP.NET Core minimal APIs.
/// </summary>
public static class TiltifyEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a Tiltify webhook endpoint using the supplied endpoint options.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to extend.</param>
    /// <param name="pattern">The route pattern to map.</param>
    /// <param name="configure">The callback used to configure endpoint options.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapTiltifyWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Action<TiltifyWebhookEndpointOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        ArgumentNullException.ThrowIfNull(configure);

        TiltifyWebhookEndpointOptions options = new()
        {
            ResolveWebhookOptionsAsync = static (_, _) => Task.FromResult(new TiltifyWebhookOptions
            {
                SigningSecret = string.Empty,
            }),
        };

        configure(options);

        return endpoints.MapPost(pattern, async (HttpContext context) =>
        {
            ITiltifyWebhookHandler handler = context.RequestServices.GetRequiredService<ITiltifyWebhookHandler>();

            TiltifyWebhookOptions webhookOptions =
                await options.ResolveWebhookOptionsAsync(context, context.RequestAborted).ConfigureAwait(false);

            WebhookRequest request =
                await HttpContextWebhookRequestMapper.FromHttpContextAsync(context, context.RequestAborted)
                    .ConfigureAwait(false);

            WebhookHandleResult<TiltifyWebhookEvent> result =
                await handler.HandleAsync(request, webhookOptions, context.RequestAborted)
                    .ConfigureAwait(false);

            if (result.Event is TiltifyWebhookEvent evt && options.OnEventAsync is not null)
            {
                await options.OnEventAsync(evt, context, context.RequestAborted).ConfigureAwait(false);
            }

            if (options.OnResultAsync is not null)
            {
                await options.OnResultAsync(result, context, context.RequestAborted).ConfigureAwait(false);
            }

            await WebhookResponseHttpContextWriter.WriteAsync(context, result.Response, context.RequestAborted)
                .ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Maps a Tiltify webhook endpoint using a direct webhook options resolver delegate.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to extend.</param>
    /// <param name="pattern">The route pattern to map.</param>
    /// <param name="resolveWebhookOptionsAsync">The callback used to resolve webhook options.</param>
    /// <param name="onEventAsync">An optional event callback.</param>
    /// <param name="onResultAsync">An optional result callback.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapTiltifyWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, CancellationToken, Task<TiltifyWebhookOptions>> resolveWebhookOptionsAsync,
        Func<TiltifyWebhookEvent, HttpContext, CancellationToken, Task>? onEventAsync = null,
        Func<WebhookHandleResult<TiltifyWebhookEvent>, HttpContext, CancellationToken, Task>? onResultAsync = null)
    {
        ArgumentNullException.ThrowIfNull(resolveWebhookOptionsAsync);

        return endpoints.MapTiltifyWebhook(
            pattern,
            options =>
            {
                options.ResolveWebhookOptionsAsync = resolveWebhookOptionsAsync;
                options.OnEventAsync = onEventAsync;
                options.OnResultAsync = onResultAsync;
            });
    }
}
