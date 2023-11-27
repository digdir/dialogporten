param location string 
param namePrefix string

resource servicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
	name: '${namePrefix}-serviceplan'
	location: location
	kind: 'app'
	sku: { name: 'F1' }
}

resource webApi 'Microsoft.Web/sites@2022-03-01' = {
	name: '${namePrefix}-webapi'
	location: location
	identity: {
		type: 'SystemAssigned'
	}
	properties: { 
		serverFarmId: servicePlan.id
	}
}

output identityPrincipalId string = webApi.identity.principalId
output name string = webApi.name