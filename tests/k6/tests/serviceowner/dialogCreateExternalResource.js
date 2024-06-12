import { describe, expect, expectStatusFor, postSO, purgeSO } from '../../common/testimports.js';
import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {

    function testDialogCreate(type, authorizationAttribute, expectedStatus, expectErrors = false) {
        let dialog = dialogToInsert();
        dialog[type][0].authorizationAttribute = authorizationAttribute;
        let r = postSO('dialogs', dialog);
        expectStatusFor(r).to.equal(expectedStatus);
        expect(r, 'response').to.have.validJsonBody();
        
        if (expectedStatus === 201) {
            let r2 = purgeSO('dialogs/' + r.json());
            expectStatusFor(r2).to.equal(204);
        }
        
        if (expectErrors) {
            expect(r.json(), 'response').to.have.property('errors');
        }
    }

    describe('Attempt dialog create with API action referring available external resource', () => {
        testDialogCreate('apiActions', "urn:altinn:resource:super-simple-service", 201);
    });

    describe('Attempt dialog create with GUI action referring available external resource', () => {
        testDialogCreate('guiActions', "urn:altinn:resource:super-simple-service", 201);
    });

    describe('Attempt dialog create with dialog element referring available external resource', () => {
        testDialogCreate('elements', "urn:altinn:resource:super-simple-service", 201);
    });

    describe('Attempt dialog create with API action referring unavailable resource', () => {
        testDialogCreate('apiActions', "urn:altinn:resource:notavailable", 403, true);
    });

    describe('Attempt dialog create with GUI action referring unavailable resource', () => {
        testDialogCreate('guiActions', "urn:altinn:resource:notavailable", 403, true);
    });

    describe('Attempt dialog create with dialog element referring unavailable resource', () => {
        testDialogCreate('elements', "urn:altinn:resource:notavailable", 403, true);
    });
}
