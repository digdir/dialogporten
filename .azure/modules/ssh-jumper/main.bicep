@description('The name prefix to be used for the resource')
param namePrefix string

@description('The location to deploy the resource to')
param location string

@description('The subnet to deploy the network interface to')
param subnetId string

@description('Tags to be applied to the resource')
param tags object

@description('The name of the source Key Vault')
param srcKeyVaultName string

@description('The subscription ID of the source Key Vault')
param srcKeyVaultSubId string

@description('The resource group name of the source Key Vault')
param srcKeyVaultRGNName string

var name = '${namePrefix}-jumper'

resource srcKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: srcKeyVaultName
  scope: resourceGroup(srcKeyVaultSubId, srcKeyVaultRGNName)
}

resource publicIp 'Microsoft.Network/publicIPAddresses@2023-11-01' = {
  name: '${name}-ip'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  zones: [
    '1'
  ]
  properties: {
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []
  }
  tags: tags
}

resource networkInterface 'Microsoft.Network/networkInterfaces@2023-11-01' = {
  name: name
  location: location
  properties: {
    ipConfigurations: [
      {
        name: '${name}-ipconfig'
        type: 'Microsoft.Network/networkInterfaces/ipConfigurations'
        properties: {
          privateIPAddress: '10.0.0.5'
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIp.id
            properties: {
              deleteOption: 'Delete'
            }
          }
          subnet: {
            id: subnetId
          }
          primary: true
          privateIPAddressVersion: 'IPv4'
        }
      }
    ]
    dnsSettings: {
      dnsServers: []
    }
    enableAcceleratedNetworking: false
    enableIPForwarding: false
    disableTcpStateTracking: false
    nicType: 'Standard'
    auxiliaryMode: 'None'
    auxiliarySku: 'None'
  }
}

module virtualMachine '../../modules/virtualMachine/main.bicep' = {
  name: name
  params: {
    name: name
    // todo: remove hardcoded environment, use naming convention here. 
    sshKeyData: srcKeyVaultResource.getSecret('dialogportenJumperTestSSH')
    location: location
    tags: tags
    hardwareProfile: {
      vmSize: 'Standard_B1s'
    }
    additionalCapabilities: {
      hibernationEnabled: false
    }
    storageProfile: {
      imageReference: {
        publisher: 'canonical'
        offer: '0001-com-ubuntu-server-focal'
        sku: '20_04-lts-gen2'
        version: 'latest'
      }
      osDisk: {
        osType: 'Linux'
        name: '${name}-osdisk'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        managedDisk: {
          storageAccountType: 'Premium_LRS'
        }
        deleteOption: 'Delete'
        diskSizeGB: 30
      }
      dataDisks: []
      diskControllerType: 'SCSI'
    }
    securityProfile: {
      uefiSettings: {
        secureBootEnabled: true
        vTpmEnabled: true
      }
      securityType: 'TrustedLaunch'
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: networkInterface.id
          properties: {
            deleteOption: 'Delete'
          }
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}
