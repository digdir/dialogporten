@description('The name of the Service Bus namespace')
param serviceBusNamespaceName string

@description('Array of principal IDs to assign the Azure Service Bus Data Owner role to')
param principalIds array

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

@description('This is the built-in Azure Service Bus Data Owner role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-service-bus-data-owner')
resource serviceBusDataOwnerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '090c5cfd-751d-490a-894a-3ce6f1109419'
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for principalId in principalIds: {
    scope: serviceBusNamespace
    name: guid(serviceBusNamespace.id, principalId, serviceBusDataOwnerRoleDefinition.id)
    properties: {
      roleDefinitionId: serviceBusDataOwnerRoleDefinition.id
      principalId: principalId
      principalType: 'ServicePrincipal'
    }
  }
]
