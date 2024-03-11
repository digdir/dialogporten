using './main.bicep'

param environment = 'staging'
param location = 'norwayeast'
param apimIp = '51.13.86.131'
param imageTag = readEnvironmentVariable('IMAGE_TAG')

// secrets
param environmentKeyVaultName = readEnvironmentVariable('ENVIRONMENT_KEY_VAULT_NAME')
param containerAppEnvironmentName = readEnvironmentVariable('CONTAINER_APP_ENVIRONMENT_NAME')
param appInsightConnectionString = readEnvironmentVariable('APP_INSIGHTS_CONNECTION_STRING')
param appConfigurationName = readEnvironmentVariable('APP_CONFIGURATION_NAME')
