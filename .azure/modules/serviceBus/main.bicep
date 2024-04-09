param namePrefix string
param location string

@export()
type Sku = {
  name: 'Basic' | 'Standard' | 'Premium'
  tier: 'Basic' | 'Standard' | 'Premium'
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
