targetScope = 'subscription'
@minLength(3)
param environment string
@minLength(3)
param location string

param keyVaultSourceKeys array

@secure()
@minLength(3)
param dialogportenPgAdminPassword string
@secure()
@minLength(3)
param sourceKeyVaultSubscriptionId string
@secure()
@minLength(3)
param sourceKeyVaultResourceGroup string
@secure()
@minLength(3)
param sourceKeyVaultName string

@allowed(
  [
    'premium'
    'standard'
  ]
)
param keyVaultSKUName string

@allowed([
  'A'
])
param keyVaultSKUFamily string

@allowed([
  'standard'
])
param appConfigurationSKUName string

@allowed([
  'CapacityReservation'
  'Free'
  'LACluster'
  'PerGB2018'
  'PerNode'
  'Premium'
  'Standalone'
  'Standard'
])
param appInsightsSKUName string

@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
])
param slackNotifierStorageAccountSKUName string

@allowed([
  'Y1'
])
param slackNotifierApplicationServicePlanSKUName string

@allowed([
  'Dynamic'

])
param slackNotifierApplicationServicePlanSKUTier string

@allowed([
  'Standard_B1ms'
])
param postgresServerSKUName string
@allowed([
  'Burstable'
  'GeneralPurpose'
  'MemoryOptimized'
])
param postgresServerSKUTier string

var secrets = {
  dialogportenPgAdminPassword: dialogportenPgAdminPassword
  sourceKeyVaultSubscriptionId: sourceKeyVaultSubscriptionId
  sourceKeyVaultResourceGroup: sourceKeyVaultResourceGroup
  sourceKeyVaultName: sourceKeyVaultName
}

var namePrefix = 'dp-be-${environment}'

// Create resource groups
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: '${namePrefix}-rg'
  location: location
}

module keyVaultModule '../modules/keyvault/create.bicep' = {
  scope: resourceGroup
  name: 'keyVault'
  params: {
    namePrefix: namePrefix
    location: location
    skuName: keyVaultSKUName
    skuFamily: keyVaultSKUFamily
  }
}

module appConfiguration '../modules/appConfiguration/create.bicep' = {
  scope: resourceGroup
  name: 'appConfiguration'
  params: {
    namePrefix: namePrefix
    location: location
    skuName: appConfigurationSKUName
  }
}

module appInsights '../modules/applicationInsights/create.bicep' = {
  scope: resourceGroup
  name: 'appInsights'
  params: {
    namePrefix: namePrefix
    location: location
    skuName: appInsightsSKUName
  }
}

// #######################################
// Create references to existing resources
// #######################################

resource srcKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: secrets.sourceKeyVaultName
  scope: az.resourceGroup(secrets.sourceKeyVaultSubscriptionId, secrets.sourceKeyVaultResourceGroup)
}

// #####################################################
// Create resources with dependencies to other resources
// #####################################################

var srcKeyVault = {
  name: secrets.sourceKeyVaultName
  subscriptionId: secrets.sourceKeyVaultSubscriptionId
  resourceGroupName: secrets.sourceKeyVaultResourceGroup
}

module postgresql '../modules/postgreSql/create.bicep' = {
  scope: resourceGroup
  name: 'postgresql'
  params: {
    namePrefix: namePrefix
    location: location
    keyVaultName: keyVaultModule.outputs.name
    srcKeyVault: srcKeyVault
    srcSecretName: 'dialogportenPgAdminPassword${environment}'
    administratorLoginPassword: contains(keyVaultSourceKeys, 'dialogportenPgAdminPassword${environment}') ? srcKeyVaultResource.getSecret('dialogportenPgAdminPassword${environment}') : secrets.dialogportenPgAdminPassword
    skuName: postgresServerSKUName
    skuTier: postgresServerSKUTier
  }
}

module copyEnvironmentSecrets '../modules/keyvault/copySecrets.bicep' = {
  scope: resourceGroup
  name: 'copyEnvironmentSecrets'
  params: {
    srcKeyVaultKeys: keyVaultSourceKeys
    srcKeyVaultName: secrets.sourceKeyVaultName
    srcKeyVaultRGNName: secrets.sourceKeyVaultResourceGroup
    srcKeyVaultSubId: secrets.sourceKeyVaultSubscriptionId
    destKeyVaultName: keyVaultModule.outputs.name
    secretPrefix: 'dialogporten--${environment}--'
  }
}

module copyCrossEnvironmentSecrets '../modules/keyvault/copySecrets.bicep' = {
  scope: resourceGroup
  name: 'copyCrossEnvironmentSecrets'
  params: { srcKeyVaultKeys: keyVaultSourceKeys
    srcKeyVaultName: secrets.sourceKeyVaultName
    srcKeyVaultRGNName: secrets.sourceKeyVaultResourceGroup
    srcKeyVaultSubId: secrets.sourceKeyVaultSubscriptionId
    destKeyVaultName: keyVaultModule.outputs.name
    secretPrefix: 'dialogporten--any--'
  }
}

module slackNotifier '../modules/functionApp/slackNotifier.bicep' = {
  name: 'slackNotifier'
  scope: resourceGroup
  params: {
    location: location
    keyVaultName: keyVaultModule.outputs.name
    namePrefix: namePrefix
    applicationInsightsName: appInsights.outputs.appInsightsName
    storageAccountSKUName: slackNotifierStorageAccountSKUName
    applicationServicePlanSKUName: slackNotifierApplicationServicePlanSKUName
    applicationServicePlanSKUTier: slackNotifierApplicationServicePlanSKUTier
  }
}

module containerAppEnv '../modules/containerAppEnv/main.bicep' = {
  scope: resourceGroup
  name: 'containerAppEnv'
  params: {
    namePrefix: namePrefix
    location: location
    appInsightWorkspaceName: appInsights.outputs.appInsightsWorkspaceName
  }
}

module appInsightsReaderAccessPolicy '../modules/applicationInsights/addReaderRoles.bicep' = {
  scope: resourceGroup
  name: 'appInsightsReaderAccessPolicy'
  params: {
    appInsightsName: appInsights.outputs.appInsightsName
    principalIds: [ slackNotifier.outputs.functionAppPrincipalId ]
  }
}

module appConfigConfigurations '../modules/appConfiguration/upsertKeyValue.bicep' = {
  scope: resourceGroup
  name: 'AppConfig_Add_DialogDbConnectionString'
  params: {
    configStoreName: appConfiguration.outputs.name
    key: 'Infrastructure:DialogDbConnectionString'
    value: postgresql.outputs.adoConnectionStringSecretUri
    keyValueType: 'keyVaultReference'
  }
}

module keyVaultReaderAccessPolicy '../modules/keyvault/addReaderRoles.bicep' = {
  scope: resourceGroup
  name: 'keyVaultReaderAccessPolicyFunctions'
  params: {
    keyvaultName: keyVaultModule.outputs.name
    principalIds: [ slackNotifier.outputs.functionAppPrincipalId ]
  }
}

output resourceGroupName string = resourceGroup.name
output containerAppEnvId string = containerAppEnv.outputs.containerAppEnvId
output environmentKeyVaultName string = keyVaultModule.outputs.name
