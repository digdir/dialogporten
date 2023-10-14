import http from "k6/http";
import encoding from "k6/encoding";
import { extend } from "./extend.js";

let defaultTokenOptions = {
  scopes: "digdir:dialogporten digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search",
  orgName: "ttd",
  orgNo: "991825827",
  ssn: "07874299582"
};

const tokenUsername = __ENV.TOKEN_GENERATOR_USERNAME;
const tokenPassword = __ENV.TOKEN_GENERATOR_PASSWORD;

const tokenTtl = 3600;
const tokenMargin = 10;

const credentials = `${tokenUsername}:${tokenPassword}`;
const encodedCredentials = encoding.b64encode(credentials);
const tokenRequestOptions = {
  headers: {
    Authorization: `Basic ${encodedCredentials}`,
  },
};

let cachedTokens = {};
let cachedTokensIssuedAt = {};

function getCacheKey(tokenType, tokenOptions) {
  return `${tokenType}|${tokenOptions.scopes}|${tokenOptions.orgName}|${tokenOptions.orgNo}|${tokenOptions.ssn}`;
}

function fetchToken(url, tokenOptions, type) {
  const currentTime = Math.floor(Date.now() / 1000);  
  const cacheKey = getCacheKey(type, tokenOptions);

  if (!cachedTokens[cacheKey] || (currentTime - cachedTokensIssuedAt[cacheKey] >= tokenTtl - tokenMargin)) {
    console.warn(`Fetching ${type} token from token generator for VU #${__VU}`);
    let response = http.get(url, tokenRequestOptions);

    if (response.status != 200) {
      throw new Error(`Failed getting ${type} token: ${response.status_text}`);
    }
    cachedTokens[cacheKey] = response.body;
    cachedTokensIssuedAt[cacheKey] = currentTime;
  }

  return cachedTokens[cacheKey];
}

export function getServiceOwnerTokenFromGenerator(tokenOptions = null) {
  let fullTokenOptions = extend({}, defaultTokenOptions, tokenOptions);
  const url = `http://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?env=tt02&scopes=${encodeURIComponent(fullTokenOptions.scopes)}&org=${fullTokenOptions.orgName}&orgNo=${fullTokenOptions.orgNo}&ttl=${tokenTtl}`;
  return fetchToken(url, fullTokenOptions, `service owner (orgno:${fullTokenOptions.orgNo})`);
}

export function getEnduserTokenFromGenerator(tokenOptions = null) {
  let fullTokenOptions = extend({}, defaultTokenOptions, tokenOptions);
  const url = `http://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken?env=tt02&scopes=${encodeURIComponent(fullTokenOptions.scopes)}&pid=${fullTokenOptions.ssn}&userId=123&partyId=123&ttl=${tokenTtl}`;
  return fetchToken(url, fullTokenOptions, `end user (ssn:${fullTokenOptions.ssn})`);
}
