targetScope = 'subscription'

@description('The environment for the deployment')
@minLength(3)
param environment string

@description('The location where the resources will be deployed')
@minLength(3)
param location string

@description('Array of all keys in the source Key Vault')
param keyVaultSourceKeys array

@description('Password for PostgreSQL admin')
@secure()
@minLength(3)
param dialogportenPgAdminPassword string

@description('Subscription ID for the source Key Vault')
@secure()
@minLength(3)
param sourceKeyVaultSubscriptionId string

@description('Resource group for the source Key Vault')
@secure()
@minLength(3)
param sourceKeyVaultResourceGroup string

@description('Name of the source Key Vault')
@secure()
@minLength(3)
param sourceKeyVaultName string

@description('SSH public key for the ssh jumper')
@secure()
@minLength(3)
param sourceKeyVaultSshJumperSshPublicKey string

import { Sku as KeyVaultSku } from '../modules/keyvault/create.bicep'
param keyVaultSku KeyVaultSku

import { Sku as AppConfigurationSku } from '../modules/appConfiguration/create.bicep'
param appConfigurationSku AppConfigurationSku

import { Sku as AppInsightsSku } from '../modules/applicationInsights/create.bicep'
param appInsightsSku AppInsightsSku

import { Sku as SlackNotifierSku } from '../modules/functionApp/slackNotifier.bicep'
param slackNotifierSku SlackNotifierSku

import { Sku as PostgresSku } from '../modules/postgreSql/create.bicep'
param postgresSku PostgresSku

import { Sku as ServiceBusSku } from '../modules/serviceBus/main.bicep'
param serviceBusSku ServiceBusSku

import { Sku as RedisSku } from '../modules/redis/main.bicep'
param redisSku RedisSku
@minLength(1)
param redisVersion string

var secrets = {
  dialogportenPgAdminPassword: dialogportenPgAdminPassword
  sourceKeyVaultSubscriptionId: sourceKeyVaultSubscriptionId
  sourceKeyVaultResourceGroup: sourceKeyVaultResourceGroup
  sourceKeyVaultName: sourceKeyVaultName
  sourceKeyVaultSshJumperSshPublicKey: sourceKeyVaultSshJumperSshPublicKey
}

var namePrefix = 'dp-be-${environment}'

var tags = {
  Environment: environment
  Product: 'Dialogporten'
}

// Create resource groups
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: '${namePrefix}-rg'
  location: location
  tags: tags
}

module environmentKeyVault '../modules/keyvault/create.bicep' = {
  scope: resourceGroup
  name: 'keyVault'
  params: {
    namePrefix: namePrefix
    location: location
    sku: keyVaultSku
    tags: tags
  }
}

module appConfiguration '../modules/appConfiguration/create.bicep' = {
  scope: resourceGroup
  name: 'appConfiguration'
  params: {
    namePrefix: namePrefix
    location: location
    sku: appConfigurationSku
    tags: tags
  }
}

module appInsights '../modules/applicationInsights/create.bicep' = {
  scope: resourceGroup
  name: 'appInsights'
  params: {
    namePrefix: namePrefix
    location: location
    sku: appInsightsSku
    tags: tags
  }
}

module serviceBus '../modules/serviceBus/main.bicep' = {
  scope: resourceGroup
  name: 'serviceBus'
  params: {
    namePrefix: namePrefix
    location: location
    sku: serviceBusSku
    subnetId: vnet.outputs.serviceBusSubnetId
    vnetId: vnet.outputs.virtualNetworkId
    tags: tags
  }
}

module vnet '../modules/vnet/main.bicep' = {
  scope: resourceGroup
  name: 'vnet'
  params: {
    namePrefix: namePrefix
    location: location
    tags: tags
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

module sshJumper '../modules/ssh-jumper/main.bicep' = {
  scope: resourceGroup
  name: 'sshJumper'
  params: {
    namePrefix: namePrefix
    location: location
    subnetId: vnet.outputs.defaultSubnetId
    tags: tags
    sshPublicKey: secrets.sourceKeyVaultSshJumperSshPublicKey
  }
}

module postgresql '../modules/postgreSql/create.bicep' = {
  scope: resourceGroup
  name: 'postgresql'
  params: {
    namePrefix: namePrefix
    location: location
    environmentKeyVaultName: environmentKeyVault.outputs.name
    srcKeyVault: srcKeyVault
    srcKeyVaultAdministratorLoginPasswordKey: 'dialogportenPgAdminPassword${environment}'
    administratorLoginPassword: contains(keyVaultSourceKeys, 'dialogportenPgAdminPassword${environment}')
      ? srcKeyVaultResource.getSecret('dialogportenPgAdminPassword${environment}')
      : secrets.dialogportenPgAdminPassword
    sku: postgresSku
    subnetId: vnet.outputs.postgresqlSubnetId
    vnetId: vnet.outputs.virtualNetworkId
    tags: tags
  }
}

module redis '../modules/redis/main.bicep' = {
  scope: resourceGroup
  name: 'redis'
  params: {
    namePrefix: namePrefix
    location: location
    environmentKeyVaultName: environmentKeyVault.outputs.name
    sku: redisSku
    version: redisVersion
    subnetId: vnet.outputs.redisSubnetId
    vnetId: vnet.outputs.virtualNetworkId
    tags: tags
  }
}

module copyCrossEnvironmentSecrets '../modules/keyvault/copySecrets.bicep' = {
  scope: resourceGroup
  name: 'copyCrossEnvironmentSecrets'
  params: {
    appConfigurationName: appConfiguration.outputs.name
    srcKeyVaultKeys: keyVaultSourceKeys
    srcKeyVaultName: secrets.sourceKeyVaultName
    srcKeyVaultRGNName: secrets.sourceKeyVaultResourceGroup
    srcKeyVaultSubId: secrets.sourceKeyVaultSubscriptionId
    destKeyVaultName: environmentKeyVault.outputs.name
    secretPrefix: 'dialogporten--any--'
    tags: tags
  }
}

module copyEnvironmentSecrets '../modules/keyvault/copySecrets.bicep' = {
  scope: resourceGroup
  name: 'copyEnvironmentSecrets'
  params: {
    appConfigurationName: appConfiguration.outputs.name
    srcKeyVaultKeys: keyVaultSourceKeys
    srcKeyVaultName: secrets.sourceKeyVaultName
    srcKeyVaultRGNName: secrets.sourceKeyVaultResourceGroup
    srcKeyVaultSubId: secrets.sourceKeyVaultSubscriptionId
    destKeyVaultName: environmentKeyVault.outputs.name
    secretPrefix: 'dialogporten--${environment}--'
    tags: tags
  }
}

module slackNotifier '../modules/functionApp/slackNotifier.bicep' = {
  name: 'slackNotifier'
  scope: resourceGroup
  params: {
    location: location
    keyVaultName: environmentKeyVault.outputs.name
    namePrefix: namePrefix
    applicationInsightsName: appInsights.outputs.appInsightsName
    sku: slackNotifierSku
    tags: tags
  }
}

module containerAppEnv '../modules/containerAppEnv/main.bicep' = {
  scope: resourceGroup
  name: 'containerAppEnv'
  params: {
    namePrefix: namePrefix
    location: location
    appInsightWorkspaceName: appInsights.outputs.appInsightsWorkspaceName
    subnetId: vnet.outputs.containerAppEnvironmentSubnetId
    tags: tags
  }
}

module appInsightsReaderAccessPolicy '../modules/applicationInsights/addReaderRoles.bicep' = {
  scope: resourceGroup
  name: 'appInsightsReaderAccessPolicy'
  params: {
    appInsightsName: appInsights.outputs.appInsightsName
    principalIds: [slackNotifier.outputs.functionAppPrincipalId]
  }
}

module postgresConnectionStringAppConfig '../modules/appConfiguration/upsertKeyValue.bicep' = {
  scope: resourceGroup
  name: 'AppConfig_Add_DialogDbConnectionString'
  params: {
    configStoreName: appConfiguration.outputs.name
    key: 'Infrastructure:DialogDbConnectionString'
    value: postgresql.outputs.adoConnectionStringSecretUri
    keyValueType: 'keyVaultReference'
    tags: tags
  }
}

module redisConnectionStringAppConfig '../modules/appConfiguration/upsertKeyValue.bicep' = {
  scope: resourceGroup
  name: 'AppConfig_Add_Redis_ConnectionString'
  params: {
    configStoreName: appConfiguration.outputs.name
    key: 'Infrastructure:Redis:ConnectionString'
    value: redis.outputs.connectionStringSecretUri
    keyValueType: 'keyVaultReference'
    tags: tags
  }
}

module keyVaultReaderAccessPolicy '../modules/keyvault/addReaderRoles.bicep' = {
  scope: resourceGroup
  name: 'keyVaultReaderAccessPolicyFunctions'
  params: {
    keyvaultName: environmentKeyVault.outputs.name
    principalIds: [slackNotifier.outputs.functionAppPrincipalId]
  }
}

output resourceGroupName string = resourceGroup.name
output containerAppEnvId string = containerAppEnv.outputs.containerAppEnvId
output environmentKeyVaultName string = environmentKeyVault.outputs.name
