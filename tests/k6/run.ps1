<#
.SYNOPSIS
This script runs a test suite with provided parameters.

.DESCRIPTION
A detailed description of the function or script. This can be used to provide more detailed help.

.PARAMETER ApiEnvironment
Either 'localdev', 'dev', or 'staging'. This parameter is required.

.PARAMETER ApiVersion
Defaults to 'v1' if not supplied.

.PARAMETER TokenGeneratorUsername
Username to Altinn Token Generator. This parameter is required.

.PARAMETER TokenGeneratorPassword
Password to Altinn Token Generator. This parameter is required.

.PARAMETER FilePath
Path to the test suite file. This parameter is required.

.EXAMPLE
PS> .\run.ps1 -TokenGeneratorUsername "supersecret" -TokenGeneratorPassword "supersecret" -ApiEnvironment "dev" -FilePath "all.js"
#>

param (
    [Parameter(Mandatory=$true)]
    [ValidateSet('localdev','dev','staging')]
    [string]$ApiEnvironment,

    [string]$ApiVersion = 'v1',

    [Parameter(Mandatory=$true)]
    [string]$TokenGeneratorUsername,

    [Parameter(Mandatory=$true)]
    [String]$TokenGeneratorPassword,
    #[SecureString]$TokenGeneratorPassword,

    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

if (-not (Test-Path $FilePath)) {
    Write-Error "Error: File '$FilePath' does not exist."
    exit 1
}

$env:API_ENVIRONMENT = $ApiEnvironment
$env:API_VERSION = $ApiVersion
$env:TOKEN_GENERATOR_USERNAME = $TokenGeneratorUsername
$env:TOKEN_GENERATOR_PASSWORD = $TokenGeneratorPassword

k6 run $FilePath
