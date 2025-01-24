/** 
 * Performance test for creating and removing a dialog
 * Run: k6 run tests/k6/tests/serviceowner/performance/create-remove-dialog.js --vus 1 --iterations 1
 */
import exec from 'k6/execution';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { serviceOwners, endUsersPart } from "../../performancetest_common/readTestdata.js";
import { createAndRemoveDialog } from '../../performancetest_common/createDialog.js';

const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],[
      'create dialog',
      'remove dialog'
    ])
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
  createAndRemoveDialog(serviceOwners[0], myEndUsers[ix], traceCalls);
}