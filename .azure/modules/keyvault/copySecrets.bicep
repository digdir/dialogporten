// Source
param srcKeyVaultKeys array
param srcKeyVaultName string
param srcKeyVaultRGNName string = resourceGroup().name
param srcKeyVaultSubId string = subscription().subscriptionId

// Destination
param destKeyVaultName string
param destKeyVaultRGName string = resourceGroup().name
param destKeyVaultSubId string = subscription().subscriptionId

// App configuration
param appConfigurationName string

// Secret
#disable-next-line secure-secrets-in-params
param secretPrefix string

var filteredKeysBySecretPrefix = filter(srcKeyVaultKeys, key => startsWith(key, secretPrefix))

var keys = map(
  filteredKeysBySecretPrefix,
  key => {
    secretNameWithoutPrefix: replace(key, secretPrefix, '')
    secretName: key
    appConfigKey: replace(replace(key, secretPrefix, ''), '--', ':')
  }
)

resource srcKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: srcKeyVaultName
  scope: resourceGroup(srcKeyVaultSubId, srcKeyVaultRGNName)
}

resource appConfigurationResource 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigurationName
}

module secrets 'upsertSecret.bicep' = [
  for key in keys: {
    name: '${take(key.secretName, 57)}-${take(uniqueString(key.secretName), 6)}'
    scope: resourceGroup(destKeyVaultSubId, destKeyVaultRGName)
    params: {
      destKeyVaultName: destKeyVaultName
      secretName: key.secretNameWithoutPrefix
      secretValue: srcKeyVaultResource.getSecret(key.secretName)
    }
  }
]

module appConfiguration '../appConfiguration/upsertKeyValue.bicep' = [
  for key in keys: {
    name: '${take(key.secretNameWithoutPrefix, 57)}-${take(uniqueString(key.secretNameWithoutPrefix), 6)}'
    scope: resourceGroup(destKeyVaultSubId, destKeyVaultRGName)
    params: {
      configStoreName: appConfigurationResource.name
      key: key.appConfigKey
      value: 'https://${destKeyVaultName}${az.environment().suffixes.keyvaultDns}/secrets/${key.secretNameWithoutPrefix}'
      keyValueType: 'keyVaultReference'
    }
  }
]
