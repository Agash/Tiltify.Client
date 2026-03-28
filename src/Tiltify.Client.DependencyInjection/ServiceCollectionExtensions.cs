using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tiltify.Client;
using Tiltify.Client.Abstractions;
using Tiltify.Client.Authentication;
using Tiltify.Client.Options;
using Tiltify.Client.Webhooks;

namespace Tiltify.Client.DependencyInjection;

/// <summary>
/// Provides dependency injection registration helpers for Tiltify.Client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Tiltify client services to the specified service collection using the provided options.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configure">A delegate that configures the <see cref="TiltifyClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddTiltifyClient(
        this IServiceCollection services,
        Action<TiltifyClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        return services.AddTiltifyClientCore();
    }

    /// <summary>
    /// Adds Tiltify client services to the specified service collection.
    /// <see cref="TiltifyClientOptions"/> must already be registered in the container.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddTiltifyClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddTiltifyClientCore();
    }

    private static IServiceCollection AddTiltifyClientCore(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.TryAddSingleton(sp =>
        {
            TiltifyClientOptions opts = sp.GetRequiredService<IOptions<TiltifyClientOptions>>().Value;
            System.Net.Http.IHttpClientFactory httpFactory = sp.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            System.Net.Http.HttpClient httpClient = httpFactory.CreateClient(nameof(TiltifyTokenProvider));
            return new TiltifyTokenProvider(Microsoft.Extensions.Options.Options.Create(opts), httpClient);
        });

        services.TryAddSingleton<ITiltifyClientFactory>(sp =>
        {
            TiltifyClientOptions opts = sp.GetRequiredService<IOptions<TiltifyClientOptions>>().Value;
            TiltifyTokenProvider tokenProvider = sp.GetRequiredService<TiltifyTokenProvider>();
            return new TiltifyClientFactory(tokenProvider, opts);
        });

        services.TryAddSingleton<TiltifyWebhookSignatureVerifier>();
        services.TryAddSingleton<ITiltifyWebhookHandler, TiltifyWebhookHandler>();

        return services;
    }
}
