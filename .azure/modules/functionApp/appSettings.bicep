param webAppName string
param appSettings object
param currentAppSettings object

resource webApp 'Microsoft.Web/sites@2023-12-01' existing = {
    name: webAppName
}

resource siteconfig 'Microsoft.Web/sites/config@2023-12-01' = {
    parent: webApp
    name: 'appsettings'
    properties: union(currentAppSettings, appSettings)
}
