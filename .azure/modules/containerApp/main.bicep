param location string
param initContainerimage string
param envVariables array = []
param port int = 8080
param name string
param image string

param containerAppEnvId string

// todo: do we need this here? 🤔
param migrationJobName string

@secure()
param migrationVerifierPrincipalPassword string
@secure()
param migrationVerifierPrincipalAppId string

// todo: refactor out the init containers & env variables
var initContainers = [
  {
    name: '${name}-init'
    image: initContainerimage
    env: concat(envVariables,
      [
        {
          name: 'AZURE_TENANT_ID'
          value: subscription().tenantId
        }
        {
          name: 'SUBSCRIPTION_ID'
          value: subscription().subscriptionId
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: migrationVerifierPrincipalAppId
        }
        {
          name: 'AZURE_CLIENT_SECRET'
          value: migrationVerifierPrincipalPassword
        }
        {
          name: 'MIGRATION_JOB_NAME'
          value: migrationJobName
        }
        {
          name: 'RESOURCE_GROUP_NAME'
          value: resourceGroup().name
        }
      ])
  } ]

var probes = [
  {
    periodSeconds: 5
    initialDelaySeconds: 2
    type: 'Liveness'
    httpGet: {
      path: '/healthz'
      port: port
    }
  }
  {
    periodSeconds: 5
    initialDelaySeconds: 2
    type: 'Readiness'
    httpGet: {
      path: '/healthz'
      port: port
    }
  }
]

var ingress = {
  targetPort: port
  external: true
  ipSecurityRestrictions: []
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    configuration: {
      ingress: ingress
    }
    environmentId: containerAppEnvId
    template: {
      scale: {
        minReplicas: 1
        maxReplicas: 1 // temp disable scaling for outbox scheduling
      }
      initContainers: initContainers
      containers: [
        {
          name: name
          image: image
          env: concat(envVariables, [ {
                name: 'RUN_OUTBOX_SCHEDULER'
                value: 'true'
              } ])
          probes: probes
        }
      ]
    }
  }
}

output identityPrincipalId string = containerApp.identity.principalId
output name string = containerApp.name
