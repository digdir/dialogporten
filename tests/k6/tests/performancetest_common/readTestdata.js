/**
 * This file contains the implementation of reading test data from CSV files.
 * The test data includes service owners, end users, and end users with tokens.
 * The data is read using the PapaParse library and stored in SharedArray variables.
 * 
 * @module readTestdata
 */

import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { SharedArray } from "k6/data";

/**
 * Function to read the CSV file specified by the filename parameter.
 * @param {} filename 
 * @returns 
 */
function readCsv(filename) {
  try {
    return papaparse.parse(open(filename), { header: true, skipEmptyLines: true }).data;
  } catch (error) {
    console.log(`Error reading CSV file: ${error}`);
    return [];
  } 
}

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
  return readCsv(filenameServiceowners);
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
  return readCsv(filenameEndusers); 
});

/**
 * SharedArray variable that stores the end users with tokens data.
 * The data is parsed from the CSV file specified by the filenameEndusersWithTokens variable.
 * 
 * @name endUsersWithTokens
 * @type {SharedArray}
 */
export const endUsersWithTokens = new SharedArray('endUsersWithTokens', function () {
  return readCsv(filenameEndusersWithTokens);
});

