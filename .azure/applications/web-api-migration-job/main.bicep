targetScope = 'resourceGroup'

param imageTag string

param environment string
param location string

// todo: this needs to be output from infrastructure.bicep 
param containerAppEnvironmentId string

@secure()
// param postgresql.outputs.adoConnectionStringSecretUri string
param adoConnectionStringSecretUri string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

var name = '${namePrefix}-migration-job'

module migrationJob '../../modules/containerAppJob/main.bicep' = {
  // todo: change to use the name of the app
  name: name
  params: {
    name: name
    location: location
    image: '${baseImageUrl}migration-bundle:${imageTag}'
    containerAppEnvId: containerAppEnvironmentId
    // todo: make more generic
    adoConnectionStringSecretUri: adoConnectionStringSecretUri
  }
}

// todo: use this identity principal id to add reader roles etc
output identityPrincipalId string = migrationJob.outputs.identityPrincipalId
output name string = migrationJob.outputs.name
