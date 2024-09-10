@description('The location where the resources will be deployed')
param location string

@description('The environment variables for the container app')
param envVariables array = []

@description('The port on which the container app will run')
param port int = 8080

@description('The name of the container app')
param name string

@description('The image to be used for the container app')
param image string

@description('The IP address of the API Management instance')
param apimIp string?

@description('The ID of the container app environment')
param containerAppEnvId string

@description('The tags to be applied to the container app')
param tags object

@description('CPU and memory resources for the container app')
param resources object?

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

var ipSecurityRestrictions = empty(apimIp)
  ? []
  : [
    //   {
    //     name: 'apim'
    //     action: 'Allow'
    //     ipAddressRange: apimIp!
    //   }
    ]

var ingress = {
  targetPort: port
  external: true
  ipSecurityRestrictions: ipSecurityRestrictions
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
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
          env: envVariables
          probes: probes
          resources: resources
        }
      ]
    }
  }
  tags: tags
}

output identityPrincipalId string = containerApp.identity.principalId
output name string = containerApp.name
output revisionName string = containerApp.properties.latestRevisionName
