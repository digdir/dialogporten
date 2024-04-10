This document contains instructions on how to publish a new version of the Dialogporten API to the DigDir common API Management instances.

## 1. Get swagger.json of the API version you wish to deploy

- Start up the Dialogporten application in your IDE and go to your local swagger (default is https://localhost:7214/swagger/)
- Select version from the dropdown in the upper right corner
- Right click and save on the `/swagger/vX/swagger.json` link under the API name.
- Save the file as `swagger.json`.

## 2. Copy the swagger.json to the Windows VM

- Go to the Azure Portal and find the resource group `dp-be-common-rg`
- Start the VM `apim-template-gen` and RDP into it (The VM shuts down automatically every night at 00.00)
  - username: digdiradmin
  - password: stored as a secret in the key vault in the same resource group
- Open the file explorer and navigate to the folder `C:\Scripts\FixPaths`
- Copy the `swagger.json` file to this folder

## 3. Transform the swagger.json to align with what the common API Management expects

- Open a PowerShell terminal and navigate to the folder `C:\Scripts\FixPaths`
- Run the script `fixpaths.ps1`. You should get a new file called `dialogporten-vX.json` in the same folder, where X is the version number.
- Copy the file "dialogporten-vX.json" to the folder `C:\Repos\altinn-studio-ops\provisioning\altinn_platform\scripts\apim\build\dialogporten\openApi`

## 4. Publish the new version to the common API Management

Refer to the guide on AltinnPedia to get the full picture: https://pedia.altinn.cloud/altinn-3/ops/release-and-deploy/api-management/

Here is a shortened explanation specific to Dialogporten:
  - Open directory `C:\Repos\altinn-studio-ops\provisioning\altinn_platform\scripts\apim\build` (the following steps will use this directory as root)


  - If you are deploying a new version of the API, you need to update the version number in the following files
    - `dialogporten-creator-config.yml`
    - `policy-api-vX.xml` (create a new file for the new version and update the contents)


  - Run the following in powershell from the root directory: `./create_templates.ps1 -appName dialogporten`.  
This will create ARM templates for the new version of the API in the folder `.\dialogporten\created_templates`


  - Commit and push the changes to the `altinn-studio-ops` repository (create a branch and PR if you want to be safe, or just push to master if you are confident)


  - Go to the Azure DevOps pipeline [apim-deploy-creator-templates](https://dev.azure.com/brreg/altinn-studio-ops/_build?definitionId=121) and trigger the pipeline. Select `dialogporten` from the Application dropdown.   
This will deploy the API to the `altinn-dev-api` APIM instance


  - Check that you are logged into the Azure CLI


  - If you are happy with the API deployed to dev, you can create templates for deploy to all the other environments by doing the following:
    - Run the following in powershell from the root directory: `./extract_templates.ps1 -appName dialogporten`  
This will create ARM templates for all the environments in the folder `.\dialogporten\extracted_templates` and `.\dialogporten\templates`
  

  - Commit and push the changes to the `altinn-studio-ops` repository (create a branch and PR if you want to be safe, or just push to master if you are confident)


  - You are then able to deploy the API to the other APIM-environments by triggering pipelines in the Azure DevOps project. The pipelines have the form a`pim-deploy-api.XXXX` where `XXXX` is the environment name.  
    (For our `staging` environment, use `tt02`)
