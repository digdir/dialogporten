using './main.bicep'

param environment = 'test'
param location = 'norwayeast'
param imageTag = readEnvironmentVariable('IMAGE_TAG')
param jobSchedule = '0 */2 * * *' // Runs every 2 hours

//secrets
param containerAppEnvironmentName = readEnvironmentVariable('AZURE_CONTAINER_APP_ENVIRONMENT_NAME')
param environmentKeyVaultName = readEnvironmentVariable('AZURE_ENVIRONMENT_KEY_VAULT_NAME')
