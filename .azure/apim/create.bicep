param location string
param namePrefix string
param publisherEmail string

resource apim 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  location: location
  name: '${namePrefix}-apim'
  sku: {
    name: 'Developer'
    capacity: 1
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: 'Digitaliseringsdirektoratet'
    developerPortalStatus: 'Disabled'
  }
}

output apiManagementName string = apim.name
output apiManagementIp string = apim.properties.publicIPAddresses[0]
