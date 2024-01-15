
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
