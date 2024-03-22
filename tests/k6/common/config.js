export const baseUrls = {
    v1: {
        enduser: {
            localdev: "https://localhost:7214/api/v1/enduser/",
            localdev_docker: "https://host.docker.internal:7214/api/v1/enduser/",
            test: "https://altinn-dev-api.azure-api.net/dialogporten/api/v1/enduser/",
            staging: "https://platform.tt02.altinn.no/dialogporten/api/v1/enduser/",
            prod: "https://platform.altinn.no/dialogporten/api/v1/enduser/"
        },
        serviceowner: {
            localdev: "https://localhost:7214/api/v1/serviceowner/",
            localdev_docker: "https://host.docker.internal:7214/api/v1/serviceowner/",
            test: "https://altinn-dev-api.azure-api.net/dialogporten/api/v1/serviceowner/",
            staging: "https://platform.tt02.altinn.no/dialogporten/api/v1/serviceowner/",
            prod: "https://platform.altinn.no/dialogporten/api/v1/serviceowner/"
        }    
    }    
};

export const defaultEndUserOrgNo = "212475912";
export const defaultEndUserSsn = "14886498226"; // has "DAGL" for 212475912
export const defaultServiceOwnerOrgNo = "991825827";

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
