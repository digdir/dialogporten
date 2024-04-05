targetScope = 'resourceGroup'

@minLength(3)
param imageTag string
@minLength(3)
param environment string
@minLength(3)
param location string

@minLength(3)
@secure()
param containerAppEnvironmentName string
@minLength(3)
@secure()
param adoConnectionStringSecretUri string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

var name = '${namePrefix}-db-migration-job-aretest'

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

module migrationJob '../../modules/containerAppJob/main.bicep' = {
  name: name
  params: {
    name: name
    location: location
    image: '${baseImageUrl}migration-bundle:${imageTag}'
    containerAppEnvId: containerAppEnvironment.id
    adoConnectionStringSecretUri: adoConnectionStringSecretUri
  }
}

output identityPrincipalId string = migrationJob.outputs.identityPrincipalId
output name string = migrationJob.outputs.name
