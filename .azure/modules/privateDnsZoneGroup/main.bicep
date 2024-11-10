param dnsZoneId string
param privateEndpointName string
param name string
param dnsZoneGroupName string

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-11-01' existing = {
  name: privateEndpointName
}

resource pe_dns_zone_group 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2024-03-01' = {
  name: name
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: dnsZoneGroupName
        properties: {
          privateDnsZoneId: dnsZoneId
        }
      }
    ]
  }
}
