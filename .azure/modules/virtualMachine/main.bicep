param name string
param location string
param tags object

type HardwareProfile = {
  vmSize: string
}
@description('Specifies the hardware profile for the virtual machine')
param hardwareProfile HardwareProfile

type AdditionalCapabilities = {
  hibernationEnabled: bool
}
@description('Specifies the additional capabilities for the virtual machine')
param additionalCapabilities AdditionalCapabilities

type SecurityProfile = {
  uefiSettings: {
    secureBootEnabled: bool
    vTpmEnabled: bool
  }
  securityType: string
}
@description('Specifies the security profile for the virtual machine')
param securityProfile SecurityProfile

type NetworkInterface = {
  id: string
  properties: {
    deleteOption: string
  }
}
type NetworkProfile = {
  networkInterfaces: NetworkInterface[]
}
@description('Specifies the network profile for the virtual machine')
param networkProfile NetworkProfile

type DiagnosticsProfile = {
  bootDiagnostics: {
    enabled: bool
  }
}
@description('Specifies the diagnostics profile for the virtual machine')
param diagnosticsProfile DiagnosticsProfile

type StorageProfile = {
  imageReference: {
    publisher: string
    offer: string
    sku: string
    version: string
  }
  osDisk: {
    osType: string
    name: string
    createOption: string
    caching: string
    managedDisk: {
      storageAccountType: string
    }
    deleteOption: string
    diskSizeGB: int
  }
  dataDisks: array
  diskControllerType: string
}
@description('Specifies the storage profile for the virtual machine')
param storageProfile StorageProfile

@description('Specifies the SSH key data for the virtual machine')
@secure()
param sshKeyData string

resource virtualMachine 'Microsoft.Compute/virtualMachines@2024-03-01' = {
  name: name
  location: location
  zones: [
    '1'
  ]
  properties: {
    hardwareProfile: hardwareProfile
    additionalCapabilities: additionalCapabilities
    storageProfile: storageProfile
    osProfile: {
      computerName: name
      adminUsername: name
      linuxConfiguration: {
        disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${name}/.ssh/authorized_keys'
              keyData: sshKeyData
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
    securityProfile: securityProfile
    networkProfile: networkProfile
    diagnosticsProfile: diagnosticsProfile
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: tags
}

resource aadLoginExtension 'Microsoft.Compute/virtualMachines/extensions@2023-03-01' = {
  parent: virtualMachine
  name: 'AADSSHLoginForLinux'
  location: location
  properties: {
    publisher: 'Microsoft.Azure.ActiveDirectory'
    type: 'AADSSHLoginForLinux'
    typeHandlerVersion: '1.0'
    autoUpgradeMinorVersion: true
  }
}
