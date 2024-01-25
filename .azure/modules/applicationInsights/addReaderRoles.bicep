param appInsightsName string
param principalIds array

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
    name: appInsightsName
}

@description('This is the built-in Reader role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#reader')
resource readerRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
    scope: subscription()
    name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
    scope: appInsights
    name: guid(subscription().id, principalId, readerRole.id)
    properties: {
        roleDefinitionId: readerRole.id
        principalId: principalId
        principalType: 'ServicePrincipal'
    }
}]
