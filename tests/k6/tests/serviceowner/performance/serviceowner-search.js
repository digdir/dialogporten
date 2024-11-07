import { serviceownerSearch } from '../../performancetest_common/simpleSearch.js'
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { serviceOwners ,endUsersWithTokens } from '../../performancetest_common/readTestdata.js';

const tag_name = 'serviceowner search';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],[tag_name])
};

export default function() {
    if (!endUsersWithTokens || endUsersWithTokens.length === 0) {
        throw new Error('No end users loaded for testing');
    } 
    if (!serviceOwners || serviceOwners.length === 0) {
        throw new Error('No service owners loaded for testing');
    }
      
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
        serviceownerSearch(serviceOwners[0], endUsersWithTokens[0], tag_name);
    }
    else {
        serviceownerSearch(randomItem(serviceOwners) ,randomItem(endUsersWithTokens), tag_name);
    }
}

