/**
 * Performance test for creating a dialog
 * Run: k6 run tests/k6/tests/serviceowner/performance/create-dialog.js --vus 1 --iterations 1
 */
import exec from 'k6/execution';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { createDialog } from '../../performancetest_common/createDialog.js';
import { serviceOwners } from '../../performancetest_common/readTestdata.js';
export { setup as setup } from '../../performancetest_common/readTestdata.js';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['create dialog'])
};

const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export default function(data) {
    const myEndUsers = data[exec.vu.idInTest - 1];
    const ix = exec.vu.iterationInInstance % myEndUsers.length;
    createDialog(serviceOwners[0], myEndUsers[ix], traceCalls);
  }

