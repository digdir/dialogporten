/**
 * This file contains the implementation of reading test data from CSV files.
 * The test data includes service owners, end users, and end users with tokens.
 * The data is read using the PapaParse library and stored in SharedArray variables.
 * 
 * @module readTestdata
 */

import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { SharedArray } from "k6/data";

const filenameServiceowners = '../performancetest_data/.serviceowners-with-tokens.csv';
if (!__ENV.API_ENVIRONMENT) {
  throw new Error('API_ENVIRONMENT must be set');
}
const filenameEndusers = `../performancetest_data/endusers-${__ENV.API_ENVIRONMENT}.csv`;
const filenameEndusersWithTokens = '../performancetest_data/.endusers-with-tokens.csv';

/**
 * SharedArray variable that stores the service owners data.
 * The data is parsed from the CSV file specified by the filenameServiceowners variable.
 * 
 * @name serviceOwners
 * @type {SharedArray}
 */
export const serviceOwners = new SharedArray('serviceOwners', function () {
  return papaparse.parse(open(filenameServiceowners), { header: true, skipEmptyLines: true }).data;
});

/**
 * SharedArray variable that stores the end users data.
 * The data is parsed from the CSV file specified by the filenameEndusers variable.
 * The filenameEndusers variable is dynamically generated based on the value of the API_ENVIRONMENT environment variable.
 * 
 * @name endUsers
 * @type {SharedArray}
 */
export const endUsers = new SharedArray('endUsers', function () {
  return papaparse.parse(open(filenameEndusers), { header: true, skipEmptyLines: true }).data;
});

/**
 * SharedArray variable that stores the end users with tokens data.
 * The data is parsed from the CSV file specified by the filenameEndusersWithTokens variable.
 * 
 * @name endUsersWithTokens
 * @type {SharedArray}
 */
export const endUsersWithTokens = new SharedArray('endUsersWithTokens', function () {
  return papaparse.parse(open(filenameEndusersWithTokens), { header: true, skipEmptyLines: true }).data;
});

