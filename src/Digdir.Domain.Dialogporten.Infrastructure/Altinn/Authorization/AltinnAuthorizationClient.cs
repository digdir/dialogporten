﻿using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class AltinnAuthorizationClient : IAltinnAuthorization
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public AltinnAuthorizationClient(HttpClient client, ILogger<AltinnAuthorizationClient> logger)
    {
        _httpClient = client;
        _logger = logger;
    }

    public async Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default) =>
        await PerformDialogDetailsAuthorization(new DialogDetailsAuthorizationRequest
        {
            ClaimsPrincipal = claimsPrincipal,
            ServiceResource = dialogEntity.ServiceResource,
            DialogId = dialogEntity.Id,
            Party = dialogEntity.Party,
            Actions = ToAuthorizationActions(dialogEntity)
        }, cancellationToken);

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> serviceResources,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default) =>
        await PerformDialogSearchAuthorization(new DialogSearchAuthorizationRequest
        {
            ClaimsPrincipal = claimsPrincipal,
            ConstraintParties = constraintParties,
            ConstraintServiceResources = serviceResources
        }, cancellationToken);

    public Task<DialogSearchAuthorizationResult> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
    {
        // TODO
        // - Implement as per https://github.com/digdir/dialogporten/issues/249
        // - Note that either ServiceResource or Party is always supplied in the request.
        // - Whether or not to populate ResourcesForParties or PartiesForResources depends on which one is supplied in the request.
        // - The user is also always authorized for its own dialogs, which might be an optimization

        throw new NotImplementedException();
    }

    public async Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken)
    {
        var xacmlJsonRequest = DecisionRequestHelper.CreateDialogDetailsRequest(request);
        var xamlJsonResponse = await SendRequest(xacmlJsonRequest);
        return DecisionRequestHelper.CreateDialogDetailsResponse(xacmlJsonRequest, xamlJsonResponse);
    }

    private static Dictionary<string, List<string>> ToAuthorizationActions(DialogEntity dialogEntity) =>
        dialogEntity.ApiActions
            .Select(x => new { x.Action, x.AuthorizationAttribute })
            .Concat(dialogEntity.GuiActions
                .Select(x => new { x.Action, x.AuthorizationAttribute }))
            .Concat(dialogEntity.Elements
                .Where(x => x.AuthorizationAttribute is not null)
                .Select(x => new { Action = DialogDetailsAuthorizationRequest.ElementReadAction, x.AuthorizationAttribute }))
            .GroupBy(x => x.Action)
            .ToDictionary(
                keySelector: x => x.Key,
                elementSelector: x => x
                    .Select(x => x.AuthorizationAttribute ?? DialogDetailsAuthorizationRequest.MainResource)
                    .Distinct()
                    .ToList()
            );

    private async Task<XacmlJsonResponse?> SendRequest(XacmlJsonRequestRoot xacmlJsonRequest)
    {
        XacmlJsonResponse? xacmlJsonResponse = null;
        var apiUrl = "authorization/api/v1/Decision";
        var requestJson = JsonSerializer.Serialize(xacmlJsonRequest);
        _logger.LogDebug("Generated XACML request: {RequestJson}", requestJson);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var response = await _httpClient.PostAsync(apiUrl, httpContent);
        stopWatch.Stop();
        var ts = stopWatch.Elapsed;
        _logger.LogInformation("Authorization PDP time elapsed: {ElapsedMs}ms", ts.TotalMilliseconds);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            xacmlJsonResponse = JsonSerializer.Deserialize<XacmlJsonResponse>(responseData);
        }
        else
        {
            _logger.LogInformation(
                "AltinnAuthorizationClient.SendRequest failed with non-successful status code: {StatusCode} {Response}",
                response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        return xacmlJsonResponse;
    }
}
