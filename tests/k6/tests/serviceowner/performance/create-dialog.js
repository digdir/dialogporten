import { postSO, expect, describe } from "../../../common/testimports.js";
import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

import { getDefaultThresholds } from '../../performancetest_common/common.js'
import { default as dialogToInsert } from '../../performancetest_data/01-create-dialog.js';

const filenameServiceowners = '../../performancetest_data/.serviceowners-with-tokens.csv';
if (!__ENV.API_ENVIRONMENT) {
   throw new Error('API_ENVIRONMENT must be set');
}
const filenameEndusers = `../../performancetest_data/endusers-${__ENV.API_ENVIRONMENT}.csv`;

export const serviceOwners = new SharedArray('serviceOwners', function () {
  return papaparse.parse(open(filenameServiceowners), { header: true, skipEmptyLines: true }).data;
});

export const endUsers = new SharedArray('endUsers', function () {
    return papaparse.parse(open(filenameEndusers), { header: true, skipEmptyLines: true }).data;
  });

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['create dialog'])
};

export default function() {
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
      createDialog(serviceOwners[0], endUsers[0]);
    }
    else {
        createDialog(randomItem(serviceOwners), randomItem(endUsers));
    }
  }

export function createDialog(serviceOwner, endUser) {  
    var paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceOwner.token,
            traceparent: uuidv4(),
        },
        tags: { name: 'create dialog' }
    }

    describe('create dialog', () => {
        let r = postSO('dialogs', dialogToInsert(endUser.ssn, endUser.resource), paramsWithToken); 
        expect(r.status, 'response status').to.equal(201);
    });
    
}