param namePrefix string
param location string
param skuName string
param skuFamily string

var keyVaultName = take('${namePrefix}-kv-${uniqueString(resourceGroup().id)}', 24)

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
	name: keyVaultName
	location: location
	properties: {
		// TODO: Remove, https://github.com/digdir/dialogporten/issues/229
		enablePurgeProtection: null // Null is the same as false and false is invalid for some reason
		enabledForTemplateDeployment: false
		sku: {
			name: skuName
			family: skuFamily
		}
		tenantId: subscription().tenantId
		accessPolicies: []
	}
}

output name string = keyVault.name
