targetScope = 'resourceGroup'

@description('Specifies the location for resources.')
param location string = resourceGroup().location
param environment string
//param configuration object = {
//	Sentinel: '1'
//}
@secure()
param secrets object = { }

var resourceNamePrefix = 'dialogporten-${environment}'


resource webApi 'Microsoft.Web/sites@2022-03-01' = {
	name: '${resourceNamePrefix}-webapi'
	location: location
	properties: { }
	identity: {
		type: 'SystemAssigned'
	}
}

resource keyvault 'Microsoft.KeyVault/vaults@2022-11-01' = {
	name: '${resourceNamePrefix}-kv'
	location: location
	properties: {
		sku: {
			family: 'A'
			name: 'standard'
		}
		tenantId: subscription().tenantId
		accessPolicies:[
			// TODO: Legg til en gruppe/person med admin tilgang
			{
                tenantId: subscription().tenantId
                objectId: '232d6380-af9f-45f3-b2e7-aef5b5292e3a'
                permissions: {
                    keys: [ 'Get', 'List', 'Update', 'Create', 'Import', 'Delete', 'Recover', 'Backup', 'Restore', 'GetRotationPolicy', 'SetRotationPolicy', 'Rotate' ]
                    secrets: [ 'Get', 'List', 'Set', 'Delete', 'Recover', 'Backup', 'Restore' ]
                    certificates: [ 'Get', 'List', 'Update', 'Create', 'Import', 'Delete', 'Recover', 'Backup', 'Restore', 'ManageContacts', 'ManageIssuers', 'GetIssuers', 'ListIssuers', 'SetIssuers', 'DeleteIssuers' ]
                }
            }
			{
				objectId: webApi.identity.principalId
				tenantId: subscription().tenantId
				permissions: {
					certificates: [ 'get', 'list' ]
					keys: [ 'get', 'list' ]
					secrets: [ 'get', 'list' ]
				}
			}
		]
	}
}

resource secretKeyValues 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = [for secret in items(secrets): {
	name: secret.key
	properties: { value: secret.value }
}]

//resource appConfigurationStore 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
//	name: '${resourceNamePrefix}-appconfiguration'
//	location: location
//	sku: {
//		name: 'free'
//	}
//}

//resource configurationKeyValues 'Microsoft.AppConfiguration/configurationStores/keyValues@2022-05-01' = [for config in items(configuration): {
//	parent: appConfigurationStore
//	name: config.key
//	properties: { value: config.value }
//}]

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
	name: '${resourceNamePrefix}-postgres'
	location: location
	properties: {
		version: '14'
		administratorLogin: 'postgresAdmin'
		// TODO: Fix this...
		administratorLoginPassword: 'changeme'
	}
	sku: {
		name: 'Standard_B1ms'
		tier: 'Burstable'
	}
	resource database 'databases' = {
		name: 'dialogporten'
	}
	resource allowAzureAccess 'firewallRules' = {
		name: 'AllowAccessFromAzure'
		properties: {
			startIpAddress: '0.0.0.0'
			endIpAddress: '0.0.0.0'
		}
	}
}

resource lala 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = {
	parent: keyvault
	name: 'DbConnectionString'
	properties: {
		value: postgres.properties.fullyQualifiedDomainName
	}
}

// TODO: 
// - Application insights
// - PostgreSQL