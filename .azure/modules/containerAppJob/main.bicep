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

@description('The cron expression for the job schedule (optional)')
param cronExpression string = ''

@description('The entrypoint command for the job')
param entrypoint string = ''

var isScheduled = !empty(cronExpression)

var scheduledJobProperties = {
  triggerType: 'Schedule'
  scheduleTriggerConfig: {
    cronExpression: cronExpression
  }
}

var manualJobProperties = {
  triggerType: 'Manual'
  manualTriggerConfig: {
    parallelism: 1
    replicaCompletionCount: 1
  }
}

resource job 'Microsoft.App/jobs@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    configuration: union(
      {
        secrets: secrets
        replicaRetryLimit: 1
        replicaTimeout: 120
      },
      isScheduled ? scheduledJobProperties : manualJobProperties
    )
    environmentId: containerAppEnvId
    template: {
      containers: [
        {
          env: environmentVariables
          image: image
          name: name
          command: empty(entrypoint) ? null : [entrypoint]
        }
      ]
    }
  }
  tags: tags
}

output identityPrincipalId string = job.identity.principalId
output name string = job.name
