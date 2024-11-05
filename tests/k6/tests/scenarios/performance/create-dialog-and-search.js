import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

import { enduserSearch } from '../../performancetest_common/simpleSearch.js';
import { createDialog } from '../../performancetest_common/createDialog.js';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { serviceOwners, endUsersWithTokens } from '../../performancetest_common/readTestdata.js';
 
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
        tags: { name: 'search dialogs'},
        exec: 'enduserSearches',
        vus: __ENV.evus,
        duration: __ENV.duration,
      }
    },
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],[
      'simple search',
      'create dialog',
      'get dialog', 
      'get dialog activities', 
      'get dialog activity', 
      'get seenlogs',
      'get seenlog',
      'get transmissions',
      'get transmission',
      'get labellog'
    ])
                 
};

export function createDialogs() {
    createDialog(randomItem(serviceOwners), randomItem(endUsersWithTokens));
}

export function enduserSearches() {
    enduserSearch(randomItem(endUsersWithTokens));
}