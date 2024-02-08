using './main.bicep'

param environment = 'staging'
param location = 'norwayeast'
param imageTag = readEnvironmentVariable('IMAGE_TAG', '')
param containerAppEnvironmentId = readEnvironmentVariable('CONTAINER_APP_ENVIRONMENT_NAME', '')

//secrets
param adoConnectionStringSecretUri = readEnvironmentVariable('ADO_CONNECTION_STRING_SECRET_URI', '')
