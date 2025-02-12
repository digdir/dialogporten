import { randomItem } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";
import { createDialog } from '../../performancetest_common/createDialog.js';
import { serviceOwners, endUsers } from '../../performancetest_common/readTestdata.js';
export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    vus: 1,
    duration: "30s",
    thresholds: {
        "http_req_duration{name:create dialog}": ["p(95)<200"],
        "http_reqs{name:create dialog}": []   
    }
}

const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export default function () { 
    createDialog(serviceOwners[0], randomItem(endUsers), traceCalls);
}