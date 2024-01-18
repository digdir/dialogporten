using 'main.bicep'

param environment = 'test'
param location = 'norwayeast'
param keyVaultSourceKeys = json(readEnvironmentVariable('KEYVAULT_SOURCE_KEYS', '[]'))
param gitSha = readEnvironmentVariable('GIT_SHA', '')

// secrets
param dialogportenPgAdminPassword = readEnvironmentVariable('PG_ADMIN_PASSWORD', '')
param apiManagementDigDirEmail = readEnvironmentVariable('APIM_DIGDIR_EMAIL', '')
param sourceKeyVaultSubscriptionId = readEnvironmentVariable('SOURCE_KEYVAULT_SUBSCRIPTION_ID', '')
param sourceKeyVaultResourceGroup = readEnvironmentVariable('SOURCE_KEYVAULT_RESOURCE_GROUP', '')
param sourceKeyVaultName = readEnvironmentVariable('SOURCE_KEYVAULT_NAME', '')

// SKUs
param APIM_SKU = 'Developer'
param keyVaultSKU = 'standard'
param appConfigurationSKU = 'standard'
param appInsightsSKU = 'PerGB2018'
param slackNotifierStorageAccountSKU = 'Standard_LRS'
param slackNotifierSKU = 'Y1'
param postgresServerSKU = 'Standard_B1ms'
