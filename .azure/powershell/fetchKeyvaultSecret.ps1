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
# Mark it as a secret in github actions log
echo "::add-mask::$connectionString"
"value='$connectionString'" >> $env:GITHUB_OUTPUT