import { serviceownerSearch } from '../../performancetest_common/simpleSearch.js'
import { serviceOwners } from '../../performancetest_common/readTestdata.js';
import { validateTestData } from '../../performancetest_common/readTestdata.js';
export { setup as setup } from '../../performancetest_common/readTestdata.js';

const tag_name = 'serviceowner search';
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: {
            "http_req_duration{scenario:default}": [],
            "http_req_duration{name:serviceowner search}": [],
            "http_req_duration{name:get dialog}": [],
            "http_req_duration{name:get dialog activities}": [],
            "http_req_duration{name:get dialog activity}": [],
            "http_req_duration{name:get seenlogs}": [],
            "http_req_duration{name:get transmissions}": [],
            "http_req_duration{name:get transmission}": [],
            "http_reqs{scenario:default}": [],
            "http_reqs{name:enduser search}": [],
            "http_reqs{name:get dialog activities}": [],
            "http_reqs{name:get dialog activity}": [],
            "http_reqs{name:get seenlogs}": [],
            "http_reqs{name:get transmissions}": [],
            "http_reqs{name:get transmission}": [],
            "http_reqs{name:get dialog}": [], 
        }
};
  
export default function(data) {
    const { endUsers, index } = validateTestData(data, serviceOwners);
    serviceownerSearch(serviceOwners[0], endUsers[index], tag_name, traceCalls);
}

