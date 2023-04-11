param destKeyVaultName string
param secretName string
@secure()
param secretValue string

resource cert 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = {
  name: '${destKeyVaultName}/${secretName}'
  properties: {
    value: secretValue
  }
}