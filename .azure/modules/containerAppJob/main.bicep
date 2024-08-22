@description('The location where the resources will be deployed')
param location string

@description('The name of the job')
param name string

@description('The image to be used for the job')
param image string

@description('The ID of the container app environment')
param containerAppEnvId string

@description('The environment variables for the job')
param environmentVariables { name: string, value: string?, secretRef: string? }[] = []

@description('The secrets to be used in the job')
param secrets { name: string, keyVaultUrl: string, identity: 'System' }[] = []

@description('The tags to be applied to the job')
param tags object

resource job 'Microsoft.App/jobs@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    configuration: {
      secrets: secrets
      manualTriggerConfig: {
        parallelism: 1
        replicaCompletionCount: 1
      }
      replicaRetryLimit: 1
      replicaTimeout: 120
      triggerType: 'Manual'
    }
    environmentId: containerAppEnvId
    template: {
      containers: [
        {
          env: environmentVariables
          image: image
          name: name
        }
      ]
    }
  }
  tags: tags
}

output identityPrincipalId string = job.identity.principalId
output name string = job.name
