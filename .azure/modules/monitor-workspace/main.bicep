@description('The prefix used for naming resources to ensure unique names')
param namePrefix string

@description('The location where the resources will be deployed')
param location string

@description('Tags to apply to resources')
param tags object

resource monitorWorkspace 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: '${namePrefix}-monitor'
  location: location
  properties: {
    // todo: enable once we have ensured a connection to this monitor workspace https://github.com/digdir/dialogporten/issues/1462
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

output monitorWorkspaceId string = monitorWorkspace.id
output monitorWorkspaceName string = monitorWorkspace.name
