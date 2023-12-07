using Azure.Core;
using Azure.Identity;
using ILogger = Serilog.ILogger;

namespace Digdir.Tool.Dialogporten.MigrationVerifier;

public static class MigrationVerifier
{
    private const int SecondsBetweenRetries = 2;
    private const int MaxRetries = 30;
    private static readonly string[] Scopes = { "https://management.azure.com/.default" };
    private static readonly HttpClient _httpClient = new();
    private static void Sleep() => Thread.Sleep(TimeSpan.FromSeconds(SecondsBetweenRetries));

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

        var retries = 0;
        while (retries++ < MaxRetries)
        {
            try
            {
                var containerAppJobExecutions = await _httpClient.GetFromJsonAsync<ContainerAppJobExecutions>(executionsUrl);

                if (containerAppJobExecutions is null)
                {
                    logger.Information("### MigrationJob/Executions not found, retrying in {SecondsBetweenRetries} seconds",
                        SecondsBetweenRetries);
                    Sleep();
                    continue;
                }

                var executionsForGitSha = containerAppJobExecutions.Executions
                    .Where(x => x.Properties.Template.Containers.Any(y => y.Image.Contains(gitSha)))
                    .ToList();

                if (executionsForGitSha.Count == 0)
                {
                    logger.Information("### No job executions found for gitSha {GitSha}, retrying in {SecondsBetweenRetries} seconds",
                        gitSha, SecondsBetweenRetries);
                    Sleep();
                    continue;
                }

                if (executionsForGitSha.Any(x => x.Properties.Status == "Succeeded"))
                {
                    logger.Information("### Migration execution for gitSha {GitSha} successful ###", gitSha);
                    return;
                }

                logger.Information("### No successful job executions found for gitSha {GitSha}, retrying in {SecondsBetweenRetries} seconds",
                    gitSha, SecondsBetweenRetries);
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
