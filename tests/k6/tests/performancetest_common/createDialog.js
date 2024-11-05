import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { describe } from "../../common/describe.js";
import { postSO, purgeSO } from "../../common/request.js";
import { expect } from "../../common/testimports.js";
import dialogToInsert from "../performancetest_data/01-create-dialog.js";

/**
 * Creates a dialog.
 *
 * @param {Object} serviceOwner - The service owner.
 * @param {Object} endUser - The end user.
 */

export function createDialog(serviceOwner, endUser) {
    var paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceOwner.token,
            traceparent: uuidv4()
        },
        tags: { name: 'create dialog' }
    };

    describe('create dialog', () => {
        let r = postSO('dialogs', dialogToInsert(endUser.ssn, endUser.resource), paramsWithToken);
        expect(r.status, 'response status').to.equal(201);
    });

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
