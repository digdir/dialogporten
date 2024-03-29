﻿using System.Net;
using System.Security.Claims;
using System.Text;
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

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class AltinnAuthorizationClient : IAltinnAuthorization
{
    private readonly HttpClient _httpClient;
    private readonly IUser _user;
    private readonly IDialogDbContext _db;
    private readonly ILogger _logger;

    public AltinnAuthorizationClient(
        HttpClient client,
        IUser user,
        IDialogDbContext db,
        ILogger<AltinnAuthorizationClient> logger)
    {
        _httpClient = client ?? throw new ArgumentNullException(nameof(client));
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity,
        CancellationToken cancellationToken = default) =>
        await PerformDialogDetailsAuthorization(new DialogDetailsAuthorizationRequest
        {
            Claims = _user.GetPrincipal().Claims.ToList(),
            ServiceResource = dialogEntity.ServiceResource,
            DialogId = dialogEntity.Id,
            Party = dialogEntity.Party,
            AltinnActions = dialogEntity.GetAltinnActions()
        }, cancellationToken);

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> serviceResources,
        string? endUserId,
        CancellationToken cancellationToken = default)
    {
        var claims = GetOrCreateClaimsBasedOnEndUserId(endUserId);
        return await PerformNonScalableDialogSearchAuthorization(new DialogSearchAuthorizationRequest
        {
            Claims = claims,
            ConstraintParties = constraintParties,
            ConstraintServiceResources = serviceResources
        }, cancellationToken);
    }

    private async Task<DialogSearchAuthorizationResult> PerformNonScalableDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
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
        var xamlJsonResponse = await SendRequest(xacmlJsonRequest, cancellationToken);
        return DecisionRequestHelper.NonScalable.CreateDialogSearchResponse(xacmlJsonRequest, xamlJsonResponse);
    }

    private async Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken)
    {
        var xacmlJsonRequest = DecisionRequestHelper.CreateDialogDetailsRequest(request);
        var xamlJsonResponse = await SendRequest(xacmlJsonRequest, cancellationToken);
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

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private async Task<XacmlJsonResponse?> SendRequest(XacmlJsonRequestRoot xacmlJsonRequest, CancellationToken cancellationToken)
    {
        const string apiUrl = "authorization/api/v1/authorize";
        var requestJson = JsonSerializer.Serialize(xacmlJsonRequest, _serializerOptions);
        _logger.LogDebug("Generated XACML request: {RequestJson}", requestJson);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, httpContent, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation(
                "AltinnAuthorizationClient.SendRequest failed with non-successful status code: {StatusCode} {Response}",
                response.StatusCode, errorResponse);

            return null;
        }

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<XacmlJsonResponse>(responseData, _serializerOptions);
    }
}
