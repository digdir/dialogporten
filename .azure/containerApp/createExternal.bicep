param location string
param namePrefix string
param gitSha string
param envVariables array = []
param baseImageUrl string
param port int = 8080
param apiManagementIp string
param appInsightsWorkspaceName string
param adoConnectionStringSecretUri string

@secure()
param migrationVerifierPrincipalPassword string
@secure()
param migrationVerifierPrincipalAppId string

resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
	name: appInsightsWorkspaceName
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${namePrefix}-cae'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: appInsightsWorkspace.properties.customerId
        sharedKey: appInsightsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

resource migrationJob 'Microsoft.App/jobs@2023-05-01' = {
  name: '${namePrefix}-migration-job'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    configuration: {
      secrets: [
        {
          name: 'adoconnectionstringsecreturi'
          keyVaultUrl: adoConnectionStringSecretUri
          identity: 'System'
        }
      ]
      manualTriggerConfig: {
        parallelism: 1
        replicaCompletionCount: 1
      }
      replicaRetryLimit: 1
      replicaTimeout: 30
      triggerType: 'Manual'
    }
    environmentId: containerAppEnv.id
    template: {
      containers: [
        {
          env: [
            {
              name: 'Infrastructure__DialogDbConnectionString'
              secretRef: 'adoconnectionstringsecreturi'
            }
          ]
          image: '${baseImageUrl}migration-bundle:${gitSha}'
          name: 'migration-bundle'
        }
      ]
    }
  }
}

var initContainers = [
  {
    name: 'migration-verifier-init'
    // Temp. hardcoded tag, waiting for fix on
    // https://github.com/Azure/azure-sdk-for-net/issues/38385
    image:'${baseImageUrl}migration-verifier:05c2045'
    env: concat(envVariables,
    [
      {
        name: 'AZURE_TENANT_ID'
        value: subscription().tenantId
      }
      {
        name: 'AZURE_CLIENT_ID'
        value: migrationVerifierPrincipalAppId
      }
      {
        name: 'AZURE_CLIENT_SECRET'
        value: migrationVerifierPrincipalPassword
      }
      {
        name: 'MIGRATION_JOB_NAME'
        value: migrationJob.name
      }
    ])
  }]

var probes = [
  {
    periodSeconds: 5
    initialDelaySeconds: 2
    type: 'Liveness'
    httpGet: {
      path: '/healthz'
      port: port
    }
  }
  {
    periodSeconds: 5
    initialDelaySeconds: 2
    type: 'Readiness'
    httpGet: {
      path: '/healthz'
      port: port
    }
  }
]

var ingress = {
  targetPort: port
  external: true
  ipSecurityRestrictions: [
    {
      name: 'allow-apim-ip'
      action: 'Allow'
      ipAddressRange: apiManagementIp
    }
  ]
}

var webapiSoName = '${namePrefix}-webapi-so-ca'
resource webapiSo 'Microsoft.App/containerApps@2023-05-01' = {
  name: webapiSoName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppEnv.id
    configuration: {
      ingress: ingress
    }
    template: {
      scale: {
        minReplicas: 1
        maxReplicas: 1 // temp disable scaling for outbox scheduling
      }
      initContainers: initContainers
      containers: [
        {
          name: 'webapi-so'
          image: '${baseImageUrl}webapi:${gitSha}'
          env: concat(envVariables, [{
            name: 'RUN_OUTBOX_SCHEDULER'
            value: 'true'
          }])
          probes: probes
        }
      ]
    }
  }
}

var webapiEuName = '${namePrefix}-webapi-eu-ca'
resource webapiEu 'Microsoft.App/containerApps@2023-05-01' = {
  name: webapiEuName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  
  properties: {
    configuration: {
      ingress: ingress
    }
    environmentId: containerAppEnv.id
    template: {
      scale: {
        minReplicas: 1
        maxReplicas: 5
      }
      initContainers: initContainers
      containers: [
        {
          name: 'webapi-eu'
          image: '${baseImageUrl}webapi:${gitSha}'
          env: envVariables
          probes: probes
        }
      ]
    }
  }
}

output identityPrincipalIds array = [
  webapiSo.identity.principalId
  webapiEu.identity.principalId
  migrationJob.identity.principalId
]

output containerAppEnvName string = containerAppEnv.name
output webApiSoName string = webapiSo.name
output webApiEuName string = webapiEu.name
output migrationJobName string = migrationJob.name
