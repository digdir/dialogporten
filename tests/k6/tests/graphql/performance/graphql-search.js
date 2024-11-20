/**
 * The performance test for GraphQL search.
 * Run: k6 run tests/k6/tests/graphql/performance/graphql-search.js --vus 1 --iterations 1 -e env=yt01
 */

import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { endUsersWithTokens as endUsers } from '../../performancetest_common/readTestdata.js';
import { graphqlSearch } from "../../performancetest_common/simpleSearch.js";

const isSingleUserMode = (__ENV.isSingleUserMode ?? 'false') === 'true';    
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';


/**
 * The options object for configuring the performance test for GraphQL search.
 *
 * @property {string[]} summaryTrendStats - The summary trend statistics to include in the test results.
 * @property {object} thresholds - The thresholds for the test metrics.
 */
export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['graphql search'])
};

/**
 * The default function for the performance test for GraphQL search.
 */
export default function() {
    if (!endUsers || endUsers.length === 0) {
        throw new Error('No end users loaded for testing');
    }
    if (isSingleUserMode) {
        graphqlSearch(endUsers[0], traceCalls);
    }
    else {
        for (let i = 0; i < endUsers.length; i++) {
            graphqlSearch(endUsers[i], traceCalls);
        }
    }
}


