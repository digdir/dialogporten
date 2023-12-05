using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
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
            ClaimsPrincipal = _user.GetPrincipal(),
            ServiceResource = dialogEntity.ServiceResource,
            DialogId = dialogEntity.Id,
            Party = dialogEntity.Party,
            AuthorizationAttributesByActions = ToAuthorizationActions(dialogEntity)
        }, cancellationToken);

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> serviceResources,
        CancellationToken cancellationToken = default) =>
        await PerformNonScalableDialogSearchAuthorization(new DialogSearchAuthorizationRequest
        {
            ClaimsPrincipal = _user.GetPrincipal(),
            ConstraintParties = constraintParties,
            ConstraintServiceResources = serviceResources
        }, cancellationToken);

    private async Task<DialogSearchAuthorizationResult> PerformNonScalableDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
    {
        /*
         * This is a preliminary implementation as per https://github.com/digdir/dialogporten/issues/249
         *
         * This scales horribly, as it will depend on building a MultiDecisionRequest with all possible combinations of parties and resources, but given
         * the small number of parties and resources during the PoC period, this is hopefully not a huge problem.
         *
         * The algorithm is as follows:
         * - Get all distinct parties and resources from the database, if not already constrained by the request
         * - Build a MultiDecisionRequest with all resources, all parties and the action "read"
         * - Send the request to the Altinn Decision API
         * - Build a DialogSearchAuthorizationResult from the response
         */

        if (request.ConstraintParties.Count == 0)
        {
            request.ConstraintParties = await _db.Dialogs
                .Select(dialog => dialog.Party)
                .Distinct()
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (request.ConstraintServiceResources.Count == 0)
        {
            request.ConstraintServiceResources = await _db.Dialogs
                .Select(x => x.ServiceResource)
                .Distinct().ToListAsync(cancellationToken: cancellationToken);
        }

        var xacmlJsonRequest = DecisionRequestHelper.NonScalable.CreateDialogSearchRequest(request);
        var xamlJsonResponse = await SendRequest(xacmlJsonRequest);
        return DecisionRequestHelper.NonScalable.CreateDialogSearchResponse(xacmlJsonRequest, xamlJsonResponse);
    }

    private async Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken _)
    {
        var xacmlJsonRequest = DecisionRequestHelper.CreateDialogDetailsRequest(request);
        var xamlJsonResponse = await SendRequest(xacmlJsonRequest);
        return DecisionRequestHelper.CreateDialogDetailsResponse(xacmlJsonRequest, xamlJsonResponse);
    }

    private static Dictionary<string, List<string>> ToAuthorizationActions(DialogEntity dialogEntity)
    {
        // Get all resources grouped by action defined on the dialogEntity, including both
        // apiActions and guiActions, as well as dialog elements with an authorization attribute.
        var actions = dialogEntity.ApiActions
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

        // We always need to check if the user can read the main resource
        if (!actions.ContainsKey(Constants.ReadAction))
        {
            actions.Add(Constants.ReadAction, new List<string>());
        }

        actions[Constants.ReadAction].Add(Constants.MainResource);
        return actions;
    }

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private async Task<XacmlJsonResponse?> SendRequest(XacmlJsonRequestRoot xacmlJsonRequest)
    {
        const string apiUrl = "authorization/api/v1/Decision";
        var requestJson = JsonSerializer.Serialize(xacmlJsonRequest, _serializerOptions);
        _logger.LogDebug("Generated XACML request: {RequestJson}", requestJson);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

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
        return JsonSerializer.Deserialize<XacmlJsonResponse>(responseData, _serializerOptions);
    }
}
