import { describe, expect, getSO, postSO, putSO, patchSO, deleteSO, uuidv4 } from '../../common/testimports.js'
import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {
    let tokenOptionsForDifferentServiceOwner = {
        orgName: "other",
        orgNo: "310778737"
    };

    let dialogId = null;
    let dialogElementId = null;

    describe('Allow dialog create with correct SO', () => {
        var dialog = dialogToInsert();
        dialogElementId = dialog.elements[0].id;
        let r = postSO('dialogs', JSON.stringify(dialog));
        expect(r.status, 'response status').to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        
        dialogId = r.json();
    });

    describe('Deny dialog create with wrong SO', () => {
        let r = postSO('dialogs', JSON.stringify(dialogToInsert()), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Allow getting dialog as correct SO', () => {
        let r = getSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(200);
    });

    describe('Deny getting dialog as wrong SO', () => {
        let r = getSO('dialogs/' + dialogId, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny patching dialog as wrong SO', () => {
        let patchDocument = [
            {
                "op": "replace",
                "path": "/apiActions/0/endpoints/1/url",
                "value": "http://vg.no"
            }
        ];
        let r = patchSO('dialogs/' + dialogId, JSON.stringify(patchDocument), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny updating dialog as wrong SO', () => {
        let r = putSO('dialogs/' + dialogId, JSON.stringify(dialogToInsert()), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny deleting dialog as wrong SO', () => {
        let r = deleteSO('dialogs/' + dialogId, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny getting dialog element list as wrong SO', () => {
        let r = getSO('dialogs/' + dialogId + "/elements/", null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny getting dialog element as wrong SO', () => {
        let r = getSO('dialogs/' + dialogId + "/elements/" + dialogElementId, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny posting dialog element as wrong SO', () => {
        let r = postSO('dialogs/' + dialogId + "/elements", "{}", null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny putting dialog element as wrong SO', () => {
        let r = putSO('dialogs/' + dialogId + "/elements" + dialogElementId, "{}", null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny deleting dialog element as wrong SO', () => {
        let r = deleteSO('dialogs/' + dialogId + "/elements" + dialogElementId, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    describe('Deny posting activity history as wrong SO', () => {
        let r = postSO('dialogs/' + dialogId + "/activities", "{}", null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(404);
    });

    // Cleanup
    describe('Allow deleting dialog as correct SO', () => {
        let r = deleteSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(204);
    });

}