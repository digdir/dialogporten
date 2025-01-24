import { randomItem } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { serviceownerSearch } from '../../performancetest_common/simpleSearch.js'
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { serviceOwners, endUsers } from '../../performancetest_common/readTestdata.js';

const tag_name = 'serviceowner search';
const isSingleUserMode = (__ENV.isSingleUserMode ?? 'false') === 'true';
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

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

export function setup() {
    const totalVus = exec.test.options.scenarios.default.vus;
    let parts = [];
    for (let i = 1; i <= totalVus; i++) {
        parts.push(endUsersPart(totalVus, i));
    }
    return parts;
  }
  
  export default function(data) {
    const myEndUsers = data[exec.vu.idInTest - 1];
    const ix = exec.vu.iterationInInstance % myEndUsers.length;
    serviceownerSearch(serviceOwner, myEndUsers[ix], tag_name, traceCalls);
  }

