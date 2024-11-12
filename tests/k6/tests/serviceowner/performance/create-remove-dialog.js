/** 
 * Performance test for creating and removing a dialog
 * Run: k6 run tests/k6/tests/serviceowner/performance/create-remove-dialog.js --vus 1 --iterations 1
 */
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { serviceOwners, endUsers } from "../../performancetest_common/readTestdata.js";
import { createAndRemoveDialog } from '../../performancetest_common/createDialog.js';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],[
      'create dialog',
      'remove dialog'
    ])
};

export default function() {
    if (!endUsers || endUsers.length === 0) {
        throw new Error('No end users loaded for testing');
    } 
    if (!serviceOwners || serviceOwners.length === 0) {
        throw new Error('No service owners loaded for testing');
    }

    const isSingleUserMode = (options.vus ?? 1) === 1 && (options.iterations ?? 1) === 1 && (options.duration ?? 0) === 0;
    if (isSingleUserMode) {
      createAndRemoveDialog(serviceOwners[0], endUsers[0]);
    }
    else {
        createAndRemoveDialog(randomItem(serviceOwners), randomItem(endUsers));
    }
  }
