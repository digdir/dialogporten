param dnsZoneId string
param privateEndpointName string
param namePrefix string

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-04-01' existing = {
  name: privateEndpointName
}

resource pe_dns_zone_group 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-04-01' = {
  name: '${namePrefix}-pe-dzg'
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: '${namePrefix}-pe-dzg'
        properties: {
          privateDnsZoneId: dnsZoneId
        }
      }
    ]
  }
}
