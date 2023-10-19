import { describe, expect, getSO, postSO, deleteSO, patchSO, uuidv4 } from '../../common/testimports.js'
import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {

    let dialogId = null;
    let eTag = null;
    let newApiActionEndpointUrl = null;

    describe('Perform dialog create', () => {
        let r = postSO('dialogs', JSON.stringify(dialogToInsert()));
        expect(r.status, 'response status').to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        
        dialogId = r.json();
    });

    describe('Perform dialog get', () => {
        let r = getSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'dialog').to.have.property("id").to.equal(dialogId);
        expect(r.json(), 'dialog').to.have.property("createdAt");        
        expect(r.json().createdAt, 'createdAt').to.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z$/);
        expect(r.json(), 'dialog').to.have.property("updatedAt");
        expect(r.json().updatedAt, 'updatedAt').to.equal(r.json().createdAt);
        expect(r.json(), 'dialog').to.have.property('eTag');
        
        eTag = r.json()["eTag"];        
    });

    describe('Perform patch', () => {
        newApiActionEndpointUrl = "https://digdir.no/" + uuidv4();
        let patchDocument = [
            {
                "op": "replace",
                "path": "/apiActions/0/endpoints/1/url",
                "value": newApiActionEndpointUrl
            }
        ];
        let r = patchSO('dialogs/' + dialogId, JSON.stringify(patchDocument));
        expect(r.status, 'response status').to.equal(204);
    });

    describe('Perform dialog get after patch', () => {
        let r = getSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'dialog').to.have.property("updatedAt");        
        expect(r.json().updatedAt, 'updatedAt').to.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z$/);
        expect(r.json(), 'dialog').to.have.property("createdAt");
        expect(r.json().updatedAt, 'updatedAt').to.not.equal(r.json().createdAt);
        expect(r.json(), 'dialog').to.have.property('eTag');
        expect(r.json().eTag, 'updated eTag').to.not.equal(eTag);  
        expect(r.json(), 'dialog').to.have.nested.property("apiActions[0].endpoints[1].url").to.equal(newApiActionEndpointUrl);
    });

    describe('Perform dialog delete', () => {
        let r = deleteSO('dialogs/' + dialogId);
        expect(r.status, 'response status').to.equal(204);
    });
}