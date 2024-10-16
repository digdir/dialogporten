import { uniqueResourceName } from '../../functions/resourceName.bicep'

@description('The prefix used for naming resources to ensure unique names')
param namePrefix string

@description('The location where the resources will be deployed')
param location string

@description('The name of the environment Key Vault')
param environmentKeyVaultName string

@description('The name of the secret name(key) in the source key vault to store the PostgreSQL administrator login password')
#disable-next-line secure-secrets-in-params
param srcKeyVaultAdministratorLoginPasswordKey string

@description('The ID of the subnet where the PostgreSQL server will be deployed')
param subnetId string

@description('The ID of the virtual network for the private DNS zone')
param vnetId string

@description('Tags to apply to resources')
param tags object

@export()
type Sku = {
  name: 'Standard_B1ms' | 'Standard_B2s' | 'Standard_B4ms' | 'Standard_B8ms' | 'Standard_B12ms' | 'Standard_B16ms' | 'Standard_B20ms'
  tier: 'Burstable' | 'GeneralPurpose' | 'MemoryOptimized'
}

@description('The SKU of the PostgreSQL server')
param sku Sku

@description('The Key Vault to store the PostgreSQL administrator login password')
@secure()
param srcKeyVault object

@description('The password for the PostgreSQL administrator login')
@secure()
param administratorLoginPassword string

var administratorLogin = 'dialogportenPgAdmin'
var databaseName = 'dialogporten'
var postgresServerNameMaxLength = 63
var postgresServerName = uniqueResourceName('${namePrefix}-postgres', postgresServerNameMaxLength)

// Uncomment the following lines to add logical replication.
// see https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-logical#pre-requisites-for-logical-replication-and-logical-decoding
//var postgresqlConfiguration = {
//	//wal_level: 'logical'
//	//max_worker_processes: '16'

//	// The leading theory is that we are using pgoutput as the replication protocol 
//	// which comes out of the box in postgresql. Therefore we may not need the 
//	// following two lines.
//	//'azure.extensions': 'pglogical'
//	//shared_preload_libraries: 'pglogical'
//}

module saveAdmPassword '../keyvault/upsertSecret.bicep' = {
  name: 'Save_${srcKeyVaultAdministratorLoginPasswordKey}'
  scope: resourceGroup(srcKeyVault.subscriptionId, srcKeyVault.resourceGroupName)
  params: {
    destKeyVaultName: srcKeyVault.name
    secretName: srcKeyVaultAdministratorLoginPasswordKey
    secretValue: administratorLoginPassword
    tags: tags
  }
}

module privateDnsZone '../privateDnsZone/main.bicep' = {
  name: 'postgresqlPrivateDnsZone'
  params: {
    namePrefix: namePrefix
    defaultDomain: '${namePrefix}.postgres.database.azure.com'
    vnetId: vnetId
    tags: tags
  }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: postgresServerName
  location: location
  properties: {
    version: '15'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    storage: { storageSizeGB: 32 }
    dataEncryption: {
      type: 'SystemManaged'
    }
    replicationRole: 'Primary'
    network: {
      delegatedSubnetResourceId: subnetId
      privateDnsZoneArmResourceId: privateDnsZone.outputs.id
    }
  }
  sku: sku
  resource database 'databases' = {
    name: databaseName
    properties: {
      charset: 'UTF8'
      collation: 'en_US.utf8'
    }
  }
  tags: tags
}

module adoConnectionString '../keyvault/upsertSecret.bicep' = {
  name: 'adoConnectionString'
  params: {
    destKeyVaultName: environmentKeyVaultName
    secretName: 'dialogportenAdoConnectionString'
    secretValue: 'Server=${postgres.properties.fullyQualifiedDomainName};Database=${databaseName};Port=5432;User Id=${administratorLogin};Password=${administratorLoginPassword};Ssl Mode=Require;Trust Server Certificate=true;'
    tags: tags
  }
}

module psqlConnectionString '../keyvault/upsertSecret.bicep' = {
  name: 'psqlConnectionString'
  params: {
    destKeyVaultName: environmentKeyVaultName
    secretName: 'dialogportenPsqlConnectionString'
    secretValue: 'psql \'host=${postgres.properties.fullyQualifiedDomainName} port=5432 dbname=${databaseName} user=${administratorLogin} password=${administratorLoginPassword} sslmode=require\''
    tags: tags
  }
}

output adoConnectionStringSecretUri string = adoConnectionString.outputs.secretUri
output psqlConnectionStringSecretUri string = psqlConnectionString.outputs.secretUri
