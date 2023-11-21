param location string
param namePrefix string
param gitSha string
param envVariables array = []
param baseImageUrl string
param port int = 8080


// TODO: Get env. id param
// resource internalEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
//   name: '${namePrefix}-internal-cae'
//   location: location
//   properties: {
//   }
// }

var scale = {
	maxReplicas: 1
	minReplicas: 1
}

resource rabbitmq 'Microsoft.App/containerApps@2023-05-01' = {
	name: '${namePrefix}-rabbitmq-ca'
	location: location
	identity: {
		type: 'SystemAssigned'
	}
	properties: {
		environmentId: internalEnv.id
		configuration: {
			ingress: {
				external: false
				targetPort: 5672
				transport: 'tcp'
			}
		}
		template: {
			scale: scale
			containers: [
				{
					name: 'rabbitmq'
					image: '${baseImageUrl}rabbitmq:${gitSha}'
					env: [
						{
							name: 'RABBITMQ_USERNAME'
							value: 'test12345'
						}
						{
							name: 'RABBITMQ_PASSWORD'
							value: 'test12345'
						}
					]
				}
			]
		}
	}
}

var envVars = concat(envVariables, [
	{
		name: 'RabbitMq__Host'
		value: rabbitmq.name
	}
	{
		name: 'RabbitMq__Password'
		value: 'test12345'
	}
	{
		name: 'RabbitMq__Username'
		value: 'test12345'
	}
])


resource cdc 'Microsoft.App/containerApps@2023-05-01' = {
	name: '${namePrefix}-cdc-ca'
	location: location
	identity: {
		type: 'SystemAssigned'
	}
	properties: {
		environmentId: internalEnv.id
		template: {
			scale: scale
			containers: [
				{
					name: 'cdc'
					image: '${baseImageUrl}cdc:${gitSha}'
					env: envVars
				}
			]
		}
	}
}

resource service 'Microsoft.App/containerApps@2023-05-01' = {
	name: '${namePrefix}-service-ca'
	location: location
	identity: {
		type: 'SystemAssigned'
	}
	properties: {
		environmentId: internalEnv.id
		template: {
			scale: scale
			containers: [
				{
					name: 'service'
					image: '${baseImageUrl}service:${gitSha}'
					env: envVars
				}
			]
		}
	}
}

output identityPrincipalIds array = [
	cdc.identity.principalId
	service.identity.principalId
]
