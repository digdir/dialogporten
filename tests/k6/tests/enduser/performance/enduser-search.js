import exec from 'k6/execution';
import { enduserSearch } from '../../performancetest_common/simpleSearch.js'
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
export { setup as setup } from '../../performancetest_common/readTestdata.js';

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

export default function(data) {
    const myEndUsers = data[exec.vu.idInTest - 1];
    const ix = exec.vu.iterationInInstance % myEndUsers.length;
    enduserSearch(myEndUsers[ix], traceCalls);  
}

