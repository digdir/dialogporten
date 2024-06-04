import { describe, expect, expectStatusFor, getSO, postSO, putSO, patchSO, deleteSO, purgeSO } from '../../common/testimports.js'
import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {

    let validSo = null;
    let invalidSo = { orgName: "other", orgNo: "310778737" };

    let dialog = dialogToInsert();
    let dialogElementId = dialog.elements[0].id;
    let dialogElement = dialog.elements[2]; // Use the one without a predefined ID or related ID
    let dialogId = null; // known after successful insert
    let activity = dialog.activities[0];
    let activityId = null; // known after successful insert
   
    let expectEither = function (statusCodeSuccess, statusCodeFailure, r, shouldSucceed) {
        expectStatusFor(r).to.equal(shouldSucceed ? statusCodeSuccess : statusCodeFailure);
    }

    let permutations = [
        [ true, validSo ],
        [ false, invalidSo ]
    ];

    permutations.forEach(([shouldSucceed, tokenOptions]) => {
        let logPrefix = shouldSucceed ? "Allow" : "Deny";
        let logSuffix = shouldSucceed ? "valid serviceowner" : "invalid serviceowner"

        describe(`${logPrefix} dialog create as ${logSuffix}`, () => {
            let r = postSO('dialogs', dialog, null, tokenOptions);
            expectEither(201, 403, r, shouldSucceed);
            if (r.status == 201) {
                dialogId = r.json();
            }
        });
    
        describe(`${logPrefix} getting dialog as ${logSuffix}`, () => {
            let r = getSO('dialogs/' + dialogId, null, tokenOptions);
            expectEither(200, 404, r, shouldSucceed);
            if (r.status == 200) {
                activityId = r.json().activities[0].id;
            }
        });

        describe(`${logPrefix} patching dialog as ${logSuffix}`, () => {
            let patchDocument = [
                {
                    "op": "replace",
                    "path": "/apiActions/0/endpoints/1/url",
                    "value": "https://vg.no"
                }
            ];
            let r = patchSO('dialogs/' + dialogId, patchDocument, null, tokenOptions);
            expectEither(204, 404, r, shouldSucceed);
        });
    
        describe(`${logPrefix} updating dialog as ${logSuffix}`, () => {
            let r = putSO('dialogs/' + dialogId, dialog, null, tokenOptions);
            expectEither(204, 404, r, shouldSucceed);
        });
    
        
        describe(`${logPrefix} deleting dialog as ${logSuffix}`, () => {
            // Only check that we cannot delete is as invalid SO so
            // that the dialog still exists for subsequent tests
            if (shouldSucceed) return;
            let r = deleteSO('dialogs/' + dialogId, null, tokenOptions);
            expectEither(204, 404, r, shouldSucceed);
        });
        
    
        describe(`${logPrefix} getting dialog element list as ${logSuffix}`, () => {
            let r = getSO('dialogs/' + dialogId + "/elements/", null, tokenOptions);
            expectEither(200, 404, r, shouldSucceed);
        });
    
        describe(`${logPrefix} getting dialog element as ${logSuffix}`, () => {
            let r = getSO('dialogs/' + dialogId + "/elements/" + dialogElementId, null, tokenOptions);
            expectEither(200, 404, r, shouldSucceed);
        });
    
        describe(`${logPrefix} posting dialog element as ${logSuffix}`, () => {            
            let r = postSO('dialogs/' + dialogId + "/elements", dialogElement, null, tokenOptions);
            expectEither(201, 404, r, shouldSucceed); 
        });
    
        describe(`${logPrefix} putting dialog element as ${logSuffix}`, () => {
            let r = putSO('dialogs/' + dialogId + "/elements/" + dialogElementId, dialogElement, null, tokenOptions);
            expectEither(204, 404, r, shouldSucceed);
        });
    
        describe(`${logPrefix} deleting dialog element as ${logSuffix}`, () => {
            // Only check that we cannot delete is as invalid SO so
            // that the dialog still exists for subsequent tests
            if (shouldSucceed) return;
            let r = deleteSO('dialogs/' + dialogId + "/elements/" + dialogElementId, null, tokenOptions);
            expectEither(200, 404, r, shouldSucceed);
        });

        describe(`${logPrefix} getting activities as ${logSuffix}`, () => {
            let r = getSO('dialogs/' + dialogId + "/activities", null, tokenOptions);
            expectEither(200, 404, r, shouldSucceed);
        });

        describe(`${logPrefix} getting activity entry as ${logSuffix}`, () => {
            let r = getSO('dialogs/' + dialogId + "/activities/" + activityId, null, tokenOptions);
            expectEither(200, 404, r, shouldSucceed);
        });

        describe(`${logPrefix} posting activity as ${logSuffix}`, () => {
            let r = postSO('dialogs/' + dialogId + "/activities", activity, null, tokenOptions);
            expectEither(201, 404, r, shouldSucceed);
        });
    
    });

    // Finally, cleanup by deleting the dialog
    describe('Allow purging dialog as valid serviceowner', () => {
        let r = purgeSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(204);
    });

    // Check that searching cannot be done without search scope
    describe('Deny searching dialogs without search-scope', () => {
        let r = getSO('dialogs', null, { "scopes": "digdir:dialogporten.serviceprovider" } );
        expect(r.status, 'response status').to.equal(403);
    });

}
