// This Bicep module provisions a Service Bus namespace with a Premium SKU in Azure, 
// assigns a system-managed identity, and sets up a private endpoint for secure connectivity. 
// It also configures a private DNS zone for the Service Bus namespace to facilitate network resolution within the virtual network.

param namePrefix string
param location string
param subnetId string

@export()
type Sku = {
  name: 'Premium'
  tier: 'Premium'
  @minValue(1)
  capacity: int
}
param sku Sku

var name = '${namePrefix}-service-bus'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: name
  location: location
  sku: sku
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  name: '${name}-pe'
  location: location
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}-plsc'
        properties: {
          privateLinkServiceId: serviceBusNamespace.id
          groupIds: [
            'namespace'
          ]
          requestMessage: 'Connection to the Service Bus namespace ${name} for Dialogporten'
        }
      }
    ]
  }
}

var serviceBusDomainName = '${name}.servicebus.windows.net'

module privateDnsZone '../privateDnsZone/main.bicep' = {
  name: 'serviceBusPrivateDnsZone'
  params: {
    namePrefix: namePrefix
    defaultDomain: serviceBusDomainName
    vnetId: subnetId
    aRecords: [
      {
        name: 'default'
        ttl: 300
        ip: privateEndpoint.properties.networkInterfaces[0].properties.ipConfigurations[0].properties.privateIPAddress
      }
    ]
  }
}
