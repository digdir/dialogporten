import {describe, expect, expectStatusFor, postSO, purgeSO, putSO, patchSO} from '../../common/testimports.js'
import {default as dialogToInsert} from './testdata/01-create-dialog.js';

export default function () {
    const dialogIds = [];
    const dialogs = [];
    const tokenOptions = {
        scopes: "digdir:dialogporten digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search digdir:dialogporten.correspondence"
    }
    describe('create dialog with correspondence  resource without correct scope', () => {
        const dialog = dialogToInsert();
        dialog.serviceResource = "urn:altinn:resource:ttd-dialogporten-automated-tests-correspondence";
        const response = postSO('dialogs', dialog);
        expectStatusFor(response).to.equal(403);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response body').to.have.property('errors');
    })

    describe('create dialog with correspondence resource with correct scope', () => {
        const dialog = dialogToInsert();
        dialog.serviceResource = "urn:altinn:resource:ttd-dialogporten-automated-tests-correspondence";
        const response = postSO('dialogs', dialog, null, tokenOptions)
        expectStatusFor(response).to.equal(201);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        dialogIds.push(response.json());
        dialogs.push(dialog);
    })

    describe('update dialog with correspondence resource without correct scope', () => {
        const dialog = dialogs[dialogs.length - 1];
        const dialogId = dialogIds[dialogs.length - 1];
        dialog.progress = 80;

        const response = putSO('dialogs/' + dialogId, dialog);
        expectStatusFor(response).to.equal(403);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response body').to.have.property('errors');

    })

    describe('update dialog with correspondence resource with correct scope', () => {
        const dialog = dialogs[dialogs.length - 1];
        const dialogId = dialogIds[dialogs.length - 1];
        dialog.progress = 80;

        const response = putSO('dialogs/' + dialogId, dialog, null, tokenOptions);
        expectStatusFor(response).to.equal(204);
        dialogs[dialogs.length - 1] = dialog;

    })
    describe('patch dialog with correspondence resource without correct scope', () => {
        const dialogId = dialogIds[dialogs.length - 1];
        const patch = [
            {
                'op': 'replace',
                'path': '/progress',
                'value': 80
            }
        ]
        const response = patchSO('dialogs/' + dialogId, patch);
        expectStatusFor(response).to.equal(403);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response body').to.have.property('errors');
    })
    
    describe('patch dialog with correspondence resource with correct scope', () => {
        const dialogId = dialogIds[dialogs.length - 1];
        const patch = [
            {
                'op': 'replace',
                'path': '/progress',
                'value': 80
            }
        ]
        const response = patchSO('dialogs/' + dialogId, patch, null, tokenOptions);
        expectStatusFor(response).to.equal(204);
    })

    describe('deconste dialog with correspondence resource without correct scope', () => {
        const dialogId = dialogIds.pop();
        const response = purgeSO('dialogs/' + dialogId);
        expectStatusFor(response).to.equal(403);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response body').to.have.property('errors');
        dialogIds.push(dialogId);
    })

    describe('deconste dialog with correspondence resource with correct scope', () => {
        const dialogId = dialogIds.pop();
        const r = purgeSO("dialogs/" + dialogId, null, tokenOptions);
        expectStatusFor(r).to.equal(204)
    })

}
