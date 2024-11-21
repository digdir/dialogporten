/**
 * Common functions for creating dialogs.
 */
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { describe } from "../../common/describe.js";
import { postSO, purgeSO } from "../../common/request.js";
import { expect } from "../../common/testimports.js";
import dialogToInsert from "../performancetest_data/01-create-dialog.js";

/**
 * Creates a dialog.
 * 
 * @param {Object} serviceOwner - The service owner object.
 * @param {Object} endUser - The end user object.
 */
export function createDialog(serviceOwner, endUser, traceCalls) {
    var traceparent = uuidv4();

    var paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceOwner.token,
            traceparent: traceparent
        },
        tags: { name: 'create dialog' }
    };
    if (traceCalls) {
        paramsWithToken.tags.traceparent = traceparent;
        paramsWithToken.tags.enduser = endUser.ssn;
    }

    describe('create dialog', () => {
        let r = postSO('dialogs', dialogToInsert(endUser.ssn, endUser.resource), paramsWithToken);
        expect(r.status, 'response status').to.equal(201);
    });

}

/**
 * Creates a dialog and removes it.
 * 
 * @param {Object} serviceOwner - The service owner object.
 * @param {Object} endUser - The end user object.
 */
export function createAndRemoveDialog(serviceOwner, endUser, traceCalls) { 
    var traceparent = uuidv4(); 
    var paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceOwner.token,
            traceparent: traceparent
        },
        tags: { name: 'create dialog' }
    }
    if (traceCalls) {
        paramsWithToken.tags.traceparent = traceparent;
        paramsWithToken.tags.enduser = endUser.ssn;
    }

    let dialogId = 0;
    describe('create dialog', () => {
      paramsWithToken.tags.name = 'create dialog';
      let r = postSO('dialogs', dialogToInsert(endUser.ssn, endUser.resource), paramsWithToken);  
      expect(r.status, 'response status').to.equal(201);
      dialogId = r.json();
    });

    describe('remove dialog', () => {
      traceparent = uuidv4();
      paramsWithToken.tags.name = 'remove dialog';
      if (dialogId) {
          let r = purgeSO('dialogs/' + dialogId, paramsWithToken);   
          expect(r.status, 'response status').to.equal(204);
      }
  });
}
