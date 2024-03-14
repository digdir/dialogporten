using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Digdir.Tool.Dialogporten.SlackNotifier.External.Slack;
using Digdir.Tool.Dialogporten.SlackNotifier.External.AppInsights;
using System.Diagnostics;
using Digdir.Tool.Dialogporten.SlackNotifier.Common;

namespace Digdir.Tool.Dialogporten.SlackNotifier.Features.AzureAlertToSlackForwarder;

internal sealed class ForwardAlertToSlack
{
    private readonly ISlackClient _slack;
    private readonly IAppInsightsClient _appInsights;

    public ForwardAlertToSlack(ISlackClient slack, IAppInsightsClient appInsights)
    {
        _slack = slack;
        _appInsights = appInsights;
    }

    [Function("ForwardAlertToSlack")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var azureAlertRequest = await req.ReadFromJsonAsync<AzureAlertDto>(cancellationToken) ?? throw new UnreachableException();
        var appInsightsResponses = await _appInsights.QueryAppInsights(azureAlertRequest, cancellationToken);

        await _slack.SendAsync(new SlackRequestDto
        {
            ExceptionReport = appInsightsResponses.ToAsciiTableExceptionReport(),
            Link = azureAlertRequest.ToQueryLink()
        }, cancellationToken);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
