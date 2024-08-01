@description('The name prefix to be used for the resource')
param namePrefix string

@description('The location to deploy the resource to')
param location string

@description('The subnet to deploy the network interface to')
param subnetId string

@description('Tags to be applied to the resource')
param tags object

var name = '${namePrefix}-jumper'

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

resource imageReference 'Microsoft.Compute/images@2024-03-01' existing = {
  id: '/Subscriptions/8a353de8-d81d-468d-a40d-f3574b6bb3f4/Providers/Microsoft.Compute/Locations/NorwayEast/Publishers/canonical/ArtifactTypes/VMImage/Offers/0001-com-ubuntu-server-focal/Skus/20_04-lts-gen2/Versions/20.04.202407150'
}

resource osDisk 'Microsoft.Compute/disks@2023-10-02' = {
  name: '${name}-osdisk'
  location: location
  sku: {
    name: 'Premium_LRS'
  }
  zones: [
    '1'
  ]
  properties: {
    osType: 'Linux'
    hyperVGeneration: 'V2'
    supportsHibernation: true
    supportedCapabilities: {
      diskControllerTypes: 'SCSI, NVMe'
      acceleratedNetwork: true
      architecture: 'x64'
    }
    creationData: {
      createOption: 'FromImage'
      imageReference: {
        id: '/Subscriptions/8a353de8-d81d-468d-a40d-f3574b6bb3f4/Providers/Microsoft.Compute/Locations/NorwayEast/Publishers/canonical/ArtifactTypes/VMImage/Offers/0001-com-ubuntu-server-focal/Skus/20_04-lts-gen2/Versions/20.04.202407150'
      }
    }
    diskSizeGB: 30
    diskIOPSReadWrite: 120
    diskMBpsReadWrite: 25
    encryption: {
      type: 'EncryptionAtRestWithPlatformKey'
    }
    networkAccessPolicy: 'AllowAll'
    securityProfile: {
      securityType: 'TrustedLaunch'
    }
    publicNetworkAccess: 'Enabled'
    tier: 'P4'
  }
}

resource sshPublicKey 'Microsoft.Compute/sshPublicKeys@2023-09-01' = {
  name: '${name}-ssh'
  location: location
  properties: {}
  tags: tags
}

resource virtualMachine 'Microsoft.Compute/virtualMachines@2024-03-01' = {
  name: name
  location: location
  zones: [
    '1'
  ]
  properties: {
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
          id: osDisk.id
        }
        deleteOption: 'Delete'
        diskSizeGB: 30
      }
      dataDisks: []
      diskControllerType: 'SCSI'
    }
    osProfile: {
      computerName: name
      adminUsername: name
      linuxConfiguration: {
        disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${name}/.ssh/authorized_keys'
              keyData: sshPublicKey.properties.publicKey
            }
          ]
        }
        provisionVMAgent: true
        patchSettings: {
          patchMode: 'AutomaticByPlatform'
          automaticByPlatformSettings: {
            rebootSetting: 'IfRequired'
            bypassPlatformSafetyChecksOnUserSchedule: false
          }
          assessmentMode: 'ImageDefault'
        }
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
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
