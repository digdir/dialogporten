using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using ILogger = Serilog.ILogger;

namespace Digdir.Tool.Dialogporten.MigrationVerifier;

public static class MigrationVerifier
{
    private const int SecondsBetweenRetries = 2;
    private const int MaxRetries = 300;
    private static readonly string[] Scopes = { "https://management.azure.com/.default" };
    private static readonly HttpClient _httpClient = new();
    private static async Task Sleep() => await Task.Delay(TimeSpan.FromSeconds(SecondsBetweenRetries));

    public static async Task Verify(ILogger logger)
    {
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

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");
        _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        logger.Information("### Executions URL: {ExecutionsUrl} ###", executionsUrl);

        var retries = 0;
        while (retries++ < MaxRetries)
        {
            await Sleep();

            try
            {
                var containerAppJobExecutions = await _httpClient.GetFromJsonAsync<ContainerAppJobExecutions>(executionsUrl);

                if (containerAppJobExecutions is null)
                {
                    // The container app job might not exist yet because of timing in the IaC pipeline
                    logger.Information("### MigrationJob/Executions not found, retrying in {SecondsBetweenRetries} seconds",
                        SecondsBetweenRetries);
                    continue;
                }

                logger.Information("### Found {ExecutionsCount} executions for job {JobName} ###",
                    containerAppJobExecutions.Executions.Count, jobName);

                var executionForGitSha = containerAppJobExecutions.Executions
                    .FirstOrDefault(x => x.Properties.Template.Containers.Any(y => y.Image.Contains(gitSha)));

                if (executionForGitSha == null)
                {
                    // The specific execution might not exist yet because of timing in the IaC pipeline
                    logger.Information("### No job executions found for gitSha {GitSha}, retrying in {SecondsBetweenRetries} seconds",
                        gitSha, SecondsBetweenRetries);
                    continue;
                }

                if (executionForGitSha.Properties.Status == "Succeeded")
                {
                    logger.Information("### Migration execution for gitSha {GitSha} successful ###", gitSha);
                    return;
                }

                logger.Information("### Migration execution status for gitSha {GitSha} is '{Status}', retrying in {SecondsBetweenRetries} seconds",
                    gitSha, executionForGitSha.Properties.Status, SecondsBetweenRetries);
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
