using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Parties.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;
internal sealed class AltinnAuthorizationClient : IAltinnAuthorization
{
    private const string AuthorizeUrl = "authorization/api/v1/authorize";
    private const string AuthorizedPartiesUrl = "/accessmanagement/api/v1/resourceowner/authorizedparties?includeAltinn2=true";

    private readonly HttpClient _httpClient;
    private readonly IFusionCache _cache;
    private readonly IUser _user;
    private readonly IDialogDbContext _db;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public AltinnAuthorizationClient(
        HttpClient client,
        IFusionCacheProvider cacheProvider,
        IUser user,
        IDialogDbContext db,
        ILogger<AltinnAuthorizationClient> logger)
    {
        _httpClient = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(Authorization)) ?? throw new ArgumentNullException(nameof(cacheProvider));
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity,
        CancellationToken cancellationToken = default)
    {
        var request = new DialogDetailsAuthorizationRequest
        {
            Claims = _user.GetPrincipal().Claims.ToList(),
            ServiceResource = dialogEntity.ServiceResource,
            DialogId = dialogEntity.Id,
            Party = dialogEntity.Party,
            AltinnActions = dialogEntity.GetAltinnActions()
        };

        return await _cache.GetOrSetAsync(request.GenerateCacheKey(), async token
            => await PerformDialogDetailsAuthorization(request, token), token: cancellationToken);
    }

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> serviceResources,
        string? endUserId,
        CancellationToken cancellationToken = default)
    {
        var claims = GetOrCreateClaimsBasedOnEndUserId(endUserId);
        var request = new DialogSearchAuthorizationRequest
        {
            Claims = claims,
            ConstraintParties = constraintParties,
            ConstraintServiceResources = serviceResources
        };

        return await _cache.GetOrSetAsync(request.GenerateCacheKey(), async token
            => await PerformNonScalableDialogSearchAuthorization(request, token), token: cancellationToken);
    }

    public async Task<AuthorizedPartiesResult> GetAuthorizedParties(IPartyIdentifier authenticatedParty,
        CancellationToken cancellationToken = default)
    {
        var authorizedPartiesRequest = new AuthorizedPartiesRequest(authenticatedParty);
        return await _cache.GetOrSetAsync(authorizedPartiesRequest.GenerateCacheKey(), async token
            => await PerformAuthorizedPartiesRequest(authorizedPartiesRequest, token), token: cancellationToken);
    }

    private async Task<AuthorizedPartiesResult> PerformAuthorizedPartiesRequest(AuthorizedPartiesRequest authorizedPartiesRequest,
        CancellationToken token)
    {
        var authorizedPartiesDto = await SendAuthorizedPartiesRequest(authorizedPartiesRequest, token);
        return AuthorizedPartiesHelper.CreateAuthorizedPartiesResult(authorizedPartiesDto);
    }

    private async Task<DialogSearchAuthorizationResult> PerformNonScalableDialogSearchAuthorization(
        DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
    {
        /*
         * This is a preliminary implementation as per https://github.com/digdir/dialogporten/issues/249
         *
         * This scales pretty horribly, O(n*m), as it will depend on building a MultiDecisionRequest with all possible combinations of parties and resources, but given
         * the small number of parties and resources during the PoC period, this is hopefully not a huge problem.
         *
         * The algorithm is as follows:
         * - Get all distinct parties and resources from the database, except for the one of which that's constrained by the request
         * - Build a MultiDecisionRequest with all resources, all parties and the action "read"
         * - Send the request to the Altinn Decision API
         * - Build a DialogSearchAuthorizationResult from the response
         */

        if (request.ConstraintParties.Count == 0)
        {
            request.ConstraintParties = await _db.Dialogs
                .Where(dialog => request.ConstraintServiceResources.Contains(dialog.ServiceResource))
                .Select(dialog => dialog.Party)
                .Distinct()
                .Take(20) // Limit to 20 parties to limit request size
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (request.ConstraintServiceResources.Count == 0)
        {
            request.ConstraintServiceResources = await _db.Dialogs
                .Where(dialog => request.ConstraintParties.Contains(dialog.Party))
                .Select(x => x.ServiceResource)
                .Distinct()
                .Take(20) // Limit to 20 resources to limit request size
                .ToListAsync(cancellationToken: cancellationToken);
        }

        var xacmlJsonRequest = DecisionRequestHelper.NonScalable.CreateDialogSearchRequest(request);
        var xamlJsonResponse = await SendPdpRequest(xacmlJsonRequest, cancellationToken);
        return DecisionRequestHelper.NonScalable.CreateDialogSearchResponse(xacmlJsonRequest, xamlJsonResponse);
    }

    private async Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(
        DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken)
    {
        var xacmlJsonRequest = DecisionRequestHelper.CreateDialogDetailsRequest(request);
        var xamlJsonResponse = await SendPdpRequest(xacmlJsonRequest, cancellationToken);
        return DecisionRequestHelper.CreateDialogDetailsResponse(request.AltinnActions, xamlJsonResponse);
    }

    private List<Claim> GetOrCreateClaimsBasedOnEndUserId(string? endUserId)
    {
        List<Claim> claims = [];
        if (endUserId is not null && PartyIdentifier.TryParse(endUserId, out var partyIdentifier))
        {
            claims.Add(new Claim(partyIdentifier.Prefix(), partyIdentifier.Id));
        }
        else
        {
            claims.AddRange(_user.GetPrincipal().Claims);
        }

        return claims;
    }

    private async Task<XacmlJsonResponse?> SendPdpRequest(
        XacmlJsonRequestRoot xacmlJsonRequest, CancellationToken cancellationToken) =>
        await SendRequest<XacmlJsonResponse>(
            AuthorizeUrl, xacmlJsonRequest, cancellationToken);

    private async Task<List<AuthorizedPartiesResultDto>?> SendAuthorizedPartiesRequest(
        AuthorizedPartiesRequest authorizedPartiesRequest, CancellationToken cancellationToken) =>
        await SendRequest<List<AuthorizedPartiesResultDto>>(
            AuthorizedPartiesUrl, authorizedPartiesRequest, cancellationToken);

    private async Task<T?> SendRequest<T>(string url, object request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Authorization request to {Url}: {RequestJson}", url, JsonSerializer.Serialize(request, SerializerOptions));
        return await _httpClient.PostAsJsonEnsuredAsync<T>(url, request, serializerOptions: SerializerOptions, cancellationToken: cancellationToken);
    }
}
