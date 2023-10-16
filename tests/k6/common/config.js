export const baseUrls = {
    v1: {
        enduser: {
            localdev: "https://localhost:7214/api/v1/enduser/",
            localdev_docker: "https://host.docker.internal:7214/api/v1/enduser/",
            poc: "https://dppoc-webapi.azurewebsites.net/api/v1/enduser/",
            test: "https://test.eu.api.dialogporten.no/v1/",    
            staging: "https://staging.eu.api.dialogporten.no/v1/",
            prod: "https://prod.eu.api.dialogporten.no/v1/"
        },
        serviceowner: {
            localdev: "https://localhost:7214/api/v1/serviceowner/",
            localdev_docker: "https://host.docker.internal:7214/api/v1/serviceowner/",
            poc: "https://dppoc-webapi.azurewebsites.net/api/v1/serviceowner/",
            test: "https://test.so.api.dialogporten.no/v1/",    
            staging: "https://staging.so.api.dialogporten.no/v1/",
            prod: "https://prod.eu.api.dialogporten.no/v1/"
        }    
    }    
};

if (__ENV.IS_DOCKER && __ENV.API_ENVIRONMENT == "localdev") {
    __ENV.API_ENVIRONMENT = "localdev_docker";
}

if (!baseUrls[__ENV.API_VERSION]) {
    throw new Error(`Invalid API version: ${__ENV.API_VERSION}. Please ensure it's set correctly in your environment variables.`);
}

if (!baseUrls[__ENV.API_VERSION]["enduser"][__ENV.API_ENVIRONMENT]) {
    throw new Error(`Invalid enduser API environment: ${__ENV.API_ENVIRONMENT}. Please ensure it's set correctly in your environment variables.`);
}

if (!baseUrls[__ENV.API_VERSION]["serviceowner"][__ENV.API_ENVIRONMENT]) {
    throw new Error(`Invalid enduser API environment: ${__ENV.API_ENVIRONMENT}. Please ensure it's set correctly in your environment variables.`);
}

export const baseUrlEndUser = baseUrls[__ENV.API_VERSION]["enduser"][__ENV.API_ENVIRONMENT];
export const baseUrlServiceOwner = baseUrls[__ENV.API_VERSION]["serviceowner"][__ENV.API_ENVIRONMENT];
