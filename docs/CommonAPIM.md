This document contains instructions on how to publish a new version of the Dialogporten API to the DigDir common API Management instances.

## 1. Get swagger.json of the API version you wish to deploy

- Start up the Dialogporten application in your IDE and go to your local swagger (default is https://localhost:7214/swagger/)
- Select version from the dropdown in the upper right corner
- Right click and save on the /swagger/vX/swagger.json link under the API name.
- Save the file as "swagger.json", where X is the version number.

## 2. Copy the swagger.json to the Windows VM

- Go to the Azure Portal and find the resource group "dp-be-common-rg"
- Start the VM "apim-template-gen" and RDP into it
  - username: digdiradmin
  - password: stored as a secret in the kevault in the same resource group
- Open the file explorer and navigate to the folder "C:\Scripts\FixPaths"
- Copy the "swagger.json" file to this folder

## 3. Transform the swagger.json to align with what the common API Management expects

- Open a PowerShell terminal and navigate to the folder "C:\Scripts\FixPaths"
- Run the script "fixpaths.ps1". You should get a new file called "dialogporten-vX.json" in the same folder.
- Copy the file "dialogporten-vX.json" to the folder "C:\Repos\altinn-studio-ops\provisioning\altinn_platform\scripts\apim\build\dialogporten\openApi"

## 4. Publish the new version to the common API Management

- Refer to the guide on AltinnPedia to get the full picture: https://pedia.altinn.cloud/altinn-3/ops/release-and-deploy/api-management/
- Here is a shortened explanation specific to Dialogporten
- 

