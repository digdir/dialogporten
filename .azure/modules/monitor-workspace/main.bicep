@description('The prefix used for naming resources to ensure unique names')
param namePrefix string

@description('The location where the resources will be deployed')
param location string

@description('Tags to apply to resources')
param tags object

resource monitorWorkspace 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: '${namePrefix}-monitor'
  location: location
  properties: {
    // todo: enable once we have ensured a connection to this monitor workspace https://github.com/digdir/dialogporten/issues/1462
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

resource containerAppEnvironmentDataCollectionEndpoint 'Microsoft.Insights/dataCollectionEndpoints@2023-03-11' = {
  name: '${namePrefix}-cae-dce'
  location: location
  properties: {
    description: 'DCE for Container App Environment'
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
  tags: tags
}

resource containerAppEnvironmentDataCollectionRule 'Microsoft.Insights/dataCollectionRules@2023-03-11' = {
  name: '${namePrefix}-cae-dcr'
  location: location
  properties: {
    description: 'DCR for Container App Environment'
    dataCollectionEndpointId: containerAppEnvironmentDataCollectionEndpoint.id
    dataSources: {
      prometheusForwarder: [
        {
          streams: [
            'Microsoft-PrometheusMetrics'
          ]
          name: 'PrometheusDataSource'
        }
      ]
    }
    destinations: {
      monitoringAccounts: [
        {
          accountResourceId: monitorWorkspace.id
          name: 'MonitoringAccountDestination'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-PrometheusMetrics'
        ]
        destinations: [
          'MonitoringAccountDestination'
        ]
      }
    ]
  }
  tags: tags
}

output monitorWorkspaceId string = monitorWorkspace.id
output monitorWorkspaceName string = monitorWorkspace.name
output containerAppEnvironmentMetricsIngestionEndpoint string = containerAppEnvironmentDataCollectionEndpoint.properties.metricsIngestion.endpoint
output containerAppEnvironmentLogsIngestionEndpoint string = containerAppEnvironmentDataCollectionEndpoint.properties.logsIngestion.endpoint
