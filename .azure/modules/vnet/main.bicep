@description('The prefix used for naming resources to ensure unique names')
param namePrefix string

@description('The location where the resources will be deployed')
param location string

resource defaultNSG 'Microsoft.Network/networkSecurityGroups@2023-11-01' = {
  name: '${namePrefix}-default-nsg'
  location: location
  properties: {
    securityRules: [
      // todo: restrict the ports further
      {
        name: 'AllowAnyCustomAnyInbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Inbound'
        }
      }
      {
        name: 'AllowAnyCustomAnyOutbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Outbound'
        }
      }
    ]
  }
}

// https://learn.microsoft.com/en-us/azure/container-apps/firewall-integration?tabs=consumption-only
resource containerAppEnvironmentNSG 'Microsoft.Network/networkSecurityGroups@2023-11-01' = {
  name: '${namePrefix}-container-app-environment-nsg'
  location: location
  properties: {
    securityRules: [
      {
        name: 'AllowAnyCustomAnyOutbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 110
          direction: 'Outbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'container-app'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 120
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: ['80', '443']
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'container-app-environment'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '10.0.0.62'
          access: 'Allow'
          priority: 130
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: ['80', '443']
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'load-balancer'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '30000-32767'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 140
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      // remove once we want a more fine grained control over the ports
      {
        name: 'AllowAnyCustomAnyInbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 150
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
    ]
  }
}

resource postgresqlNSG 'Microsoft.Network/networkSecurityGroups@2023-11-01' = {
  name: '${namePrefix}-postgresql-nsg'
  location: location
  properties: {
    securityRules: [
      // todo: restrict the ports furter: https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-networking-private#virtual-network-concepts
      {
        name: 'AllowAnyCustomAnyInbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Inbound'
        }
      }
      {
        name: 'AllowAnyCustomAnyOutbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Outbound'
        }
      }
    ]
  }
}

resource redisNSG 'Microsoft.Network/networkSecurityGroups@2023-09-01' = {
  name: '${namePrefix}-redis-nsg'
  location: location
  properties: {
    securityRules: [
      // todo: restrict the ports furter: https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-networking-private#virtual-network-concepts
      {
        name: 'AllowAnyCustomAnyInbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Inbound'
        }
      }
      {
        name: 'AllowAnyCustomAnyOutbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Outbound'
        }
      }
    ]
  }
}

resource serviceBusNSG 'Microsoft.Network/networkSecurityGroups@2023-11-01' = {
  name: '${namePrefix}-service-bus-nsg'
  location: location
  properties: {
    securityRules: [
      // todo: make more restrictive
      {
        name: 'AllowAnyCustomAnyInbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Inbound'
        }
      }
      {
        name: 'AllowAnyCustomAnyOutbound'
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 100
          direction: 'Outbound'
        }
      }
    ]
  }
}

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: '${namePrefix}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'default'
        properties: {
          addressPrefix: '10.0.0.0/24'
          networkSecurityGroup: {
            id: defaultNSG.id
          }
        }
      }
      {
        name: 'postgresqlSubnet'
        properties: {
          addressPrefix: '10.0.1.0/24'
          networkSecurityGroup: {
            id: postgresqlNSG.id
          }
          delegations: [
            {
              name: 'postgresql'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
          serviceEndpoints: [
            {
              service: 'Microsoft.Storage'
              locations: [location]
            }
          ]
        }
      }
      {
        name: 'containerAppEnvSubnet'
        properties: {
          // required size for the container app environment is /23
          addressPrefix: '10.0.2.0/23'
          networkSecurityGroup: {
            id: containerAppEnvironmentNSG.id
          }
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: 'serviceBusSubnet'
        properties: {
          addressPrefix: '10.0.4.0/24'
          networkSecurityGroup: {
            id: serviceBusNSG.id
          }
        }
      }
      {
        name: 'redisSubnet'
        properties: {
          addressPrefix: '10.0.5.0/24'
          networkSecurityGroup: {
            id: redisNSG.id
          }
        }
      }
    ]
  }
}

output virtualNetworkName string = virtualNetwork.name
output virtualNetworkId string = virtualNetwork.id
output defaultSubnetId string = virtualNetwork.properties.subnets[0].id
output postgresqlSubnetId string = virtualNetwork.properties.subnets[1].id
output containerAppEnvironmentSubnetId string = virtualNetwork.properties.subnets[2].id
output serviceBusSubnetId string = virtualNetwork.properties.subnets[3].id
output redisSubnetId string = virtualNetwork.properties.subnets[4].id
