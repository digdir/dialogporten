param(
	[Parameter(Mandatory)]
	[string]$secretId
)

$secret = $( `
	az keyvault secret show `
	--query value `
	--id $secretId `
	--output tsv `
)
# Mark it as a secret in github actions log
echo "::add-mask::$secret"
"value=$secret" >> $env:GITHUB_OUTPUT