using 'main.bicep'

// rather use keyvault directly here to access secrets? https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/key-vault-parameter?tabs=azure-cli#use-getsecret-function

param environment = 'dev'
param location = 'norwayeast'
param keyVaultSourceKeys = []
param gitSha = ''

param dialogportenPgAdminPassword = ''
param apiManagementDigDirEmail = ''
param sourceKeyVaultSubscriptionId = ''
param sourceKeyVaultResourceGroup = ''
param sourceKeyVaultName = ''
