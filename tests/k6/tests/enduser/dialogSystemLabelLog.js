import {
    describe, expect, expectStatusFor, getEU, postEU, postSO, putSO, purgeSO
} from '../../common/testimports.js'

import {default as dialogToInsert} from '../serviceowner/testdata/01-create-dialog.js';

export default function () {
    //Lag 1 dialog, gjør forandriger på den, sjekk om det er log
    let dialogId;
    let dialog;
    describe('Arrange: Create a dialog to test against', () => {
        let d = dialogToInsert();
        let r = postSO("dialogs", d);
        expectStatusFor(r).to.equal(201);
        dialogId = r.json();
        dialog = d;
    });

    describe('Update label as enduser', () => {
        let body = {
            'label': 'systemlabel:Bin'
        }
        let response = postEU('dialogs/' + dialogId + '/labels', body);
        expectStatusFor(response).to.equal(204);
        response = getEU('dialogs/' + dialogId + '/labellog');
        expectStatusFor(response).to.equal(200);
        expect(response, 'response').to.have.validJsonBody();
        console.log(response.json());
        expect(response.json(), 'response body').to.have.lengthOf(1);
    });

    describe('Changing labels trigger 2 logs', () => {

        let body = {
            'label': 'systemlabel:Archive'
        }
        let response = postEU('dialogs/' + dialogId + '/labels', body);
        expectStatusFor(response).to.equal(204);
        response = getEU('dialogs/' + dialogId + '/labellog');
        expectStatusFor(response).to.equal(200);
        expect(response, 'response').to.have.validJsonBody();
        console.log(response.json());
        expect(response.json(), 'response body').to.have.lengthOf(3);
    })

    describe('Dialog update set system label to default', () => {
        dialog.progress = "60";
        let response = putSO('dialogs/' + dialogId, dialog);
        expectStatusFor(response).to.equal(204);
        response = getEU('dialogs/' + dialogId + '/labellog');
        expectStatusFor(response).to.equal(200);
        expect(response, 'response').to.have.validJsonBody();
        console.log(response.json());
        expect(response.json(), 'response body').to.have.lengthOf(4);
    })

    describe('Cleanup', () => {
        let response = purgeSO("dialogs/" + dialogId);
        expectStatusFor(response).to.equal(204);
    })
}
