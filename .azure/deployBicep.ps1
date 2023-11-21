param(
	[Parameter(Mandatory)]
	[string]$gitSha,

	[Parameter(Mandatory)]
	[string]$environment,

	[Parameter(Mandatory)]
	[string]$subscriptionId,

	[Parameter(Mandatory)]
	[string]$sourceKeyVaultSubscriptionId,
	
	[Parameter(Mandatory)]
    [string]$sourceKeyVaultName,
    	
	[Parameter(Mandatory)]
	[string]$apiManagementDigDirEmail
)
Import-module "$PSScriptRoot/powershell/jsonMerge.ps1" -Force
Import-module "$PSScriptRoot/powershell/pwdGenerator.ps1" -Force

# Merge main.parameters.json and optional main.parameters.$environment.json
$paramsJson = JsonMergeFromPath "$PSScriptRoot/main.parameters.json" "$PSScriptRoot/main.parameters.$environment.json"

# Add keyvault keys to parameters.keyVault.value.source.keys
AddMemberPath $paramsJson "parameters.keyVault.value.source.keys" @( `
	az keyvault secret list `
		--vault-name $paramsJson.parameters.keyVault.value.source.name `
		--subscription $paramsJson.parameters.keyVault.value.source.subscriptionId `
		--query "[].name" `
		--output tsv `
)

# Add secrets to parameters
AddMemberPath $paramsJson "parameters.secrets.value" @{
	dialogportenPgAdminPassword = (GeneratePassword -length 30).Password
	apiManagementDigDirEmail = $apiManagementDigDirEmail
    sourceKeyVaultSubscriptionId = $sourceKeyVaultSubscriptionId
    sourceKeyVaultName = $sourceKeyVaultName	
}

# Add gitSha to parameters
AddMemberPath $paramsJson "parameters.gitSha.value" $gitSha

# Add environment to parameters
AddMemberPath $paramsJson "parameters.environment.value" $environment

# Format parameters to be used in az deployment sub create
$formattedParamsJson = $paramsJson `
	| ConvertTo-Json -Compress -Depth 100 `
	| % {$_ -replace "`"", "\`""} `
	| % {$_ -replace "`n", ""} `
	| % {$_ -replace "\s", ""}

# Deploy
$deploymentOutputs = @( `
	az deployment sub create `
		--subscription $subscriptionId `
		--location $paramsJson.parameters.location.value `
		--name "GithubActionsDeploy-be-$environment" `
		--template-file "$($PSScriptRoot)/main.bicep" `
		--parameters $formattedParamsJson `
		--query properties.outputs `
		#--confirm-with-what-if
	| ConvertFrom-Json `
)

# Start migration job
$resourceGroup = $deploymentOutputs.resourceGroupName.value
$migrationJobName = $deploymentOutputs.migrationJobName.value

if ([string]::IsNullOrEmpty($resourceGroup)) {
    Write-Host "ResourceGroup output is missing"
	exit 1
}
if ([string]::IsNullOrEmpty($migrationJobName)) {
    Write-Host "MigrationJobName output is missing"
	exit 1
}

az containerapp job start -n $migrationJobName -g $resourceGroup