using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;
using Tiltify.Client.Authentication;
using Tiltify.Client.Models;
using Tiltify.Client.Options;
using Tiltify.Client.Serialization;

namespace Tiltify.Client;

/// <summary>
/// Typed reads of a campaign's public data: the campaign, its polls, rewards, targets and milestones. Covers what the
/// webhooks do not send.
/// </summary>
public interface ITiltifyCampaignApi
{
    /// <summary>The campaign, with what it has raised against its goal; null when it does not exist.</summary>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<TiltifyCampaign?> GetCampaignAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    );

    /// <summary>The campaign's polls with each option's amount raised.</summary>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<IReadOnlyList<TiltifyPoll>> GetPollsAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    );

    /// <summary>One poll with its standings; null when it does not exist.</summary>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="pollId">The poll id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<TiltifyPoll?> GetPollAsync(
        string campaignId,
        string pollId,
        CancellationToken cancellationToken = default
    );

    /// <summary>The campaign's rewards.</summary>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<IReadOnlyList<TiltifyReward>> GetRewardsAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    );

    /// <summary>The campaign's targets with what each has raised.</summary>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<IReadOnlyList<TiltifyTarget>> GetTargetsAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    );

    /// <summary>The campaign's milestones.</summary>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<IReadOnlyList<TiltifyMilestone>> GetMilestonesAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// <see cref="ITiltifyCampaignApi"/> over HTTP. Tiltify's OpenAPI types every list endpoint as an untyped page, so
/// the generated client cannot read polls, rewards, targets or milestones; this reads them with hand-written models
/// and source-generated JSON contracts.
/// </summary>
public sealed class TiltifyCampaignApi : ITiltifyCampaignApi
{
    // Tiltify pages lists; a campaign rarely has more than this many of any of them.
    private const int PageLimit = 100;

    private readonly TiltifyTokenProvider _tokens;
    private readonly HttpClient _http;
    private readonly Uri _baseUri;

    /// <summary>Creates the API over <paramref name="httpClient"/>, authenticated by <paramref name="tokenProvider"/>.</summary>
    /// <param name="tokenProvider">Supplies access tokens.</param>
    /// <param name="options">The API base address.</param>
    /// <param name="httpClient">Sends the requests.</param>
    public TiltifyCampaignApi(
        TiltifyTokenProvider tokenProvider,
        TiltifyClientOptions options,
        HttpClient httpClient
    )
    {
        _tokens = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        ArgumentNullException.ThrowIfNull(options);
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUri = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
    }

    /// <inheritdoc />
    public async Task<TiltifyCampaign?> GetCampaignAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    ) =>
        (
            await GetAsync(
                    Path(campaignId),
                    TiltifyJsonContext.Default.TiltifyItemResponseTiltifyCampaign,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.Data;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TiltifyPoll>> GetPollsAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    ) =>
        (
            await GetAsync(
                    Path(campaignId, "polls"),
                    TiltifyJsonContext.Default.TiltifyListResponseTiltifyPoll,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.Data
        ?? [];

    /// <inheritdoc />
    public async Task<TiltifyPoll?> GetPollAsync(
        string campaignId,
        string pollId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pollId);
        return (
            await GetAsync(
                    $"{Path(campaignId)}/polls/{Uri.EscapeDataString(pollId)}",
                    TiltifyJsonContext.Default.TiltifyItemResponseTiltifyPoll,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.Data;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TiltifyReward>> GetRewardsAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    ) =>
        (
            await GetAsync(
                    Path(campaignId, "rewards"),
                    TiltifyJsonContext.Default.TiltifyListResponseTiltifyReward,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.Data
        ?? [];

    /// <inheritdoc />
    public async Task<IReadOnlyList<TiltifyTarget>> GetTargetsAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    ) =>
        (
            await GetAsync(
                    Path(campaignId, "targets"),
                    TiltifyJsonContext.Default.TiltifyListResponseTiltifyTarget,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.Data
        ?? [];

    /// <inheritdoc />
    public async Task<IReadOnlyList<TiltifyMilestone>> GetMilestonesAsync(
        string campaignId,
        CancellationToken cancellationToken = default
    ) =>
        (
            await GetAsync(
                    Path(campaignId, "milestones"),
                    TiltifyJsonContext.Default.TiltifyListResponseTiltifyMilestone,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.Data
        ?? [];

    private static string Path(string campaignId, string? list = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campaignId);
        string campaign = $"api/public/campaigns/{Uri.EscapeDataString(campaignId)}";
        return list is null ? campaign : $"{campaign}/{list}?limit={PageLimit}";
    }

    private async Task<T?> GetAsync<T>(
        string relative,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken
    )
    {
        string token = await _tokens.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        using HttpRequestMessage request = new(HttpMethod.Get, new Uri(_baseUri, relative));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using HttpResponseMessage response = await _http
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        _ = response.EnsureSuccessStatusCode();
        Stream body = await response
            .Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (body.ConfigureAwait(false))
        {
            return await System
                .Text.Json.JsonSerializer.DeserializeAsync(body, typeInfo, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
