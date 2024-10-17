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

@description('The suffix for the revision of the container app')
param revisionSuffix string

@description('The probes for the container app')
param probes array = []

@export()
type ScaleRule = {
  // add additional types as needed: https://keda.sh/docs/2.15/scalers/
  custom: {
    type: 'cpu' | 'memory'
    metadata: {
      type: 'Utilization'
      value: string
    }
  }
}

@export()
type Scale = {
  minReplicas: int
  maxReplicas: int
  rules: ScaleRule[]
}

@description('The scaling configuration for the container app')
param scale Scale = {
  minReplicas: 1
  maxReplicas: 1
  rules: []
}

// TODO: Refactor to make userAssignedIdentityId a required parameter once all container apps use user-assigned identities
@description('The ID of the user-assigned managed identity (optional)')
param userAssignedIdentityId string = ''

// Container app revision name does not allow '.' character
var cleanedRevisionSuffix = replace(revisionSuffix, '.', '-')

var ipSecurityRestrictions = empty(apimIp)
  ? []
  : [
      {
        name: 'apim'
        action: 'Allow'
        ipAddressRange: apimIp!
      }
    ]

var ingress = {
  targetPort: port
  external: true
  ipSecurityRestrictions: ipSecurityRestrictions
}

var identityConfig = empty(userAssignedIdentityId) ? {
  type: 'SystemAssigned'
} : {
  type: 'UserAssigned'
  userAssignedIdentities: {
    '${userAssignedIdentityId}': {}
  }
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: identityConfig
  properties: {
    configuration: {
      ingress: ingress
    }
    environmentId: containerAppEnvId
    template: {
      revisionSuffix: cleanedRevisionSuffix
      scale: scale
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

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = if (!empty(userAssignedIdentityId)) {
  name: last(split(userAssignedIdentityId, '/'))
}

output identityPrincipalId string = empty(userAssignedIdentityId) ? containerApp.identity.principalId : managedIdentity.properties.principalId
output name string = containerApp.name
output revisionName string = containerApp.properties.latestRevisionName
