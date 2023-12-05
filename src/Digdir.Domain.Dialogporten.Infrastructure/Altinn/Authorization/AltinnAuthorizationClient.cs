using System.Net;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.Extensions.Logging;
using SerilogTimings;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class AltinnAuthorizationClient : IAltinnAuthorization
{
    private readonly HttpClient _httpClient;
    private readonly IUser _user;
    private readonly ILogger _logger;

    public AltinnAuthorizationClient(
        HttpClient client,
        IUser user,
        ILogger<AltinnAuthorizationClient> logger)
    {
        _httpClient = client;
        _user = user;
        _logger = logger;
    }

    public async Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity,
        CancellationToken cancellationToken = default) =>
        await PerformDialogDetailsAuthorization(new DialogDetailsAuthorizationRequest
        {
            ClaimsPrincipal = _user.GetPrincipal(),
            ServiceResource = dialogEntity.ServiceResource,
            DialogId = dialogEntity.Id,
            Party = dialogEntity.Party,
            Actions = ToAuthorizationActions(dialogEntity)
        }, cancellationToken);

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> serviceResources,
        CancellationToken cancellationToken = default) =>
        await PerformDialogSearchAuthorization(new DialogSearchAuthorizationRequest
        {
            ClaimsPrincipal = _user.GetPrincipal(),
            ConstraintParties = constraintParties,
            ConstraintServiceResources = serviceResources
        }, cancellationToken);

    private Task<DialogSearchAuthorizationResult> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
    {
        // TODO
        // - Implement as per https://github.com/digdir/dialogporten/issues/249
        // - Note that either ServiceResource or Party is always supplied in the request.
        // - Whether or not to populate ResourcesForParties or PartiesForResources depends on which one is supplied in the request.
        // - The user is also always authorized for its own dialogs, which might be an optimization

        throw new NotImplementedException();
    }

    private async Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken _)
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
                .Select(x => new { Action = Constants.ElementReadAction, x.AuthorizationAttribute }))
            .GroupBy(x => x.Action)
            .ToDictionary(
                keySelector: x => x.Key,
                elementSelector: x => x
                    .Select(x => x.AuthorizationAttribute ?? Constants.MainResource)
                    .Distinct()
                    .ToList()
            );

    private async Task<XacmlJsonResponse?> SendRequest(XacmlJsonRequestRoot xacmlJsonRequest)
    {
        const string apiUrl = "authorization/api/v1/Decision";
        var requestJson = JsonSerializer.Serialize(xacmlJsonRequest);
        _logger.LogDebug("Generated XACML request: {RequestJson}", requestJson);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using (Operation.Time("Authorization PDP request to {ApiUrl}", apiUrl))
        {
            var response = await _httpClient.PostAsync(apiUrl, httpContent);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(
                    "AltinnAuthorizationClient.SendRequest failed with non-successful status code: {StatusCode} {Response}",
                    response.StatusCode, errorResponse);

                return null;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<XacmlJsonResponse>(responseData);
        }
    }
}
