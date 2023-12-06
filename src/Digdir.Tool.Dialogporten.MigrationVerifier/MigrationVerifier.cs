using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using ILogger = Serilog.ILogger;

namespace Digdir.Tool.Dialogporten.MigrationVerifier;

public static class MigrationVerifier
{
    private static readonly string[] Scopes = { "https://management.azure.com/.default" };
    public static async Task Verify(ILogger logger)
    {
        const int secondsBetweenRetries = 2;
        const int maxRetries = 30;

        var subscriptionId = GetSubscriptionId();
        var resourceGroupName = GetResourceGroupName();
        var gitSha = GetGitSha();
        var jobName = GetMigrationJobName();

        var credentials = new DefaultAzureCredential();
        var tokenRequestContext = new TokenRequestContext(Scopes);
        var tokenResult = await credentials.GetTokenAsync(tokenRequestContext);

        var executionsUrl =
            $"https://management.azure.com/subscriptions/" +
            $"{subscriptionId}/resourceGroups/{resourceGroupName}/" +
            $"providers/Microsoft.App/jobs/{jobName}/executions?api-version=2023-05-01";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Remove("Authorization");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");

        var retries = 0;
        while (retries++ < maxRetries)
        {
            try
            {
                var executionsResponse = await httpClient.GetAsync(executionsUrl);
                var executionsResponseContent = await executionsResponse.Content.ReadAsStringAsync();
                var containerAppJobExecutions = JsonSerializer.Deserialize<ContainerAppJobExecutions>(executionsResponseContent);

                if (containerAppJobExecutions is null)
                {
                    // log/sleep
                    continue;
                }

                var executionsForGitSha = containerAppJobExecutions.Executions
                    .Where(x => x.Properties.Template.Containers.Any(y => y.Image.Contains(gitSha)))
                    .ToList();

                if (executionsForGitSha.Count == 0)
                {
                    // log/sleep
                    continue;
                }

                if (executionsForGitSha.Any(x => x.Properties.Status == "Succeeded"))
                {
                    // log
                    return;
                }

                logger.Information("### Sleeping {SecondsBetweenRetries} seconds before checking for job executions",
                    secondsBetweenRetries);
            }
            catch (Exception e)
            {
                logger.Error(e, "### MIGRATION JOB VERIFICATION EXCEPTION: {EMessage} ###", e.Message);
                throw;
            }
        }

        logger.Error("### MIGRATION JOB VERIFICATION FAILED: Timeout ###");
        throw new TimeoutException();
    }

    private static string GetMigrationJobName()
    {
        var migrationJobName = Environment.GetEnvironmentVariable("MIGRATION_JOB_NAME");
        ArgumentException.ThrowIfNullOrEmpty(migrationJobName);
        return migrationJobName;
    }

    private static string GetGitSha()
    {
        var gitSha = Environment.GetEnvironmentVariable("GIT_SHA");
        ArgumentException.ThrowIfNullOrEmpty(gitSha);
        return gitSha;
    }

    private static string GetResourceGroupName()
    {
        var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME");
        ArgumentException.ThrowIfNullOrEmpty(resourceGroupName);
        return resourceGroupName;
    }

    private static string GetSubscriptionId()
    {
        var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        return subscriptionId;
    }
}
