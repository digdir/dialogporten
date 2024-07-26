import { uniqueResourceName } from '../../functions/resourcePostFix.bicep'

param namePrefix string
param location string
param environmentKeyVaultName string
param srcSecretName string
param subnetId string
param vnetId string

@export()
type Sku = {
  name: 'Standard_B1ms'
  tier: 'Burstable' | 'GeneralPurpose' | 'MemoryOptimized'
}
param sku Sku

@secure()
param srcKeyVault object

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
  name: 'Save_${srcSecretName}'
  scope: resourceGroup(srcKeyVault.subscriptionId, srcKeyVault.resourceGroupName)
  params: {
    destKeyVaultName: srcKeyVault.name
    secretName: srcSecretName
    secretValue: administratorLoginPassword
  }
}

module privateDnsZone '../privateDnsZone/main.bicep' = {
  name: 'postgresqlPrivateDnsZone'
  params: {
    namePrefix: namePrefix
    defaultDomain: '${namePrefix}.postgres.database.azure.com'
    vnetId: vnetId
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
}

module adoConnectionString '../keyvault/upsertSecret.bicep' = {
  name: 'adoConnectionString'
  params: {
    destKeyVaultName: environmentKeyVaultName
    secretName: 'dialogportenAdoConnectionString'
    secretValue: 'Server=${postgres.properties.fullyQualifiedDomainName};Database=${databaseName};Port=5432;User Id=${administratorLogin};Password=${administratorLoginPassword};Ssl Mode=Require;Trust Server Certificate=true;'
  }
}

module psqlConnectionString '../keyvault/upsertSecret.bicep' = {
  name: 'psqlConnectionString'
  params: {
    destKeyVaultName: environmentKeyVaultName
    secretName: 'dialogportenPsqlConnectionString'
    secretValue: 'psql \'host=${postgres.properties.fullyQualifiedDomainName} port=5432 dbname=${databaseName} user=${administratorLogin} password=${administratorLoginPassword} sslmode=require\''
  }
}

output adoConnectionStringSecretUri string = adoConnectionString.outputs.secretUri
output psqlConnectionStringSecretUri string = psqlConnectionString.outputs.secretUri
