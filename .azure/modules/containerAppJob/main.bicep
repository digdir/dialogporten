param location string
param name string
param image string
param adoConnectionStringSecretUri string

param containerAppEnvId string

resource job 'Microsoft.App/jobs@2023-05-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    configuration: {
      secrets: [
        {
          // todo: move this and refactor into adding this somewhere else
          name: 'dbconnectionstring'
          keyVaultUrl: adoConnectionStringSecretUri
          identity: 'System'
        }
      ]
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
          env: [
            {
              name: 'Infrastructure__DialogDbConnectionString'
              secretRef: 'dbconnectionstring'
            }
          ]
          image: image
          name: name
        }
      ]
    }
  }
}

output identityPrincipalId string = job.identity.principalId
output name string = job.name
