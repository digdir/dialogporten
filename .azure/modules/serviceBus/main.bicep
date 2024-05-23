param namePrefix string
param location string

param subnetIds array

@export()
type Sku = {
  name: 'Premium'
  tier: 'Premium'
  @minValue(1)
  capacity: int
}
param sku Sku

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${namePrefix}-service-bus'
  location: location
  sku: sku
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

var virtualNetworkRules = [
  for subnetId in subnetIds: {
    subnet: {
      id: subnetId
    }
  }
]

resource serviceBusNetworkRuleSets 'Microsoft.ServiceBus/namespaces/networkRuleSets@2022-10-01-preview' = {
  name: 'default'
  parent: serviceBusNamespace
  properties: {
    publicNetworkAccess: 'Disabled'
    defaultAction: 'Deny'
    virtualNetworkRules: virtualNetworkRules
  }
}
