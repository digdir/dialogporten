using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Authorization;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class AltinnAuthorizationClient : IAltinnAuthorization
{

    private const string SubscriptionKeyHeaderName = "Ocp-Apim-Subscription-Key";
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Initialize a new instance of the <see cref="AuthorizationApiClient"/> class.
    /// </summary>
    /// <param name="client">A HttpClient provided by the built in HttpClientFactory.</param>
    /// <param name="platformSettings">The current platform settings</param>
    /// <param name="logger">A logger provided by the built in LoggerFactory.</param>
    public AltinnAuthorizationClient(HttpClient client, ILogger<AltinnAuthorizationClient> logger)
    {
        _httpClient = client;
        _logger = logger;
    }

    public Task<DialogSearchAuthorizationResponse> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
    {
        // TODO
        // - Implement as per https://github.com/digdir/dialogporten/issues/249
        // - Note that either ServiceResource or Party is always supplied in the request.
        // - Whether or not to populate ResourcesForParties or PartiesForResources depends on which one is supplied in the request.
        // - The user is also always authorized for its own dialogs, which might be an optimization

        throw new NotImplementedException();
    }

    public async Task<DialogDetailsAuthorizationResponse> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken)
    {
        var xamlJsonResponse = await SendRequest(DecisionRequestHelper.CreateDialogDetailsRequest(request));
        return DecisionRequestHelper.CreateDialogDetailsResponse(xamlJsonResponse);
    }

    /// <summary>
    /// Method for performing authorization.
    /// </summary>
    /// <param name="xacmlJsonRequest">An authorization request.</param>
    /// <returns>The result of the authorization request.</returns>
    public async Task<XacmlJsonResponse?> SendRequest(XacmlJsonRequestRoot xacmlJsonRequest)
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
