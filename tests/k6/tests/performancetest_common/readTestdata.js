import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { SharedArray } from "k6/data";

const filenameServiceowners = '../performancetest_data/.serviceowners-with-tokens.csv';
export const serviceOwners = new SharedArray('serviceOwners', function () {
  return papaparse.parse(open(filenameServiceowners), { header: true, skipEmptyLines: true }).data;
});

if (!__ENV.API_ENVIRONMENT) {
  throw new Error('API_ENVIRONMENT must be set');
}
const filenameEndusers = `../performancetest_data/endusers-${__ENV.API_ENVIRONMENT}.csv`;
export const endUsers = new SharedArray('endUsers', function () {
  return papaparse.parse(open(filenameEndusers), { header: true, skipEmptyLines: true }).data;
});

const filenameEndusersWithTokens = '../performancetest_data/.endusers-with-tokens.csv';
export const endUsersWithTokens = new SharedArray('endUsersWithTokens', function () {
  return papaparse.parse(open(filenameEndusersWithTokens), { header: true, skipEmptyLines: true }).data;
});

