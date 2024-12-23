/**
 * Performance test for creating a dialog
 * Run: k6 run tests/k6/tests/serviceowner/performance/create-dialog.js --vus 1 --iterations 1
 */
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { createDialog } from '../../performancetest_common/createDialog.js';
import { serviceOwners, endUsers } from '../../performancetest_common/readTestdata.js';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['create dialog'])
};

const isSingleUserMode = (__ENV.isSingleUserMode ?? 'false') === 'true';
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';

export default function() {
    if (!endUsers || endUsers.length === 0) {
        throw new Error('No end users loaded for testing');
    }
    if (!serviceOwners || serviceOwners.length === 0) {
        throw new Error('No service owners loaded for testing');
    } 
    
    if (isSingleUserMode) {
      createDialog(serviceOwners[0], endUsers[0], traceCalls);
    }
    else {
        let serviceOwner = randomItem(serviceOwners);
        for (let i = 0; i < endUsers.length; i++) {
            createDialog(serviceOwner, endUsers[i], traceCalls);
        }   
    }
  }

