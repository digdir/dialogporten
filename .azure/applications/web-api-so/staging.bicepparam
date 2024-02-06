using './main.bicep'

param environment = 'staging'
param location = 'norwayeast'
param imageTag = readEnvironmentVariable('IMAGE_TAG')

// secrets
param environmentKeyVaultName = readEnvironmentVariable('ENVIRONMENT_KEY_VAULT_NAME')
param containerAppEnvironmentId = readEnvironmentVariable('CONTAINER_APP_ENVIRONMENT_ID')
param appInsightConnectionString = readEnvironmentVariable('APP_INSIGHTS_CONNECTION_STRING')
param appConfigurationName = readEnvironmentVariable('APP_CONFIGURATION_NAME')
