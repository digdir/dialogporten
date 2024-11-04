import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

import { createDialog, options as dialogOptions } from './create-dialog.js';
import { simpleSearch, options as searchOptions } from '../../enduser/performance/simple-search.js';
 
const filenameServiceowners = '../../performancetest_data/.serviceowners-with-tokens.csv';
const serviceOwners = new SharedArray('serviceOwners', function () {
  return papaparse.parse(open(filenameServiceowners), { header: true, skipEmptyLines: true }).data;
});

const filenameEndusersWithTokens = '../../performancetest_data/.endusers-with-tokens.csv';
const endUsersWithTokens = new SharedArray('endUsersWithTokens', function () {
    return papaparse.parse(open(filenameEndusersWithTokens), { header: true, skipEmptyLines: true }).data;
  });

function joinThresholds(t1, t2) {
  for (var k in t2) {
    t1[k] = t2[k];
  } 
  return t1;
}

export const options = {
    scenarios: {
      create_dialogs: {
        executor: 'constant-vus',
        tags: { name: 'create dialog'},
        exec: 'createDialogs',
        vus: __ENV.svus,
        duration: __ENV.duration
      },
      simple_search: {
        executor: 'constant-vus',
        tags: { name: 'simple search'},
        exec: 'simpleSearches',
        vus: __ENV.evus,
        duration: __ENV.duration,
      }
    },
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: joinThresholds(searchOptions.thresholds, dialogOptions.thresholds)
                 
};

export function createDialogs() {
    createDialog(randomItem(serviceOwners), randomItem(endUsersWithTokens));
}

export function simpleSearches() {
    simpleSearch(randomItem(endUsersWithTokens));
}