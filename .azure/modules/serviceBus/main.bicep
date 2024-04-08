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

// todo: add a service bus here pls
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${namePrefix}-service-bus'
  location: location
  sku: sku
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  // todo: resolve what topics to create
  name: '${namePrefix}-service-bus-topic'
  properties: {
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: serviceBusTopic
  // todo: resolve what subscriptions to create
  name: '${namePrefix}-service-bus-subscription'
  properties: {}
}
