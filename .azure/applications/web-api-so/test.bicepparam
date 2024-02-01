using './main.bicep'

param environment = 'test'
param location = 'norwayeast'
param gitSha = readEnvironmentVariable('GIT_SHA', '')
param imageTag = readEnvironmentVariable('IMAGE_TAG', '')

param containerAppEnvironmentId = readEnvironmentVariable('CONTAINTER_APP_ENVIRONMENT_ID', '')

param appInsightConnectionString = readEnvironmentVariable('APP_INSIGHTS_CONNECTION_STRING', '')

param appConfigurationName = readEnvironmentVariable('APP_CONFIGURATION_NAME', '')

// secrets
param sourceKeyVaultSubscriptionId = readEnvironmentVariable('SOURCE_KEY_VAULT_SUBSCRIPTION_ID', '')
param sourceKeyVaultResourceGroup = readEnvironmentVariable('SOURCE_KEY_VAULT_RESOURCE_GROUP', '')
param sourceKeyVaultName = readEnvironmentVariable('SOURCE_KEY_VAULT_NAME', '')
param environmentKeyVaultName = readEnvironmentVariable('ENVIRONMENT_KEY_VAULT_NAME', '')
