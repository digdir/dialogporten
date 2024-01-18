param namePrefix string
param location string
param sku string

resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
    name: '${namePrefix}-insightsWorkspace'
    location: location
    properties: {
        retentionInDays: 30
        sku: {
            name: sku
        }
        workspaceCapping: {
            dailyQuotaGb: -1
        }
    }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
    name: '${namePrefix}-applicationInsights'
    location: location
    kind: 'web'
    properties: {
        Application_Type: 'web'
        WorkspaceResourceId: appInsightsWorkspace.id
        Flow_Type: 'Bluefield'
        Request_Source: 'rest'
    }
}

output connectionString string = appInsights.properties.ConnectionString
output appInsightsWorkspaceName string = appInsightsWorkspace.name
output appInsightsName string = appInsights.name
