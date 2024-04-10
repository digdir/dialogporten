using './main.bicep'

param environment = 'staging'
param location = 'norwayeast'
param keyVaultSourceKeys = json(readEnvironmentVariable('KEY_VAULT_SOURCE_KEYS'))

param redisVersion = '6.0'

// secrets
param dialogportenPgAdminPassword = readEnvironmentVariable('PG_ADMIN_PASSWORD')
param sourceKeyVaultSubscriptionId = readEnvironmentVariable('SOURCE_KEY_VAULT_SUBSCRIPTION_ID')
param sourceKeyVaultResourceGroup = readEnvironmentVariable('SOURCE_KEY_VAULT_RESOURCE_GROUP')
param sourceKeyVaultName = readEnvironmentVariable('SOURCE_KEY_VAULT_NAME')

// SKUs
param keyVaultSku = {
  name: 'standard'
  family: 'A'
}
param appConfigurationSku = {
  name: 'standard'
}
param appInsightsSku = {
  name: 'PerGB2018'
}
param slackNotifierSku = {
  storageAccountName: 'Standard_LRS'
  applicationServicePlanName: 'Y1'
  applicationServicePlanTier: 'Dynamic'
}
param postgresSku = {
  name: 'Standard_B1ms'
  tier: 'Burstable'
}

param redisSku = {
  name: 'Basic'
  family: 'C'
  capacity: 1
}

param serviceBusSku = {
  name: 'Standard'
  tier: 'Standard'
  capacity: 1
}
