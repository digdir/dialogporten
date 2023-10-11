import { describe, expect, getSO, postSO, deleteSO, patchSO, uuidv4 } from '../../common/testimports.js'
import { dialogToInsert } from './testdata/01-create-dialog.js';

let state = {};

function get(key) {
    return state[__VU] ? state[__VU][key] : null;
}

function set(key, val) {
    if (!state[__VU]) state[__VU] = {};
    state[__VU][key] = val;
}


export default function () {

    describe('Perform dialog create', () => {
        let r = postSO('dialogs', JSON.stringify(dialogToInsert));
        expect(r.status, 'response status').to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        set("dialogId", r.json());
    });

    describe('Perform dialog get', () => {
        console.log('dialogs/' + get("dialogId"));
        let r = getSO('dialogs/' + get("dialogId"));
        expect(r.status, 'response status').to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'dialog').to.have.property("id").to.equal(get("dialogId"));
        expect(r.json(), 'dialog').to.have.property("createdAt");        
        expect(r.json().createdAt, 'createdAt').to.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z$/);
        expect(r.json(), 'dialog').to.have.property("updatedAt");
        expect(r.json().updatedAt, 'updatedAt').to.equal(r.json().createdAt);
        expect(r.json(), 'dialog').to.have.property('eTag');
        
        set("etag", r.json()["eTag"]);        
    });

    describe('Perform patch', () => {
        let newApiActionEndpointUrl = "https://digdir.no/" + uuidv4();
        let patchDocument = [
            {
                "op": "replace",
                "path": "/apiActions/0/endpoints/1/url",
                "value": newApiActionEndpointUrl
            }
        ];
        let r = patchSO('dialogs/' + get("dialogId"), JSON.stringify(patchDocument));
        expect(r.status, 'response status').to.equal(204);
        set("apiendpoint", newApiActionEndpointUrl);
    });

    describe('Perform dialog get after patch', () => {
        let r = getSO('dialogs/' + get("dialogId"));
        expect(r.status, 'response status').to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'dialog').to.have.property("updatedAt");        
        expect(r.json().updatedAt, 'updatedAt').to.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z$/);
        expect(r.json(), 'dialog').to.have.property("createdAt");
        expect(r.json().updatedAt, 'updatedAt').to.not.equal(r.json().createdAt);
        expect(r.json(), 'dialog').to.have.property('eTag');
        expect(r.json().eTag, 'updated eTag').to.not.equal(get("etag"));  
        expect(r.json(), 'dialog').to.have.nested.property("apiActions[0].endpoints[1].url").to.equal(get("apiendpoint"));
    });

    describe('Perform dialog delete', () => {
        let r = deleteSO('dialogs/' + get("dialogId"));
        expect(r.status, 'response status').to.equal(204);
    });
}