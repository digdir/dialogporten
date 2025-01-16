/**
 * Performance test for creating dialogs with transmissions
 * Run: k6 run tests/k6/tests/serviceowner/performance/create-transmissions.js --vus 1 --iterations 1 -e numberOfTransmissions=100
 */
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { createTransmissions } from '../../performancetest_common/createDialog.js';
import { serviceOwners, endUsers } from '../../performancetest_common/readTestdata.js';

export const options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['create dialog', 'create transmission'])
};

const isSingleUserMode = (__ENV.isSingleUserMode ?? 'false') === 'true';
const traceCalls = (__ENV.traceCalls ?? 'false') === 'true';
const numberOfTransmissions = (__ENV.numberOfTransmissions ?? '10');
const maxTransmissionsInThread = (__ENV.maxTransmissionsInThread ?? '100');
const testid = (__ENV.TESTID ?? 'createTransmissions');

export default function() {
    if (!endUsers || endUsers.length === 0) {
        throw new Error('No end users loaded for testing');
    }
    if (!serviceOwners || serviceOwners.length === 0) {
        throw new Error('No service owners loaded for testing');
    } 
    if (isSingleUserMode) {
      createTransmissions(serviceOwners[0], endUsers[0], traceCalls, numberOfTransmissions, maxTransmissionsInThread, testid);
    }
    else {
        let serviceOwner = randomItem(serviceOwners);
        for (const endUser of endUsers) {
            createTransmissions(serviceOwner, endUser, traceCalls, numberOfTransmissions, maxTransmissionsInThread, testid);
        }  
    }
  }

