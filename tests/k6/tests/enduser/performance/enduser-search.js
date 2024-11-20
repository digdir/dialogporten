import { enduserSearch } from '../../performancetest_common/simpleSearch.js'
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { endUsersWithTokens } from '../../performancetest_common/readTestdata.js';

const isSingleUserMode = (__ENV.isSingleUserMode ?? 'false') === 'true';
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['enduser search', 
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

export default function() {
    if (!endUsersWithTokens || endUsersWithTokens.length === 0) {
        throw new Error('No end users loaded for testing');
    }
      
    if (isSingleUserMode) {
        enduserSearch(endUsersWithTokens[0], traceCalls);
    }
    else {
        for (let i = 0; i < endUsersWithTokens.length; i++) {
            enduserSearch(endUsersWithTokens[i], traceCalls);
        }
    }
}

