targetScope = 'resourceGroup'

param imageTag string

param environment string
param location string

param containerAppEnvironmentId string

@secure()
// param postgresql.outputs.adoConnectionStringSecretUri string
param adoConnectionStringSecretUri string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

var name = '${namePrefix}-migration-job'

module migrationJob '../../modules/containerAppJob/main.bicep' = {
  name: name
  params: {
    name: name
    location: location
    image: '${baseImageUrl}migration-bundle:${imageTag}'
    containerAppEnvId: containerAppEnvironmentId
    adoConnectionStringSecretUri: adoConnectionStringSecretUri
  }
}

output identityPrincipalId string = migrationJob.outputs.identityPrincipalId
output name string = migrationJob.outputs.name
