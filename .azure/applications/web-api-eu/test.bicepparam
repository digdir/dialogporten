using './main.bicep'

param environment = 'test'
param location = 'norwayeast'
param imageTag = readEnvironmentVariable('IMAGE_TAG', '')

// secrets
param environmentKeyVaultName = readEnvironmentVariable('ENVIRONMENT_KEY_VAULT_NAME', '')
param containerAppEnvironmentId = readEnvironmentVariable('CONTAINTER_APP_ENVIRONMENT_ID', '')
param appInsightConnectionString = readEnvironmentVariable('APP_INSIGHTS_CONNECTION_STRING', '')
param appConfigurationName = readEnvironmentVariable('APP_CONFIGURATION_NAME', '')
