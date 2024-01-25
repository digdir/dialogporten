targetScope = 'subscription'

@minLength(3)
param environment string
param location string
param keyVaultSourceKeys array
param gitSha string

@secure()
param dialogportenPgAdminPassword string
@secure()
param apiManagementDigDirEmail string
@secure()
param sourceKeyVaultSubscriptionId string
@secure()
param sourceKeyVaultResourceGroup string
@secure()
param sourceKeyVaultName string

@allowed(
    [
        'Basic'
        'Consumption'
        'Developer'
        'Isolated'
        'Premium'
        'Standard'
    ]
)
param APIMSKUName string

@minValue(1)
param APIMSKUCapcity int

@allowed(
    [
        'premium'
        'standard'
    ]
)
param keyVaultSKUName string

@allowed([
    'A'
])
param keyVaultSKUFamily string

@allowed([
    'standard'
])
param appConfigurationSKUName string

@allowed([
    'CapacityReservation'
    'Free'
    'LACluster'
    'PerGB2018'
    'PerNode'
    'Premium'
    'Standalone'
    'Standard'
])
param appInsightsSKUName string

@allowed([
    'Standard_LRS'
    'Standard_GRS'
    'Standard_RAGRS'
    'Standard_ZRS'
    'Premium_LRS'
    'Premium_ZRS'
])
param slackNotifierStorageAccountSKUName string

@allowed([
    'Y1'
])
param slackNotifierApplicationServicePlanSKUName string

@allowed([
    'Dynamic'

])
param slackNotifierApplicationServicePlanSKUTier string

@allowed([
    'Standard_B1ms'
])
param postgresServerSKUName string
@allowed([
    'Burstable'
    'GeneralPurpose'
    'MemoryOptimized'
])
param postgresServerSKUTier string

module common '../../common/main.bicep' = {
    name: 'common'
    params: {
        keyVaultSKUName: keyVaultSKUName
        keyVaultSKUFamily: keyVaultSKUFamily
        keyVaultSourceKeys: keyVaultSourceKeys
        sourceKeyVaultSubscriptionId: sourceKeyVaultSubscriptionId
        sourceKeyVaultResourceGroup: sourceKeyVaultResourceGroup
        sourceKeyVaultName: sourceKeyVaultName
        environment: environment
        location: location
        apiManagementDigDirEmail: apiManagementDigDirEmail
        APIMSKUCapcity: APIMSKUCapcity
        APIMSKUName: APIMSKUName
        appConfigurationSKUName: appConfigurationSKUName
        appInsightsSKUName: appInsightsSKUName
        dialogportenPgAdminPassword: dialogportenPgAdminPassword
        gitSha: gitSha
        postgresServerSKUName: postgresServerSKUName
        postgresServerSKUTier: postgresServerSKUTier
        slackNotifierApplicationServicePlanSKUName: slackNotifierApplicationServicePlanSKUName
        slackNotifierApplicationServicePlanSKUTier: slackNotifierApplicationServicePlanSKUTier
        slackNotifierStorageAccountSKUName: slackNotifierStorageAccountSKUName
    }
}

output migrationJobName string = common.outputs.migrationJobName
output resourceGroupName string = common.outputs.resourceGroupName
