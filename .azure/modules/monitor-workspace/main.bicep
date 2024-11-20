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

resource dataCollectionEndpoint 'Microsoft.Insights/dataCollectionEndpoints@2023-03-11' = {
  name: '${namePrefix}-monitor'
  location: location
  properties: {
    description: 'Default DCE created for Monitoring Account'
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
  tags: tags
}

resource dataCollectionRule 'Microsoft.Insights/dataCollectionRules@2023-03-11' = {
  name: '${namePrefix}-monitor'
  location: location
  properties: {
    description: 'Default DCR created for Monitoring Account'
    dataCollectionEndpointId: dataCollectionEndpoint.id
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
output monitorMetricsIngestionEndpoint string = dataCollectionEndpoint.properties.metricsIngestion.endpoint
output monitorLogsIngestionEndpoint string = dataCollectionEndpoint.properties.logsIngestion.endpoint
