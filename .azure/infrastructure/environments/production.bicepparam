using '../infrastructure.bicep'

param environment = 'production'
param location = 'norwayeast'
param keyVaultSourceKeys = json(readEnvironmentVariable('KEY_VAULT_SOURCE_KEYS', '[]'))
param gitSha = readEnvironmentVariable('GIT_SHA', '')

// secrets
param dialogportenPgAdminPassword = readEnvironmentVariable('PG_ADMIN_PASSWORD', '')
param sourceKeyVaultSubscriptionId = readEnvironmentVariable('SOURCE_KEYVAULT_SUBSCRIPTION_ID', '')
param sourceKeyVaultResourceGroup = readEnvironmentVariable('SOURCE_KEYVAULT_RESOURCE_GROUP', '')
param sourceKeyVaultName = readEnvironmentVariable('SOURCE_KEYVAULT_NAME', '')

// SKUs
param keyVaultSKUName = 'standard'
param keyVaultSKUFamily = 'A'
param appConfigurationSKUName = 'standard'
param appInsightsSKUName = 'PerGB2018'
param slackNotifierStorageAccountSKUName = 'Standard_LRS'
param slackNotifierApplicationServicePlanSKUName = 'Y1'
param slackNotifierApplicationServicePlanSKUTier = 'Dynamic'
param postgresServerSKUName = 'Standard_B1ms'
param postgresServerSKUTier = 'Burstable'
