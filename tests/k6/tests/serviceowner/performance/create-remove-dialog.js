import { postSO, purgeSO, expect, describe } from "../../../common/testimports.js";
import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

import { getDefaultThresholds } from '../../performancetest_common/common.js'
import { default as dialogToInsert } from '../../performancetest_data/01-create-dialog.js';
import { serviceOwners, endUsers } from "./create-dialog.js";
export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],[
      'create dialog',
      'remove dialog'
    ])
};

export default function() {
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
      createAndRemoveDialog(serviceOwners[0], endUsers[0]);
    }
    else {
        createAndRemoveDialog(randomItem(serviceOwners), randomItem(endUsers));
    }
  }

export function createAndRemoveDialog(serviceOwner, endUser) {  
    var paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceOwner.token
        },
        tags: { name: 'create dialog' }
    }

    let dialogId = 0;
    describe('create dialog', () => {
      paramsWithToken.tags.name = 'create dialog';  
      let r = postSO('dialogs', dialogToInsert(endUser.ssn, endUser.resource), paramsWithToken);  
      expect(r.status, 'response status').to.equal(201);
      dialogId = r.json();
    });

    describe('remove dialog', () => {
      paramsWithToken.tags.name = 'remove dialog';
      if (dialogId) {
          let r = purgeSO('dialogs/' + dialogId, paramsWithToken);   
          expect(r.status, 'response status').to.equal(204);
      }
  });
    
}