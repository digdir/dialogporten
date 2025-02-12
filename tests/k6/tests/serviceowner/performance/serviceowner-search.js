import { serviceownerSearch, emptySearchThresholds } from '../../performancetest_common/simpleSearch.js'
import { serviceOwners } from '../../performancetest_common/readTestdata.js';
import { validateTestData } from '../../performancetest_common/readTestdata.js';
export { setup as setup } from '../../performancetest_common/readTestdata.js';

const tag_name = 'serviceowner search';
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: emptySearchThresholds
};
options.thresholds[`http_req_duration{name:serviceowner search}`] = [];
options.thresholds[`http_reqs{name:serviceowner search}`] = [];
  
export default function(data) {
    const { endUsers, index } = validateTestData(data, serviceOwners);
    serviceownerSearch(serviceOwners[0], endUsers[index], tag_name, traceCalls);
}

