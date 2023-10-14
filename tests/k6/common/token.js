import http from "k6/http";
import encoding from "k6/encoding";

// TODO! This should be overridable in tests
const scopes = "digdir:dialogporten digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search";
const orgName = "ttd";
const orgNo = "991825827";

const tokenUsername = __ENV.TOKEN_GENERATOR_USERNAME;
const tokenPassword = __ENV.TOKEN_GENERATOR_PASSWORD;

const tokenTtl = 3600;
const tokenMargin = 10;  // token expires 10 seconds before its TTL

const credentials = `${tokenUsername}:${tokenPassword}`;
const encodedCredentials = encoding.b64encode(credentials);
const tokenRequestOptions = {
  headers: {
    Authorization: `Basic ${encodedCredentials}`,
  },
};

let cachedServiceOwnerToken = null;
let cachedServiceOwnerTokenIssuedAt = null;

export function getServiceOwnerTokenFromGenerator() {
  const currentTime = Math.floor(Date.now() / 1000); 

  if (cachedServiceOwnerToken == null || (currentTime - cachedServiceOwnerTokenIssuedAt >= tokenTtl - tokenMargin)) {

    console.warn("Fetching token from token generator for VU #" + __VU);

    let response = http.get("http://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?env=tt02&scopes=" + encodeURIComponent(scopes) + "&org=" + orgName + "&orgNo=" + orgNo + "&ttl=" + tokenTtl, tokenRequestOptions);
    cachedServiceOwnerToken = response.body;

    if (response.status != 200) {
      throw new Error(`Failed getting service owner token: ${response.status_text}`);
    }

    cachedServiceOwnerTokenIssuedAt = currentTime;
  }

  return cachedServiceOwnerToken;
}

export function getEnduserTokenFromGenerator() {
  throw new Error("Not yet implemented")
}