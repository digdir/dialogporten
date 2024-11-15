import { randomItem } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { serviceownerSearch } from '../../performancetest_common/simpleSearch.js'
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { serviceOwners, endUsersWithTokens } from '../../performancetest_common/readTestdata.js';

const tag_name = 'serviceowner search';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],[tag_name, 
        'get dialog', 
        'get dialog activities', 
        'get dialog activity', 
        'get seenlogs',
        'get seenlog',
        'get transmissions',
        'get transmission'])
};

/**
 * Perform a service owner search.
 * In single user mode, the first service owner and end user with token is used. Only one iteration is performed.
 * In multi user mode, a random service owner and end user with token is used.
 */
export default function() {
    if (!endUsersWithTokens || endUsersWithTokens.length === 0) {
        throw new Error('No end users loaded for testing');
    } 
    if (!serviceOwners || serviceOwners.length === 0) {
        throw new Error('No service owners loaded for testing');
    }

    const isSingleUserMode = (options.vus ?? 1) === 1 && (options.iterations ?? 1) === 1 && (options.duration ?? 0) === 0;
    if (isSingleUserMode) {
        serviceownerSearch(serviceOwners[0], endUsersWithTokens[0], tag_name);
    }
    else {
        serviceownerSearch(randomItem(serviceOwners), randomItem(endUsersWithTokens), tag_name);
    }
}

