param location string
param name string
param image string
param containerAppEnvId string
param environmentVariables { name: string, value: string?, secretRef: string? }[] = []
param secrets { name: string, keyVaultUrl: string, identity: 'System' }[] = []

resource job 'Microsoft.App/jobs@2023-05-01' = {
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
}

output identityPrincipalId string = job.identity.principalId
output name string = job.name
