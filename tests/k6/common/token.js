import http from "k6/http";
import encoding from "k6/encoding";

// TODO! This should be overridable in tests
const scopes = "digdir:dialogporten digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search";
const orgName = "ttd";
const orgNo = "991825827";

const tokenUsername = __ENV.TOKEN_GENERATOR_USERNAME;
const tokenPassword = __ENV.TOKEN_GENERATOR_PASSWORD;

const credentials = `${tokenUsername}:${tokenPassword}`;
const encodedCredentials = encoding.b64encode(credentials);
const tokenRequestOptions = {
  headers: {
    Authorization: `Basic ${encodedCredentials}`,
  },
};

let cachedServiceOwnerToken = null;

// TODO! Handle expiration of tokens (for soak testing)
export function getServiceOwnerTokenFromGenerator() {
  if (cachedServiceOwnerToken == null) {
    let response = http.get(`http://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?env=tt02&scopes=${scopes}&org=${orgName}&orgNo=${orgNo}&ttl=3600`, tokenRequestOptions);
    cachedServiceOwnerToken = response.body;
  }

  return cachedServiceOwnerToken;
}

export function getEnduserTokenFromGenerator() {
  throw new Error("Not yet implemented")
}