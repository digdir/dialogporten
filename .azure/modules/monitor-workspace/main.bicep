@description('The prefix used for naming resources to ensure unique names')
param namePrefix string

@description('The location where the resources will be deployed')
param location string

@description('The ID of the subnet for the Private Link')
param subnetId string

@description('Tags to apply to resources')
param tags object

@description('The ID of the virtual network for the private DNS zone')
param vnetId string

resource monitorWorkspace 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: '${namePrefix}-monitor'
  location: location
  properties: {
    // todo: enable once we have a use case for it https://github.com/digdir/dialogporten/issues/1462
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

// private endpoint name max characters is 80
var monitorPrivateEndpointName = '${namePrefix}-monitor-pe'

resource monitorPrivateEndpoint 'Microsoft.Network/privateEndpoints@2024-03-01' = {
  name: monitorPrivateEndpointName
  location: location
  properties: {
    privateLinkServiceConnections: [
      {
        name: monitorPrivateEndpointName
        properties: {
          privateLinkServiceId: monitorWorkspace.id
          groupIds: [
            'prometheusMetrics'
          ]
        }
      }
    ]
    customNetworkInterfaceName: '${namePrefix}-monitor-pe-nic'
    subnet: {
      id: subnetId
    }
  }
  tags: tags
}

module privateDnsZone '../privateDnsZone/main.bicep' = {
  name: '${namePrefix}-monitor-pdz'
  params: {
    namePrefix: namePrefix
    defaultDomain: 'privatelink.${location}.prometheus.monitor.azure.com'
    vnetId: vnetId
    tags: tags
  }
}

module privateDnsZoneGroup '../privateDnsZoneGroup/main.bicep' = {
  name: '${namePrefix}-monitor-privateDnsZoneGroup'
  dependsOn: [
    privateDnsZone
  ]
  params: {
    name: 'default'
    dnsZoneGroupName: 'privatelink-${location}-prometheus-monitor-azure-com'
    dnsZoneId: privateDnsZone.outputs.id
    privateEndpointName: monitorPrivateEndpoint.name
  }
}

output monitorWorkspaceId string = monitorWorkspace.id
output monitorWorkspaceName string = monitorWorkspace.name
