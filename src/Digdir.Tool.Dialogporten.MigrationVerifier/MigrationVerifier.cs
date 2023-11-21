using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.Resources;
using ILogger = Serilog.ILogger;

namespace Digdir.Tool.Dialogporten.MigrationVerifier;

public static class MigrationVerifier
{
    private static readonly ArmClient _client = new(new DefaultAzureCredential());

    public static async Task Verify(ILogger logger)
    {
        const int secondsBetweenRetries = 2;
        const int maxRetries = 30;

        var subscription = await GetSubscription();
        var resourceGroupName = GetResourceGroupName();
        var resourceGroup = GetResourceGroup(subscription, resourceGroupName);
        var gitSha = GetGitSha();

        var jobName = GetMigrationJobName();

        var jobCompleted = false;
        var retries = 0;

        while (!jobCompleted && retries++ < maxRetries)
        {
            try
            {
                retries++;
                logger.Information("### Sleeping {SecondsBetweenRetries} seconds before checking for job executions",
                    secondsBetweenRetries);
                Thread.Sleep(TimeSpan.FromSeconds(secondsBetweenRetries));

                var migrationJob = await resourceGroup.GetContainerAppJobAsync(jobName);
                if (migrationJob is null)
                {
                    // The container app job might not exist yet because of timing in the IaC pipeline
                    logger.Information("### Migration job not found, retrying in {SecondsBetweenRetries} seconds",
                        secondsBetweenRetries);
                    continue;
                }

                var executions = migrationJob.Value.GetContainerAppJobExecutions();
                var executionsForGitSha = executions
                    .Where(x => x.Data.Template.Containers[0].Image.Contains(gitSha))
                    .ToList();

                if (executionsForGitSha.Count == 0)
                {
                    // The specific execution might not exist yet because of timing in the IaC pipeline
                    logger.Information(
                        "### Migration job executions for git sha {GitSha} not found, retrying in {SecondsBetweenRetries} seconds",
                        gitSha[..8], secondsBetweenRetries);
                }

                logger.Information("### Found {NumberOfExecutions} job executions for git sha {GitSha}",
                    executionsForGitSha.Count, gitSha[..8]);
                if (executionsForGitSha.Exists(x => x.Data.Status == "Succeeded"))
                {
                    jobCompleted = true;
                }
                else
                {
                    logger.Information(
                        "### No executions with status Succeeded for git sha {GitSha}, retrying in {SecondsBetweenRetries} seconds",
                        gitSha[..8], secondsBetweenRetries);
                }
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

    private static ResourceGroupResource GetResourceGroup(ArmResource subscription, string resourceGroupName)
    {
        var resourceGroupId = ResourceGroupResource
            .CreateResourceIdentifier(subscription.Id.SubscriptionId, resourceGroupName);
        var resourceGroup = _client.GetResourceGroupResource(resourceGroupId);

        ArgumentNullException.ThrowIfNull(resourceGroup);
        return resourceGroup;
    }

    private static string GetResourceGroupName()
    {
        var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME");
        ArgumentException.ThrowIfNullOrEmpty(resourceGroupName);
        return resourceGroupName;
    }

    private static async Task<SubscriptionResource> GetSubscription()
    {
        var subscription = await _client.GetDefaultSubscriptionAsync();
        ArgumentNullException.ThrowIfNull(subscription);
        return subscription;
    }
}
