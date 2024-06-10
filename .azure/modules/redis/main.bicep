param namePrefix string
param location string
param subnetId string
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
    // todo: disable public access once we know the private link is working
    publicNetworkAccess: 'Enabled'
  }
}

resource redisPrivateEndpoint 'Microsoft.Network/privateEndpoints@2022-01-01' = {
  name: '${namePrefix}-redis-pe'
  location: location
  properties: {
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}-plsc'
        properties: {
          privateLinkServiceId: redis.id
          groupIds: [
            'redisCache'
          ]
        }
      }
    ]
    subnet: {
      id: subnetId
    }
  }
}

module privateDnsZoneGroup '../privateDnsZoneGroup/main.bicep' = {
  name: '${namePrefix}-redis-privateDnsZoneGroup'
  params: {
    // the private DNS Zone is created automatically by Azure, so we just want to reference it
    // https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-private-link#how-do-i-connect-to-my-cache-with-private-endpoint
    dnsZoneName: 'privatelink.redis.cache.windows.net'
    privateEndpointName: redisPrivateEndpoint.name
    namePrefix: namePrefix
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
