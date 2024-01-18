param location string
param namePrefix string
param publisherEmail string
param sku string

resource apim 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  location: location
  name: '${namePrefix}-apim'
  sku: {
    name: sku
    capacity: 1
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: 'Digitaliseringsdirektoratet'
    developerPortalStatus: 'Disabled'
    legacyPortalStatus: 'Enabled'
    publicNetworkAccess: 'Enabled'
    natGatewayState: 'Disabled'
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Protocols.Server.Http2': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TripleDes168': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Ssl30': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'False'
    }
  }
}

output apiManagementName string = apim.name
output apiManagementIp string = apim.properties.publicIPAddresses[0]
