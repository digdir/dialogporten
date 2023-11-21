param namePrefix string
param location string

var keyVaultName = take('${namePrefix}-kv-${uniqueString(resourceGroup().id)}', 24)

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' = {
	name: keyVaultName
	location: location
	properties: {
		// TODO: Remove, https://github.com/digdir/dialogporten/issues/229
		enablePurgeProtection: null // Null is the same as false and false is invalid for some reason
		enabledForTemplateDeployment: false
		sku: {
			family: 'A'
			name: 'standard'
		}
		tenantId: subscription().tenantId
	}
}

output name string = keyVault.name
