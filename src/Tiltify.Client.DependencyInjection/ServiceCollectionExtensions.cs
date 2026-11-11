using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Options;
using Tiltify.Client.Webhooks;

namespace Tiltify.Client.DependencyInjection;

/// <summary>Dependency injection registration for Tiltify.Client.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ITiltifyClientFactory"/>, the webhook handler, and the
    /// <see cref="TiltifyClientFactory.HttpClientName"/> HTTP client with Kiota's middleware. Configure that client
    /// further (proxy, primary handler) with <c>AddHttpClient(TiltifyClientFactory.HttpClientName)</c>.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configure">Optional changes to <see cref="TiltifyClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTiltifyClient(
        this IServiceCollection services,
        Action<TiltifyClientOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        OptionsBuilder<TiltifyClientOptions> options = services.AddOptions<TiltifyClientOptions>();
        if (configure is not null)
        {
            _ = options.Configure(configure);
        }

        _ = services
            .AddHttpClient(TiltifyClientFactory.HttpClientName)
            .ConfigureAdditionalHttpMessageHandlers(
                static (handlers, _) =>
                {
                    foreach (
                        System.Net.Http.DelegatingHandler handler in KiotaClientFactory.CreateDefaultHandlers()
                    )
                    {
                        handlers.Add(handler);
                    }
                }
            );

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ITiltifyClientFactory, TiltifyClientFactory>();
        services.TryAddSingleton<TiltifyWebhookSignatureVerifier>();
        services.TryAddSingleton<ITiltifyWebhookHandler, TiltifyWebhookHandler>();
        return services;
    }
}
