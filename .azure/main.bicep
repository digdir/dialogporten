targetScope = 'subscription'

param keyVault object
param environment string
param location string
param srcKeyVaultKeys array

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
	name: 'dialogporten-${environment}-rg'
	location: location
}

module keyVaultResource 'keyvault/keyvault.bicep' = {
	scope: resourceGroup
	name: 'keyVault'
	params: {
		environment: environment
		location: location
		adminObjectIds: keyVault.adminObjectIds
	}
}

module secretCopy 'keyvault/copySecrets.bicep' = {
	scope: resourceGroup
	name: 'copySecrets'
	params: {
		srcKeyVaultKeys: srcKeyVaultKeys
		srcKeyVaultName: keyVault.source.name
		srcKeyVaultRGNName: keyVault.source.resourceGroupName
		srcKeyVaultSubId: keyVault.source.subscriptionId
		destKeyVaultName: keyVaultResource.outputs.name
		secretPrefix: 'dialogporten--${environment}--'
	}
}