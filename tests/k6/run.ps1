<#
.SYNOPSIS
This script runs a test suite or individual tests with provided parameters.

.DESCRIPTION
This script invokes the 'k6' binary setting various environment variables used within
the K6 scripts required to get an authorization token, select environment and perform
the tests specified. This script also invokes script\generate_alltests.ps1 which 
generates tests\enduser\all-tests.js and tests\serviceowner\all-tests.js, which is used
to group all defined tests together.

Any additional parameters passed will be passed verbatim to K6

.PARAMETER ApiEnvironment
Either 'localdev', 'test', or 'staging'. This parameter is required.

.PARAMETER ApiVersion
Defaults to 'v1' if not supplied.

.PARAMETER TokenGeneratorUsername
Username to Altinn Token Generator. This parameter is required.

.PARAMETER TokenGeneratorPassword
Password to Altinn Token Generator. This parameter is required.

.PARAMETER FilePath
Path to the test suite file. This parameter is required.

.EXAMPLE
PS> .\run.ps1 -TokenGeneratorUsername "supersecret" -TokenGeneratorPassword "supersecret" -ApiEnvironment "test" -FilePath "suites/all-single-pass.js"
PS> .\run.ps1 -TokenGeneratorUsername "supersecret" -TokenGeneratorPassword "supersecret" -ApiEnvironment "test" -FilePath "tests/serviceowner/dialogCreate.js" --http-debug
#>

param (
    [Parameter(Mandatory=$true)]
    [ValidateSet('localdev','test','staging')]
    [string]$ApiEnvironment,

    [string]$ApiVersion = 'v1',

    [Parameter(Mandatory=$true)]
    [string]$TokenGeneratorUsername,

    [Parameter(Mandatory=$true)]
    [String]$TokenGeneratorPassword,
    #[SecureString]$TokenGeneratorPassword,

    [Parameter(Mandatory=$true)]
    [string]$FilePath,

    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]$K6Args
)

try {
    Get-Command k6 -ErrorAction Stop > $null
} catch {
    Write-Error "Error: k6 is not installed or not available in the system PATH. Please install it before proceeding, see https://k6.io/docs/get-started/installation/"
    exit 1
}

if (-not (Test-Path $FilePath)) {
    Write-Error "Error: File '$FilePath' does not exist."
    exit 1
}

& "$PSScriptRoot\scripts\generate_alltests.ps1" "$PSScriptRoot\tests\serviceowner\" > $null
& "$PSScriptRoot\scripts\generate_alltests.ps1" "$PSScriptRoot\tests\enduser\" > $null

$env:API_ENVIRONMENT = $ApiEnvironment
$env:API_VERSION = $ApiVersion
$env:TOKEN_GENERATOR_USERNAME = $TokenGeneratorUsername
$env:TOKEN_GENERATOR_PASSWORD = $TokenGeneratorPassword

k6 run @K6Args $FilePath 
