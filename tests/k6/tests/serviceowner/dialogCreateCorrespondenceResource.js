import {describe, expect, expectStatusFor, postSO, purgeSO} from '../../common/testimports.js'
import {default as dialogToInsert} from './testdata/01-create-dialog.js';

export default function () {
    let dialogIds = [];
    let tokenOptions = {
        scopes: "digdir:dialogporten digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search digdir:dialogporten.correspondence"
    }
    describe('dialog create correspondence resource without correct scope ', () => {
        let dialog = dialogToInsert();
        dialog.serviceResource = "urn:altinn:resource:ttd-dialogporten-automated-tests-correspondence";
        let response = postSO('dialogs', dialog);
        expectStatusFor(response).to.equal(403);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response body').to.have.property('errors');
    })

    describe('dialog create correspondence resource with correct scope', () => {
        let dialog = dialogToInsert();
        dialog.serviceResource = "urn:altinn:resource:ttd-dialogporten-automated-tests-correspondence";
        let response = postSO('dialogs', dialog, null, tokenOptions)
        expectStatusFor(response).to.equal(201);
        expect(response, 'response').to.have.validJsonBody();
        expect(response.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        dialogIds.push(response.json());
    })

    describe("Cleanup", () => {
        dialogIds.forEach((d) => {
            let r = purgeSO("dialogs/" + d, null, tokenOptions);
            expect(r.status, 'response status').to.equal(204);
        });
    });
}
