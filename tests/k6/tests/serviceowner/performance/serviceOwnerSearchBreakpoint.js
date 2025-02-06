import { serviceownerSearch } from '../../performancetest_common/simpleSearch.js'
import { serviceOwners, endUsersPart, validateTestData } from '../../performancetest_common/readTestdata.js';

const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';
const stages_duration = (__ENV.stages_duration ?? '1m');
const stages_target = (__ENV.stages_target ?? '5');
const abort_on_fail = (__ENV.abort_on_fail ?? 'true') === 'true';
const tag_name = 'serviceowner search';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: {
        "http_req_duration{name:serviceowner search}": [{ threshold: "max<5000", abortOnFail: abort_on_fail }],
        "http_req_duration{name:get dialog activities}": [{ threshold: "max<5000", abortOnFail: abort_on_fail }],
        "http_req_duration{name:get dialog activity}": [{ threshold: "max<5000", abortOnFail: abort_on_fail }],
        "http_req_duration{name:get seenlogs}": [{ threshold: "max<5000", abortOnFail: abort_on_fail }],
        "http_req_duration{name:get transmissions}": [{ threshold: "max<5000", abortOnFail: abort_on_fail }],
        "http_req_duration{name:get transmission}": [{ threshold: "max<5000", abortOnFail: abort_on_fail }],
        "http_reqs{name:get dialog activities}": [],
        "http_reqs{name:get dialog activity}": [],
        "http_reqs{name:get seenlogs}": [],
        "http_reqs{name:get transmissions}": [],
        "http_reqs{name:get transmission}": [],
        "http_reqs{name:serviceowner search}": [],   
    },
    executor: 'ramping-arrival-rate', //Assure load increase if the system slows
    stages: [
        { duration: stages_duration, target: stages_target }, // simulate ramp-up of traffic from 1 to stages_target users over stages_duration
    ],
}

export function setup() {
    const totalVus = stages_target;
    let parts = [];
    for (let i = 1; i <= totalVus; i++) {
        parts.push(endUsersPart(totalVus, i));
    }
    return parts;
}

export default function(data) {
    const { endUsers, index } = validateTestData(data, serviceOwners);
    serviceownerSearch(serviceOwners[0], endUsers[index], tag_name, traceCalls);
}
