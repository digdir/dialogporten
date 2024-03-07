using './main.bicep'

param environment = 'test'
param location = 'norwayeast'
param imageTag = readEnvironmentVariable('IMAGE_TAG')

//secrets
param containerAppEnvironmentName = readEnvironmentVariable('CONTAINER_APP_ENVIRONMENT_NAME')
param adoConnectionStringSecretUri = readEnvironmentVariable('ADO_CONNECTION_STRING_SECRET_URI')
