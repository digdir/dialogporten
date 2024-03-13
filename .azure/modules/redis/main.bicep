param namePrefix string
param location string
@minLength(1)
param environmentKeyVaultName string
@minLength(1)
param version string

@export()
type Sku = {
  name: 'Basic' | 'Standard' | 'Premium'
  family: 'C' | 'P'
  @minValue(1)
  capacity: int
}
param sku Sku

// https://learn.microsoft.com/en-us/azure/templates/microsoft.cache/redis?pivots=deployment-language-bicep
resource redis 'Microsoft.Cache/Redis@2023-08-01' = {
  name: '${namePrefix}-redis'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    sku: sku
    enableNonSslPort: false
    redisConfiguration: {
      'aad-enabled': 'true'
      'maxmemory-policy': 'allkeys-lru'
    }
    redisVersion: version
  }
}

// Until managed identity is supported in the Redis for IDistributedCache, we need to use a connection string
// https://github.com/dotnet/aspnetcore/issues/54414
module redisConnectionString '../keyvault/upsertSecret.bicep' = {
  name: 'redisConnectionString'
  params: {
    destKeyVaultName: environmentKeyVaultName
    secretName: 'dialogportenRedisConnectionString'
    // disable public access? Use vnet here maybe?
    secretValue: '${redis.properties.hostName}:${redis.properties.sslPort},password=${redis.properties.accessKeys.primaryKey},ssl=True,abortConnect=False'
  }
}

output connectionStringSecretUri string = redisConnectionString.outputs.secretUri
