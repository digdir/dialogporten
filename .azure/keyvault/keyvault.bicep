param environment string
param location string
param adminObjectIds array = []
param readerObjectIds array = []

var adminAccessPolicies = [for admin in adminObjectIds: {
    objectId: admin
    tenantId: subscription().tenantId
    permissions: {
        keys: [ 'all' ]
        secrets: [ 'all' ]
        certificates: [ 'all' ]
    }
}]

var readerAccessPolicies = [for reader in readerObjectIds: {
	objectId: reader
	tenantId: subscription().tenantId
	permissions: {
		certificates: [ 'get', 'list' ]
		keys: [ 'get', 'list' ]
		secrets: [ 'get', 'list' ]
	}
}]

resource keyvault 'Microsoft.KeyVault/vaults@2022-11-01' = {
	name: 'dialogporten-${environment}-kv'
	location: location
	properties: {
		sku: {
			family: 'A'
			name: 'standard'
		}
		tenantId: subscription().tenantId
		accessPolicies: concat(adminAccessPolicies, readerAccessPolicies)
	}
}

output name string = keyvault.name