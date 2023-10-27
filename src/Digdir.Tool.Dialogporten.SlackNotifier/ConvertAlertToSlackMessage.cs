using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Data;
using Azure.Identity;
using Azure.Core;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Generic;
using System;

namespace Digdir.Tool.Dialogporten.SlackNotifier;

public static class ConvertAlertToSlackMessage
{
    private static readonly DefaultAzureCredential _credentials = new();
    private static readonly HttpClient _httpClient = new();
    private const string encodedKqlQuery = "H4sIAAAAAAAAA0utSE4tKMnMzyvmqlHIL0pJLVJIqlQoycxNLS5JzC1QSEktTgYAbgDhFSQAAAA%253D/timespan/P1D";

    [FunctionName("ConvertAlertToSlackMessage")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        using var streamReader = new StreamReader(req.Body);
        var azureAlertRequest = JsonConvert.DeserializeObject<AzureAlert>(await streamReader.ReadToEndAsync());
        var responses = await QueryAppInsights(azureAlertRequest);

        var link = azureAlertRequest.Data.AlertContext.Condition.AllOf
            .Select(x => x.LinkToSearchResultsUI)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        link = RemoveQuery(link) + encodedKqlQuery;

        var asciiTables = responses.SelectMany(x => x.Tables)
            .Select(table => Enumerable.Empty<List<object>>()
                .Append(table.Columns.Select(x => (object)x.Name))
                .Concat(table.Rows)
                .ToAsciiTable());

        await _httpClient.PostAsJsonAsync(Environment.GetEnvironmentVariable("SlackWebhookUrl"), new
        {
            exceptionReport = string.Join(Environment.NewLine, asciiTables),
            link = link
        });

        return new OkResult();
    }

    private static async Task<AppInsightsQueryResponse[]> QueryAppInsights(AzureAlert azureAlertRequest)
    {
        var token = await _credentials.GetTokenAsync(new TokenRequestContext(new[] { "https://api.applicationinsights.io" }));
        var requests = azureAlertRequest.Data.AlertContext.Condition.AllOf
            .Select(x =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, x.LinkToFilteredSearchResultsAPI);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                return request;
            })
            .Select(x => _httpClient.SendAsync(x));
        var responses = await Task.WhenAll(requests);
        var typedResponses = await Task.WhenAll(responses.Select(x => x.Content.ReadFromJsonAsync<AppInsightsQueryResponse>()));
        return typedResponses;
    }

    public static string RemoveQuery(string inputUrl)
    {
        int index = inputUrl.IndexOf("q/");

        if (index >= 0)
        {
            return inputUrl.Substring(0, index + 2); // Include "q/"
        }

        return inputUrl; // "q/" not found, return the original URL
    }
}