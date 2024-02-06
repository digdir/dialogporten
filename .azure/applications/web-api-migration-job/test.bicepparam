using './main.bicep'

param environment = 'test'
param location = 'norwayeast'

//secrets
param imageTag = readEnvironmentVariable('IMAGE_TAG')
param containerAppEnvironmentId = readEnvironmentVariable('CONTAINER_APP_ENVIRONMENT_ID')
param adoConnectionStringSecretUri = readEnvironmentVariable('ADO_CONNECTION_STRING_SECRET_URI')
