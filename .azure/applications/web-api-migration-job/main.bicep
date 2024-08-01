targetScope = 'resourceGroup'

@description('The tag of the image to be used')
@minLength(3)
param imageTag string

@description('The environment for the deployment')
@minLength(3)
param environment string

@description('The location where the resources will be deployed')
@minLength(3)
param location string

@description('The name of the container app environment')
@minLength(3)
@secure()
param containerAppEnvironmentName string

@description('The name of the Key Vault for the environment')
@minLength(3)
@secure()
param environmentKeyVaultName string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'
var tags = {
  Environment: environment
  Product: 'Dialogporten'
}
var name = '${namePrefix}-db-migration-job'

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvironmentName
}

var containerAppEnvVars = [
  {
    name: 'Infrastructure__DialogDbConnectionString'
    secretRef: 'dbconnectionstring'
  }
]

// https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/bicep-functions-deployment#example-1
var keyVaultUrl = 'https://${environmentKeyVaultName}${az.environment().suffixes.keyvaultDns}/secrets/dialogportenAdoConnectionString'

var secrets = [
  {
    name: 'dbconnectionstring'
    keyVaultUrl: keyVaultUrl
    identity: 'System'
  }
]

module migrationJob '../../modules/containerAppJob/main.bicep' = {
  name: name
  params: {
    name: name
    location: location
    image: '${baseImageUrl}migration-bundle:${imageTag}'
    containerAppEnvId: containerAppEnvironment.id
    environmentVariables: containerAppEnvVars
    secrets: secrets
    tags: tags
  }
}

module keyVaultReaderAccessPolicy '../../modules/keyvault/addReaderRoles.bicep' = {
  name: 'keyVaultReaderAccessPolicy-${name}'
  params: {
    keyvaultName: environmentKeyVaultName
    principalIds: [migrationJob.outputs.identityPrincipalId]
  }
}

output identityPrincipalId string = migrationJob.outputs.identityPrincipalId
output name string = migrationJob.outputs.name
