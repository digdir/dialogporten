import { enduserSearch } from '../../performancetest_common/simpleSearch.js'
import { getEndUserTokens } from '../../../common/token.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.1.0/index.js';

const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';
const numberOfEndUsers = 2799;

export let options = {
    setupTimeout: '10m',
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: {
        "http_req_duration{scenario:default}": [],
        "http_req_duration{name:enduser search}": [],
        "http_req_duration{name:get dialog}": [],
        "http_req_duration{name:get dialog activities}": [],
        "http_req_duration{name:get dialog activity}": [],
        "http_req_duration{name:get seenlogs}": [],
        "http_req_duration{name:get transmissions}": [],
        "http_req_duration{name:get transmission}": [],
        "http_req_duration{name:get labellog}": [],
        "http_reqs{scenario:default}": [],
        "http_reqs{name:enduser search}": [],
        "http_reqs{name:get dialog activities}": [],
        "http_reqs{name:get dialog activity}": [],
        "http_reqs{name:get seenlogs}": [],
        "http_reqs{name:get transmissions}": [],
        "http_reqs{name:get transmission}": [],
        "http_reqs{name:get dialog}": [], 
        "http_reqs{name:get labellog}": [], 
    }
};

export function setup() {
    const tokenOptions = {
        scopes: "digdir:dialogporten"
    }
    const endusers = getEndUserTokens(numberOfEndUsers, tokenOptions);
    return endusers
}

export default function(data) {
    const endUser = randomItem(Object.keys(data));
    const token = data[endUser];
    enduserSearch(endUser, token, traceCalls);  
}

