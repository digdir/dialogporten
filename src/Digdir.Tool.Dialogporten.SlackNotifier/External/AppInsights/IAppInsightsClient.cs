using Azure.Core;
using Digdir.Tool.Dialogporten.SlackNotifier.Features.AzureAlertToSlackForwarder;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Digdir.Tool.Dialogporten.SlackNotifier.External.AppInsights;

internal interface IAppInsightsClient
{
    Task<AppInsightsQueryResponseDto[]> QueryAppInsights(AzureAlertDto azureAlertRequest, CancellationToken cancellationToken);
}

internal sealed class AppInsightsClient : IAppInsightsClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenCredential _credentials;

    public AppInsightsClient(HttpClient httpClient, TokenCredential credentials)
    {
        _httpClient = httpClient;
        _credentials = credentials;
    }

    public async Task<AppInsightsQueryResponseDto[]> QueryAppInsights(AzureAlertDto azureAlertRequest, CancellationToken cancellationToken)
    {
        const string appInsightsTokenScope = "https://api.applicationinsights.io";
        var token = await _credentials.GetTokenAsync(new TokenRequestContext([appInsightsTokenScope]), cancellationToken);
        var requests = azureAlertRequest.Data.AlertContext.Condition.AllOf
            .Select(x =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, x.LinkToFilteredSearchResultsAPI);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                return request;
            })
            .Select(_httpClient.SendAsync);
        var responses = await Task.WhenAll(requests);

        foreach (var httpResponseMessage in responses)
        {
            httpResponseMessage.EnsureSuccessStatusCode();
        }

        var typedResponses = await Task.WhenAll(responses.Select(x =>
            x.Content.ReadFromJsonAsync<AppInsightsQueryResponseDto>(cancellationToken: cancellationToken)));
        return typedResponses!;
    }
}
