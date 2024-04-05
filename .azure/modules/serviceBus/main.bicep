param namePrefix string
param location string

@export()
type Sku = {
  name: 'Basic' | 'Standard' | 'Premium'
}
param sku Sku

// todo: add a service bus here pls
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2023-03-01' = {
  name: 'myServiceBusNamespace'
  location: resourceGroup().location
  sku: sku
  properties: {
    maximumThroughputUnits: 0 // Set according to your needs
  }
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2023-03-01' = {
  name: 'myQueue'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    enableBatchedOperations: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S' // Set to never auto-delete
  }
}

