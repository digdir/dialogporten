param location string
param envVariables array = []
param port int = 8080
param name string
param image string

param containerAppEnvId string

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
output revisionName string = containerApp.properties.latestRevisionName
