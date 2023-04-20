param(
	[Parameter(Mandatory)]
	[string]$secretId
)

$connectionString = $( `
	az keyvault secret show `
	--query value `
	--id $secretId `
	| % {$_ -replace '\\', ''} `
	| % {$_ -replace '"', ''} `
)
"value='$connectionString'" >> $env:GITHUB_OUTPUT