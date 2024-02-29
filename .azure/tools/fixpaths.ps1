## This script removes /api/vX from the endpoint paths, to avoid duplication of these in the common APIM instance.

param(
    [string]$appName = "dialogporten",
    [string]$fileName = "swagger.json"
)

function Get-APIVersion ($fileName){

    # Read the JSON file
    $jsonContent = Get-Content -Path $fileName -Raw

    # Define regular expression pattern to match "/api/**/"
    $pattern = '/api/v(\d+)/'

    # Search for the pattern in the JSON content
    $matches = [regex]::Matches($jsonContent.ToString(), $pattern)

    # If matches found, extract the version
    if ($matches.Count -gt 0) {
        # Extract version from the first match
        $v = "v" + $matches[0].Groups[1].Value
        Write-Host "API Version found: $v"
        return $v
    } else {
        Write-Host "No API version found in the JSON file."
        return $null
    }
}

function Update-OpenApiSpecification ($appName, $version) {
    if(!(Test-Path -Path $fileName -PathType Leaf)){
        $fileName = $appName + ".json"
    }

    Write-Host ("Getting API version.")
    $version = Get-APIVersion -fileName $fileName

    if ($version -ne $null) {
        $outFileName = $appName + "-" + $version + ".json"
        $apiSuffix = "/api/$version"

        Write-Host ("Removing cases of '" + $apiSuffix + "' from the endpoint paths.")

        Copy-Item -Path $fileName -Destination $outFileName
        (Get-Content $outFileName).replace($apiSuffix, '') | Set-Content $outFileName
    }
}

Update-OpenApiSpecification -appName $appName -version $version
