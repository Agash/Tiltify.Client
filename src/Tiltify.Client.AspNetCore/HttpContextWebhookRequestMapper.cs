using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Tiltify.Client.AspNetCore;

/// <summary>
/// Converts ASP.NET Core <see cref="HttpContext"/> instances into transport-neutral
/// <see cref="WebhookRequest"/> objects.
/// </summary>
public static class HttpContextWebhookRequestMapper
{
    /// <summary>
    /// Creates a <see cref="WebhookRequest"/> from the specified <see cref="HttpContext"/>.
    /// </summary>
    public static async Task<WebhookRequest> FromHttpContextAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        Dictionary<string, string[]> headers = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string key, StringValues value) in context.Request.Headers)
        {
            headers[key] = [.. value.Select(static x => x ?? string.Empty)];
        }

        byte[] body;

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }

        using (MemoryStream buffer = new())
        {
            await context.Request.Body.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            body = buffer.ToArray();
        }

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }

        string? rawPath = context.Request.Path.Value;

        return new WebhookRequest
        {
            Method = context.Request.Method,
            Path = string.IsNullOrEmpty(rawPath) ? "/" : rawPath,
            QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
            ContentType = context.Request.ContentType,
            Headers = headers,
            Body = body,
        };
    }
}
