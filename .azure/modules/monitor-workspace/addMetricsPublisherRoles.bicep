@description('The name of the Monitor workspace')
param monitorWorkspaceName string

@description('Array of principal IDs to assign the Monitoring Metrics Publisher role to')
param principalIds array

resource monitorWorkspace 'Microsoft.Monitor/accounts@2023-04-03' existing = {
  name: monitorWorkspaceName
}

@description('This is the built-in Monitoring Metrics Publisher role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#monitoring-metrics-publisher')
resource monitoringMetricsPublisherRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '3913510d-42f4-4e42-8a64-420c390055eb'
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  scope: monitorWorkspace
  name: guid(monitorWorkspace.id, principalId, monitoringMetricsPublisherRole.id)
  properties: {
    roleDefinitionId: monitoringMetricsPublisherRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
