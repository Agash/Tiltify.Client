using DevTunnels.Client;
using Spectre.Console;
using System.Collections.Concurrent;
using Tiltify.Client.AspNetCore;
using Tiltify.Client.DependencyInjection;
using Tiltify.Client.Events;
using Tiltify.Client.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

CancellationTokenSource shutdown = new();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    shutdown.Cancel();
};

try
{
    await SampleApplication.RunAsync(shutdown.Token).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    // Normal shutdown path.
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}

internal static class SampleApplication
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("Tiltify Sample")
                .Color(Color.CornflowerBlue));

        AnsiConsole.MarkupLine("[grey]Tiltify v5 API + webhook sample with ASP.NET Core, Spectre.Console, and DevTunnels.Client.[/]");
        AnsiConsole.WriteLine();

        SampleConfiguration configuration = PromptConfiguration();

        ConcurrentQueue<TiltifyWebhookEvent> receivedEvents = new();
        object consoleLock = new();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://127.0.0.1:{configuration.LocalPort}");
        builder.Services.AddTiltifyClient(opts =>
        {
            opts.ClientId = configuration.ClientId;
            opts.ClientSecret = configuration.ClientSecret;
        });

        WebApplication app = builder.Build();

        app.MapGet(
            "/",
            () => Results.Text(
                "Tiltify.Client.Sample is running.\n" +
                "POST Tiltify webhook payloads to the configured route.\n",
                "text/plain"));

        app.MapTiltifyWebhook(
            configuration.WebhookPath,
            (context, ct) => Task.FromResult(new TiltifyWebhookOptions
            {
                SigningSecret = configuration.SigningSecret,
            }),
            async (evt, _, _) =>
            {
                receivedEvents.Enqueue(evt);
                lock (consoleLock)
                {
                    RenderReceivedEvent(evt);
                }
                await Task.CompletedTask.ConfigureAwait(false);
            },
            async (result, httpContext, _) =>
            {
                lock (consoleLock)
                {
                    string remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    string requestId = httpContext.TraceIdentifier;

                    string auth = result.IsAuthenticated ? "[green]yes[/]" : "[red]no[/]";
                    string known = result.IsKnownEvent ? "[green]yes[/]" : "[yellow]no[/]";
                    string status = $"[blue]{result.Response.StatusCode}[/]";

                    AnsiConsole.MarkupLineInterpolated(
                        $"[grey]Request[/] [white]{Markup.Escape(requestId)}[/] from [white]{Markup.Escape(remoteIp)}[/] -> status {status}, authenticated {auth}, known event {known}.");

                    if (!string.IsNullOrWhiteSpace(result.FailureReason))
                    {
                        AnsiConsole.MarkupLineInterpolated($"[yellow]Reason:[/] {Markup.Escape(result.FailureReason)}");
                    }
                }
                await Task.CompletedTask.ConfigureAwait(false);
            });

        await app.StartAsync(cancellationToken).ConfigureAwait(false);

        string localBaseUrl = $"http://127.0.0.1:{configuration.LocalPort}";
        RenderStartupSummary(configuration, localBaseUrl);

        DevTunnelsRuntime? devTunnelsRuntime = null;
        if (configuration.UseDevTunnels)
        {
            devTunnelsRuntime = await StartDevTunnelsAsync(configuration, cancellationToken).ConfigureAwait(false);
            RenderTunnelSummary(configuration, devTunnelsRuntime.PublicBaseUrl);
        }

        RenderUsageInstructions(configuration, localBaseUrl, devTunnelsRuntime?.PublicBaseUrl);

        await RunCommandLoopAsync(configuration, receivedEvents, devTunnelsRuntime, consoleLock, cancellationToken)
            .ConfigureAwait(false);

        if (devTunnelsRuntime is not null)
        {
            await devTunnelsRuntime.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }

        await app.StopAsync(CancellationToken.None).ConfigureAwait(false);
        await app.DisposeAsync().ConfigureAwait(false);
    }

    private static SampleConfiguration PromptConfiguration()
    {
        int localPort = AnsiConsole.Prompt(
            new TextPrompt<int>("Local [green]HTTP port[/]?")
                .DefaultValue(5075)
                .Validate(port => port is > 0 and <= 65535
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Port must be between 1 and 65535.[/]")));

        string webhookPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Webhook [green]path[/]?")
                .DefaultValue("/webhooks/tiltify/events")
                .AllowEmpty());

        if (string.IsNullOrWhiteSpace(webhookPath))
        {
            webhookPath = "/webhooks/tiltify/events";
        }

        if (!webhookPath.StartsWith('/'))
        {
            webhookPath = "/" + webhookPath;
        }

        string clientId = AnsiConsole.Prompt(
            new TextPrompt<string>("Tiltify [green]Client ID[/]?")
                .PromptStyle("deepskyblue1"));

        string clientSecret = AnsiConsole.Prompt(
            new TextPrompt<string>("Tiltify [green]Client Secret[/]?")
                .PromptStyle("deepskyblue1")
                .Secret());

        string signingSecret = AnsiConsole.Prompt(
            new TextPrompt<string>("Webhook [green]signing secret[/]?")
                .PromptStyle("deepskyblue1")
                .Secret());

        bool useDevTunnels = AnsiConsole.Confirm("Use [green]Azure Dev Tunnels[/] for a public HTTPS URL?", true);

        string tunnelId = "tiltify-client-sample";
        LoginProvider loginProvider = LoginProvider.GitHub;

        if (useDevTunnels)
        {
            tunnelId = AnsiConsole.Prompt(
                new TextPrompt<string>("Dev Tunnel [green]tunnel ID[/]?")
                    .DefaultValue("tiltify-client-sample")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(tunnelId))
            {
                tunnelId = "tiltify-client-sample";
            }

            loginProvider = AnsiConsole.Prompt(
                new SelectionPrompt<LoginProvider>()
                    .Title("Login provider for [green]devtunnel[/]?")
                    .AddChoices(LoginProvider.GitHub, LoginProvider.Microsoft));
        }

        return new SampleConfiguration(
            LocalPort: localPort,
            WebhookPath: webhookPath,
            ClientId: clientId,
            ClientSecret: clientSecret,
            SigningSecret: signingSecret,
            UseDevTunnels: useDevTunnels,
            TunnelId: tunnelId,
            LoginProvider: loginProvider);
    }

    private static async Task<DevTunnelsRuntime> StartDevTunnelsAsync(
        SampleConfiguration configuration,
        CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Azure Dev Tunnels walkthrough[/]");
        AnsiConsole.WriteLine();

        DevTunnelsClient client = new(new DevTunnelsClientOptions
        {
            CommandTimeout = TimeSpan.FromSeconds(20),
        });

        DevTunnelCliProbeResult probe = await client.ProbeCliAsync(cancellationToken).ConfigureAwait(false);

        if (!probe.IsInstalled)
        {
            throw new InvalidOperationException(
                "The devtunnel CLI is not installed or could not be found. Install it first, then re-run the sample.");
        }

        AnsiConsole.MarkupLineInterpolated($"[green]CLI found:[/] devtunnel [white]{Markup.Escape(probe.Version?.ToString() ?? "unknown")}[/]");

        await client.EnsureLoggedInAsync(configuration.LoginProvider, cancellationToken).ConfigureAwait(false);

        await client.CreateOrUpdateTunnelAsync(
            configuration.TunnelId,
            new DevTunnelOptions
            {
                Description = "Tiltify.Client.Sample tunnel",
                AllowAnonymous = true,
            },
            cancellationToken).ConfigureAwait(false);

        await client.CreateOrReplacePortAsync(
            configuration.TunnelId,
            configuration.LocalPort,
            new DevTunnelPortOptions
            {
                Protocol = "http",
            },
            cancellationToken).ConfigureAwait(false);

        IDevTunnelHostSession session = await client.StartHostSessionAsync(
            new DevTunnelHostStartOptions
            {
                TunnelId = configuration.TunnelId,
            },
            cancellationToken).ConfigureAwait(false);

        await session.WaitForReadyAsync(cancellationToken).ConfigureAwait(false);

        Uri publicBaseUrl = session.PublicUrl
            ?? throw new InvalidOperationException("The Dev Tunnel host session became ready without a public URL.");

        return new DevTunnelsRuntime(session, publicBaseUrl);
    }

    private static void RenderStartupSummary(SampleConfiguration configuration, string localBaseUrl)
    {
        string localWebhookUrl = CombineUrl(localBaseUrl, configuration.WebhookPath);

        Table table = new Table()
            .RoundedBorder()
            .BorderColor(Color.CornflowerBlue)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Local base URL", $"[white]{Markup.Escape(localBaseUrl)}[/]");
        table.AddRow("Webhook path", $"[white]{Markup.Escape(configuration.WebhookPath)}[/]");
        table.AddRow("Local webhook URL", $"[white]{Markup.Escape(localWebhookUrl)}[/]");
        table.AddRow("Client ID", $"[white]{Markup.Escape(configuration.ClientId)}[/]");
        table.AddRow("Signing secret", "[grey](hidden)[/]");
        table.AddRow("Dev Tunnels enabled", configuration.UseDevTunnels ? "[green]yes[/]" : "[yellow]no[/]");

        AnsiConsole.Write(new Panel(table)
            .Header("[bold]Local runtime[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.CornflowerBlue));
    }

    private static void RenderTunnelSummary(SampleConfiguration configuration, Uri publicBaseUrl)
    {
        string publicWebhookUrl = CombineUrl(publicBaseUrl.ToString().TrimEnd('/'), configuration.WebhookPath);

        Table table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Green)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Tunnel ID", $"[white]{Markup.Escape(configuration.TunnelId)}[/]");
        table.AddRow("Public base URL", $"[white]{Markup.Escape(publicBaseUrl.ToString())}[/]");
        table.AddRow("Public webhook URL", $"[white]{Markup.Escape(publicWebhookUrl)}[/]");

        AnsiConsole.Write(new Panel(table)
            .Header("[bold]Public tunnel[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green));
    }

    private static void RenderUsageInstructions(
        SampleConfiguration configuration,
        string localBaseUrl,
        Uri? publicBaseUrl)
    {
        string localWebhookUrl = CombineUrl(localBaseUrl, configuration.WebhookPath);
        string? publicWebhookUrl = publicBaseUrl is null
            ? null
            : CombineUrl(publicBaseUrl.ToString().TrimEnd('/'), configuration.WebhookPath);

        Rows rows = new(
            new Markup("[bold]Walkthrough[/]"),
            new Text(string.Empty),
            new Markup("1. Start this sample and keep it running."),
            new Markup("2. In the Tiltify dashboard, configure a webhook subscription."),
            new Markup("3. Paste the webhook URL from below into the Tiltify webhook URL field."),
            new Markup("4. Copy the signing secret from Tiltify into this sample when prompted."),
            new Markup("5. Send a test donation or trigger a real event."),
            new Text(string.Empty),
            new Markup($"[grey]Local webhook URL:[/]  [white]{Markup.Escape(localWebhookUrl)}[/]"),
            publicWebhookUrl is not null
                ? new Markup($"[grey]Public webhook URL:[/] [white]{Markup.Escape(publicWebhookUrl)}[/]")
                : new Markup("[grey]Public webhook URL:[/] [yellow](Dev Tunnels disabled)[/]"));

        AnsiConsole.Write(new Panel(rows)
            .Header("[bold]How to use the sample[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue));
    }

    private static async Task RunCommandLoopAsync(
        SampleConfiguration configuration,
        ConcurrentQueue<TiltifyWebhookEvent> receivedEvents,
        DevTunnelsRuntime? devTunnelsRuntime,
        object consoleLock,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.WriteLine();

            string command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Choose an action[/]")
                    .AddChoices(
                        "Show webhook URLs",
                        "Show recent events",
                        "Exit"));

            switch (command)
            {
                case "Show webhook URLs":
                    lock (consoleLock)
                    {
                        string localBase = $"http://127.0.0.1:{configuration.LocalPort}";
                        string localWebhookUrl = CombineUrl(localBase, configuration.WebhookPath);

                        Table table = new Table()
                            .RoundedBorder()
                            .AddColumn("[bold]Endpoint[/]")
                            .AddColumn("[bold]URL[/]");

                        table.AddRow("Local", $"[white]{Markup.Escape(localWebhookUrl)}[/]");

                        if (devTunnelsRuntime is not null)
                        {
                            string publicWebhookUrl = CombineUrl(
                                devTunnelsRuntime.PublicBaseUrl.ToString().TrimEnd('/'),
                                configuration.WebhookPath);
                            table.AddRow("Public", $"[white]{Markup.Escape(publicWebhookUrl)}[/]");
                        }

                        AnsiConsole.Write(table);
                    }
                    break;

                case "Show recent events":
                    lock (consoleLock)
                    {
                        if (receivedEvents.IsEmpty)
                        {
                            AnsiConsole.MarkupLine("[yellow]No events have been received yet.[/]");
                            break;
                        }

                        TiltifyWebhookEvent[] snapshot = [.. receivedEvents];

                        Table table = new Table()
                            .RoundedBorder()
                            .AddColumn("[bold]Type[/]")
                            .AddColumn("[bold]Delivery ID[/]")
                            .AddColumn("[bold]Generated At[/]");

                        foreach (TiltifyWebhookEvent evt in snapshot.TakeLast(20))
                        {
                            table.AddRow(
                                Markup.Escape(evt.EventName),
                                Markup.Escape(evt.DeliveryId),
                                Markup.Escape(evt.GeneratedAt));
                        }

                        AnsiConsole.Write(table);
                    }
                    break;

                case "Exit":
                    return;
            }

            await Task.Yield();
        }
    }

    private static void RenderReceivedEvent(TiltifyWebhookEvent evt)
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow("[bold]Event[/]", Markup.Escape(evt.EventName));
        grid.AddRow("[bold]Delivery[/]", Markup.Escape(evt.DeliveryId));
        grid.AddRow("[bold]Generated[/]", Markup.Escape(evt.GeneratedAt));

        if (evt is TiltifyDonationWebhookEvent donation)
        {
            grid.AddRow("[bold]Donor[/]", Markup.Escape(donation.Data.DonorName ?? "Anonymous"));
            grid.AddRow("[bold]Amount[/]", Markup.Escape($"{donation.Data.Amount?.Value} {donation.Data.Amount?.Currency}"));
            grid.AddRow("[bold]Direct[/]", donation.IsDirect ? "[green]yes[/]" : "[yellow]no (indirect)[/]");

            if (!string.IsNullOrWhiteSpace(donation.Data.DonorComment))
            {
                grid.AddRow("[bold]Comment[/]", Markup.Escape(donation.Data.DonorComment));
            }
        }
        else if (evt is TiltifyFactWebhookEvent fact)
        {
            grid.AddRow("[bold]Fact[/]", Markup.Escape(fact.Data.Name ?? "-"));
            grid.AddRow("[bold]Value[/]", Markup.Escape(fact.Data.Value ?? "-"));
            grid.AddRow("[bold]Active[/]", fact.Data.Active == true ? "[green]yes[/]" : "[yellow]no[/]");
            grid.AddRow("[bold]Direct[/]", fact.IsDirect ? "[green]yes[/]" : "[yellow]no (indirect)[/]");
        }

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold green]Webhook event received[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green));
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        string normalizedBase = baseUrl.TrimEnd('/');
        string normalizedPath = path.StartsWith('/') ? path : "/" + path;
        return normalizedBase + normalizedPath;
    }

    private sealed record SampleConfiguration(
        int LocalPort,
        string WebhookPath,
        string ClientId,
        string ClientSecret,
        string SigningSecret,
        bool UseDevTunnels,
        string TunnelId,
        LoginProvider LoginProvider);

    private sealed class DevTunnelsRuntime(IDevTunnelHostSession session, Uri publicBaseUrl)
    {
        public IDevTunnelHostSession Session { get; } = session;

        public Uri PublicBaseUrl { get; } = publicBaseUrl;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Session.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort shutdown for the sample.
            }
        }
    }
}
