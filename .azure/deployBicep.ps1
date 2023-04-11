$paramsPath = "$($PSScriptRoot)/main.parameters.json"
$paramsJson = Get-Content -Raw -Path $paramsPath | ConvertFrom-Json
$keyvaultName = $paramsJson.parameters.keyVault.value.source.name
$keyvaultSubscription = $paramsJson.parameters.keyVault.value.source.subscriptionId
$location = $paramsJson.parameters.location.value

$keyvaultKeys = @( `
	az keyvault secret list `
		--vault-name "$($keyvaultName)" `
		--subscription "$($keyvaultSubscription)" `
		--query "[].name" `
		--output tsv `
	| ConvertTo-Json -Compress `
	| % {$_ -replace "`"", "'"} `
	| % {$_ -replace "`n", ""} `
	| % {$_ -replace "\s", ""} `
)

az deployment sub create `
	--subscription $keyvaultSubscription `
	--location $location `
	--name GithubActionsDeploy `
	--template-file "$($PSScriptRoot)/main.bicep" `
	--parameters $paramsPath `
	--parameters srcKeyVaultKeys=$keyvaultKeys 