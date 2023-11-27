param websiteName string
param settings object

resource website 'Microsoft.Web/sites@2022-03-01' existing = {
	name: websiteName
}

resource appsettings 'Microsoft.Web/sites/config@2022-09-01' = {
    parent: website
    name: 'appsettings'
    properties: settings
}