targetScope = 'subscription'
@minLength(3)
param environment string
param location string
param keyVaultSourceKeys array
param gitSha string

@secure()
param dialogportenPgAdminPassword string
@secure()
param apiManagementDigDirEmail string
@secure()
param sourceKeyVaultSubscriptionId string
@secure()
param sourceKeyVaultResourceGroup string
@secure()
param sourceKeyVaultName string

@allowed([
    'Basic'
    'Consumption'
    'Developer'
    'Isolated'
    'Premium'
    'Standard'
])
param APIM_SKU string

@allowed([
    'premium'
    'standard'
])
param keyVaultSKU string

@allowed([
    'standard'
])
param appConfigurationSKU string

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
param appInsightsSKU string

@allowed([
    'Standard_LRS'
    'Standard_GRS'
    'Standard_RAGRS'
    'Standard_ZRS'
    'Premium_LRS'
    'Premium_ZRS'
])
param slackNotifierStorageAccountSKU string

@allowed([
    'Y1'
])
param slackNotifierSKU string

@allowed([
    'Standard_B1ms'
])
param postgresServerSKU string

var secrets = {
    dialogportenPgAdminPassword: dialogportenPgAdminPassword
    apiManagementDigDirEmail: apiManagementDigDirEmail
    sourceKeyVaultSubscriptionId: sourceKeyVaultSubscriptionId
    sourceKeyVaultResourceGroup: sourceKeyVaultResourceGroup
    sourceKeyVaultName: sourceKeyVaultName
}

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

// Create resource groups
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
    name: '${namePrefix}-rg'
    location: location
}

module apiManagement 'apim/create.bicep' = {
    scope: resourceGroup
    name: 'apiManagement'
    params: {
        publisherEmail: secrets.apiManagementDigDirEmail
        location: location
        namePrefix: namePrefix
        sku: APIM_SKU
    }
}

module keyVaultModule 'keyvault/create.bicep' = {
    scope: resourceGroup
    name: 'keyVault'
    params: {
        namePrefix: namePrefix
        location: location
        sku: keyVaultSKU
    }
}

module appConfiguration 'appConfiguration/create.bicep' = {
    scope: resourceGroup
    name: 'appConfiguration'
    params: {
        namePrefix: namePrefix
        location: location
        sku: appConfigurationSKU
    }
}

module appInsights 'applicationInsights/create.bicep' = {
    scope: resourceGroup
    name: 'appInsights'
    params: {
        namePrefix: namePrefix
        location: location
        sku: appInsightsSKU
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

module postgresql 'postgreSql/create.bicep' = {
    scope: resourceGroup
    name: 'postgresql'
    params: {
        namePrefix: namePrefix
        location: location
        keyVaultName: keyVaultModule.outputs.name
        srcKeyVault: srcKeyVault
        srcSecretName: 'dialogportenPgAdminPassword${environment}'
        administratorLoginPassword: contains(keyVaultSourceKeys, 'dialogportenPgAdminPassword${environment}') ? srcKeyVaultResource.getSecret('dialogportenPgAdminPassword${environment}') : secrets.dialogportenPgAdminPassword
        sku: postgresServerSKU
    }
}

module copyEnvironmentSecrets 'keyvault/copySecrets.bicep' = {
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

module copyCrossEnvironmentSecrets 'keyvault/copySecrets.bicep' = {
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

module slackNotifier 'functionApp/slackNotifier.bicep' = {
    name: 'slackNotifier'
    scope: resourceGroup
    params: {
        location: location
        keyVaultName: keyVaultModule.outputs.name
        namePrefix: namePrefix
        applicationInsightsName: appInsights.outputs.appInsightsName
        storageAccountSKU: slackNotifierStorageAccountSKU
        applicationServicePlanSKU: slackNotifierSKU
    }
}

var containerAppEnvVars = [
    {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: environment
    }
    {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        value: appInsights.outputs.connectionString
    }
    {
        name: 'AZURE_APPCONFIG_URI'
        value: appConfiguration.outputs.endpoint
    }
    {
        name: 'ASPNETCORE_URLS'
        value: 'http://+:8080'
    }
    {
        name: 'GIT_SHA'
        value: gitSha
    }
]

module containerAppsExternal 'containerApp/createExternal.bicep' = {
    scope: resourceGroup
    name: 'containerAppsExternal'
    params: {
        baseImageUrl: baseImageUrl
        namePrefix: namePrefix
        location: location
        gitSha: gitSha
        envVariables: containerAppEnvVars
        migrationVerifierPrincipalAppId: srcKeyVaultResource.getSecret('MigrationVerificationInitContainerPrincipalAppId')
        migrationVerifierPrincipalPassword: srcKeyVaultResource.getSecret('MigrationVerificationInitContainerPrincipalPassword')
        apiManagementIp: apiManagement.outputs.apiManagementIp
        appInsightsWorkspaceName: appInsights.outputs.appInsightsWorkspaceName
        adoConnectionStringSecretUri: postgresql.outputs.adoConnectionStringSecretUri
    }
}

module apiBackends 'apim/addBackends.bicep' = {
    scope: resourceGroup
    name: 'apiBackends'
    params: {
        apiManagementName: apiManagement.outputs.apiManagementName
        containerAppEnvName: containerAppsExternal.outputs.containerAppEnvName
        webApiEuName: containerAppsExternal.outputs.webApiEuName
        webApiSoName: containerAppsExternal.outputs.webApiSoName
    }
}
// module containerAppsInternal 'containerApp/createInternal.bicep' = {
//     scope: resourceGroup
//     name: 'containerAppsInternal'
//     params: {
//         baseImageUrl: baseImageUrl
//         namePrefix: namePrefix
//         location: location
//         gitSha: gitSha
//         envVariables: containerAppEnvVars
//         environmentId: containerAppEnvs.outputs.internalEnvId
//     }
// }

var containerAppsPrincipals = concat(
    containerAppsExternal.outputs.identityPrincipalIds)
// containerAppsInternal.outputs.identityPrincipalIds

module appConfigReaderAccessPolicy 'appConfiguration/addReaderRoles.bicep' = {
    scope: resourceGroup
    name: 'appConfigReaderAccessPolicy'
    params: {
        appConfigurationName: appConfiguration.outputs.name
        principalIds: containerAppsPrincipals
    }
}

module appInsightsReaderAccessPolicy 'applicationInsights/addReaderRoles.bicep' = {
    scope: resourceGroup
    name: 'appInsightsReaderAccessPolicy'
    params: {
        appInsightsName: appInsights.outputs.appInsightsName
        principalIds: [ slackNotifier.outputs.functionAppPrincipalId ]
    }
}

module appConfigConfigurations 'appConfiguration/upsertKeyValue.bicep' = {
    scope: resourceGroup
    name: 'AppConfig_Add_DialogDbConnectionString'
    params: {
        configStoreName: appConfiguration.outputs.name
        key: 'Infrastructure:DialogDbConnectionString'
        value: postgresql.outputs.adoConnectionStringSecretUri
        keyValueType: 'keyVaultReference'
    }
}

module keyVaultReaderAccessPolicy 'keyvault/addReaderRoles.bicep' = {
    scope: resourceGroup
    name: 'keyVaultReaderAccessPolicy'
    params: {
        keyvaultName: keyVaultModule.outputs.name
        principalIds: concat(containerAppsPrincipals, [ slackNotifier.outputs.functionAppPrincipalId ])
    }
}

output migrationJobName string = containerAppsExternal.outputs.migrationJobName
output resourceGroupName string = resourceGroup.name
