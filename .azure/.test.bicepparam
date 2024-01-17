using 'main.bicep'

param environment = 'test'
param location = 'norwayeast'
param keyVaultSourceKeys = json(readEnvironmentVariable('KEYVAULT_SOURCE_KEYS', '[]'))
param gitSha = readEnvironmentVariable('GIT_SHA', '')

param dialogportenPgAdminPassword = readEnvironmentVariable('PG_ADMIN_PASSWORD', '')
param apiManagementDigDirEmail = readEnvironmentVariable('APIM_DIGDIR_EMAIL', '')
param sourceKeyVaultSubscriptionId = readEnvironmentVariable('SOURCE_KEYVAULT_SUBSCRIPTION_ID', '')
param sourceKeyVaultResourceGroup = readEnvironmentVariable('SOURCE_KEYVAULT_RESOURCE_GROUP', '')
param sourceKeyVaultName = readEnvironmentVariable('SOURCE_KEYVAULT_NAME', '')
