@description('The name of the web application')
param webAppName string

@description('The new app settings to be applied')
param appSettings object

@description('The current app settings of the web application')
param currentAppSettings object

resource webApp 'Microsoft.Web/sites@2024-04-01' existing = {
  name: webAppName
}

resource siteconfig 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: webApp
  name: 'appsettings'
  properties: union(currentAppSettings, appSettings)
}
