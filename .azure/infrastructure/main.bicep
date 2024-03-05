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

import {Sku as KeyVaultSku} from '../modules/keyvault/create.bicep'
param keyVaultSku KeyVaultSku

import {Sku as AppConfigurationSku} from '../modules/appConfiguration/create.bicep'
param appConfigurationSku AppConfigurationSku

import {Sku as AppInsightsSku} from '../modules/applicationInsights/create.bicep'
param appInsightsSku AppInsightsSku

import {Sku as SlackNotifierSku} from '../modules/functionApp/slackNotifier.bicep'
param slackNotifierSku SlackNotifierSku

import {Sku as PostgresSku} from '../modules/postgreSql/create.bicep'
param postgresSku PostgresSku

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
    sku: keyVaultSku
  }
}

module appConfiguration '../modules/appConfiguration/create.bicep' = {
  scope: resourceGroup
  name: 'appConfiguration'
  params: {
    namePrefix: namePrefix
    location: location
    sku: appConfigurationSku
  }
}

module appInsights '../modules/applicationInsights/create.bicep' = {
  scope: resourceGroup
  name: 'appInsights'
  params: {
    namePrefix: namePrefix
    location: location
    sku: appInsightsSku
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
    sku: postgresSku
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
    sku: slackNotifierSku
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
