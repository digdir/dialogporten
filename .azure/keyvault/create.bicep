param namePrefix string
param location string
param adminObjectIds array = []

var adminAccessPolicies = [for admin in adminObjectIds: {
    objectId: admin
    tenantId: subscription().tenantId
    permissions: {
        keys: [ 'all' ]
        secrets: [ 'all' ]
        certificates: [ 'all' ]
    }
}]

var keyvaultName = take('${namePrefix}-kv-${uniqueString(resourceGroup().id)}', 24)

resource keyvault 'Microsoft.KeyVault/vaults@2023-02-01' = {
	name: keyvaultName
	location: location
	properties: {
		// TODO: Remove
		enablePurgeProtection: null // Null is the same as false and false is invalid for some reason
		enabledForTemplateDeployment: false
		sku: {
			family: 'A'
			name: 'standard'
		}
		tenantId: subscription().tenantId
		accessPolicies: adminAccessPolicies
	}
}

output name string = keyvault.name