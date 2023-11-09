param namePrefix string
param location string

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
	name: '${namePrefix}-appConfiguration'
	location: location
	sku: {
		name: 'standard'
	}
	properties: {
		// TODO: Remove
		enablePurgeProtection: false
	}
	resource configStoreKeyValue 'keyValues' = {
        name: 'Sentinel'
        properties: {
            value: '1'
        }
    }
}

output endpoint string = appConfig.properties.endpoint
output name string = appConfig.name