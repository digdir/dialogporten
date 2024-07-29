param namePrefix string
param location string
param tags object

@export()
type Sku = {
  name: 'premium' | 'standard'
  family: 'A'
}
param sku Sku

var keyVaultName = take('${namePrefix}-kv-${uniqueString(resourceGroup().id)}', 24)

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    enablePurgeProtection: true
    enabledForTemplateDeployment: false
    sku: sku
    tenantId: subscription().tenantId
    accessPolicies: []
    enableRbacAuthorization: true
  }
  tags: tags
}

output name string = keyVault.name
