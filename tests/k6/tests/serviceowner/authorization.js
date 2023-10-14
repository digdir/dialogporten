import { describe, expect, getSO, postSO, putSO, patchSO, deleteSO, uuidv4 } from '../../common/testimports.js'
import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {
    let tokenOptionsForDifferentServiceOwner = {
        orgName: "other",
        orgNo: "310778737"
    };

    let dialogId = null;

    describe('Perform dialog create with correct SO', () => {
        let r = postSO('dialogs', JSON.stringify(dialogToInsert()));
        expect(r.status, 'response status').to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        
        dialogId = r.json();
    });

    describe('Perform dialog create with wrong SO', () => {
        let r = postSO('dialogs', JSON.stringify(dialogToInsert()), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to get dialog as correct SO', () => {
        let r = getSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(200);
    });

    describe('Attempt to get dialog as wrong SO', () => {
        let r = getSO('dialogs/' + dialogId, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to patch dialog as wrong SO', () => {
        let patchDocument = [
            {
                "op": "replace",
                "path": "/apiActions/0/endpoints/1/url",
                "value": "http://vg.no"
            }
        ];
        let r = patchSO('dialogs/' + dialogId, JSON.stringify(patchDocument), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to put dialog as wrong SO', () => {
        let r = putSO('dialogs/' + dialogId, JSON.stringify(dialogToInsert()), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to delete dialog as wrong SO', () => {
        let r = deleteSO('dialogs/' + dialogId, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to get dialog elements as wrong SO', () => {
        let r = getSO('dialogs/' + dialogId + "/elements/" + uuidv4(), null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to post dialog element as wrong SO', () => {
        let r = postSO('dialogs/' + dialogId + "/elements", {}, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to put dialog element as wrong SO', () => {
        let r = putSO('dialogs/' + dialogId + "/elements" + uuidv4(), {}, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    describe('Attempt to post activity history as wrong SO', () => {
        let r = postSO('dialogs/' + dialogId + "/activities", {}, null, tokenOptionsForDifferentServiceOwner);
        expect(r.status, 'response status').to.equal(403);
    });

    // Cleanup
    describe('Attempt to delete dialog as correct SO', () => {
        let r = deleteSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(204);
    });

}