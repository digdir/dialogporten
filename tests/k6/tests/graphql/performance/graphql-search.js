import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { endUsersWithTokens as endUsers } from '../../performancetest_common/readTestdata.js';
import { graphqlSearch } from "../../performancetest_common/simpleSearch.js";

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['graphql search'])
};

export default function() {
    if (!endUsers || endUsers.length === 0) {
        throw new Error('No end users loaded for testing');
    }
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
        graphqlSearch(endUsers[0]);
    }
    else {
        graphqlSearch(randomItem(endUsers));
    }
}


