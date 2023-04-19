targetScope = 'subscription'

param environment string
param location string
param keyVault object
var namePrefix = 'dp${environment}'
@secure()
param secrets object

// Create resource groups
resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
	name: '${namePrefix}-rg'
	location: location
}

// Create resources without dependencies to other resources
module website 'website/create.bicep' = {
    scope: resourceGroup
    name: 'website'
    params: {
        namePrefix: namePrefix
        location: location
    }
}

module keyVaultModule 'keyvault/create.bicep' = {
	scope: resourceGroup
	name: 'keyVault'
	params: {
		namePrefix: namePrefix
		location: location
		adminObjectIds: keyVault.adminObjectIds
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

// Create references to existing resources
resource srcKeyVaultResource 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
	name: keyVault.source.name
    scope: az.resourceGroup(keyVault.source.subscriptionId, keyVault.source.resourceGroupName)
}

// Create resources with dependencies to other resources
module postgresql 'postgreSql/create.bicep' = {
    scope: resourceGroup
    name: 'postgresql'
    params: {
        namePrefix: namePrefix
        location: location
		keyVaultName: keyVaultModule.outputs.name
        srcKeyVault: keyVault.source
        srcSecretName: 'dialogportenPgAdminPassword${environment}'
		administratorLoginPassword: contains(keyVault.source.keys, 'dialogportenPgAdminPassword${environment}') ? srcKeyVaultResource.getSecret('dialogportenPgAdminPassword${environment}') : secrets.dialogportenPgAdminPassword
    }
}

module copySecret 'keyvault/copySecrets.bicep' = {
	scope: resourceGroup
	name: 'copySecrets'
	params: {
		srcKeyVaultKeys: keyVault.source.keys
		srcKeyVaultName: keyVault.source.name
		srcKeyVaultRGNName: keyVault.source.resourceGroupName
		srcKeyVaultSubId: keyVault.source.subscriptionId
		destKeyVaultName: keyVaultModule.outputs.name
		secretPrefix: 'dialogporten--${environment}--'
	}
}

module appsettings 'website/upsertAppsettings.bicep' = {
	scope: resourceGroup
	name: 'appsettings'
	params: {
		websiteName: website.outputs.name
		settings: {
			APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
			AZURE_APPCONFIG_URI: appConfiguration.outputs.endpoint
		}
	}
}

module appConfigConfigurations 'appConfiguration/upsertKeyValue.bicep' = {
    scope: resourceGroup
    name: 'AppConfig_Add_DialogueDbConnectionString'
    params: {
        configStoreName: appConfiguration.outputs.name
        key: 'Infrastructure:DialogueDbConnectionString' 
        value: postgresql.outputs.adoConnectionStringSecretUri
        keyValueType: 'keyVaultReference'
    }
}

module keyVaultReaderAccessPolicy 'keyvault/addReaderRoles.bicep' = {
    scope: resourceGroup
    name: 'keyVaultReaderAccessPolicy'
    params: {
        keyvaultName: keyVaultModule.outputs.name
        principalIds: [ website.outputs.identityPrincipalId ]
    }
}

module appConfigReaderAccessPolicy 'appConfiguration/addReaderRoles.bicep' = {
    scope: resourceGroup
    name: 'appConfigReaderAccessPolicy'
    params: {
        appConfigurationName: appConfiguration.outputs.name
        principalIds: [ website.outputs.identityPrincipalId ]
    }
}

output resourceGroupName string = resourceGroup.name
output postgresqlName string = postgresql.outputs.serverName
output psqlConnectionStringSecretUri string = postgresql.outputs.psqlConnectionStringSecretUri
output websiteName string = website.outputs.name