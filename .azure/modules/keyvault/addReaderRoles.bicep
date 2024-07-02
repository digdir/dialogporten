param keyvaultName string
param principalIds array

// Key Vault Secrets User
var readerRoleDefinitionId = '4633458b-17de-408a-b874-0445c86b69e6'

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for principalId in principalIds: {
    scope: keyvault
    name: guid(keyvault.id, principalId, readerRoleDefinitionId)
    properties: {
      roleDefinitionId: readerRoleDefinitionId
      principalId: principalId
      principalType: 'ServicePrincipal'
    }
  }
]
