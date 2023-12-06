targetScope = 'subscription'
@minLength(3)
param environment string
param location string
param keyVault object
param gitSha string

@secure()
param secrets object

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

// Create resource groups
resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
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
    }
}

module keyVaultModule 'keyvault/create.bicep' = {
	scope: resourceGroup
	name: 'keyVault'
	params: {
		namePrefix: namePrefix
		location: location
	}
}

module appConfiguration 'appConfiguration/create.bicep' = {
    scope: resourceGroup
    name: 'appConfiguration'
    params: {
        namePrefix: namePrefix
        location: location
    }
}

module appInsights 'applicationInsights/create.bicep' = {
    scope: resourceGroup
    name: 'appInsights'
    params: {
        namePrefix: namePrefix
        location: location
    }
}

// #######################################
// Create references to existing resources
// #######################################

resource srcKeyVaultResource 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
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
		administratorLoginPassword: contains(keyVault.source.keys, 'dialogportenPgAdminPassword${environment}') ? srcKeyVaultResource.getSecret('dialogportenPgAdminPassword${environment}') : secrets.dialogportenPgAdminPassword
    }
}

module copySecrets 'keyvault/copySecrets.bicep' = {
	scope: resourceGroup
	name: 'copySecrets'
	params: {
		srcKeyVaultKeys: keyVault.source.keys
		srcKeyVaultName: secrets.sourceKeyVaultName
		srcKeyVaultRGNName: keyVault.source.resourceGroupName
		srcKeyVaultSubId: secrets.sourceKeyVaultSubscriptionId
		destKeyVaultName: keyVaultModule.outputs.name
		secretPrefix: 'dialogporten--${environment}--'
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
    {
        name: 'RESOURCE_GROUP_NAME'
        value: resourceGroup.name
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
        principalIds: containerAppsPrincipals
    }
}

output migrationJobName string = containerAppsExternal.outputs.migrationJobName
output resourceGroupName string = resourceGroup.name
