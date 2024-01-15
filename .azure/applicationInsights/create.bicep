param namePrefix string
param location string

resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
    name: '${namePrefix}-insightsWorkspace'
    location: location
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
    name: '${namePrefix}-applicationInsights'
    location: location
    kind: 'web'
    properties: {
        Application_Type: 'web'
        WorkspaceResourceId: appInsightsWorkspace.id
    }
}

output connectionString string = appInsights.properties.ConnectionString
output appInsightsWorkspaceName string = appInsightsWorkspace.name
output appInsightsName string = appInsights.name
