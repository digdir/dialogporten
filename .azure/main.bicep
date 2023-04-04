targetScope = 'subscription'

param environment string
param location string

var resourceGroupName = 'dialogporten-${environment}-rg'

resource newResourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
	name: resourceGroupName
	location: location
}

module deployment 'ResourceGroup.bicep' = {
	scope: newResourceGroup	
	name: 'deploymentModuleName'
	params: {
		environment: environment
	}
}