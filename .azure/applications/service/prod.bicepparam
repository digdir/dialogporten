using './main.bicep'

param environment = 'prod'
param location = 'norwayeast'
param imageTag = readEnvironmentVariable('IMAGE_TAG')
param revisionSuffix = readEnvironmentVariable('REVISION_SUFFIX')

// secrets
param environmentKeyVaultName = readEnvironmentVariable('AZURE_ENVIRONMENT_KEY_VAULT_NAME')
param containerAppEnvironmentName = readEnvironmentVariable('AZURE_CONTAINER_APP_ENVIRONMENT_NAME')
param appInsightConnectionString = readEnvironmentVariable('AZURE_APP_INSIGHTS_CONNECTION_STRING')
param appConfigurationName = readEnvironmentVariable('AZURE_APP_CONFIGURATION_NAME')
