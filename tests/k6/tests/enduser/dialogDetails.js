import { 
    describe, expect, expectStatusFor,
    getEU,
    setVisibleFrom, 
    postSO,
    purgeSO } from '../../common/testimports.js'

import { default as dialogToInsert } from '../serviceowner/testdata/01-create-dialog.js';
export default function () {

    let dialogId = null;
    let dialog = null;

    describe('Arrange: Create a dialog to test against', () => {
        let d = dialogToInsert();
        setVisibleFrom(d, null);
        let r = postSO("dialogs", d);
        expectStatusFor(r).to.equal(201);
        dialogId = r.json();
    });

    describe('Perform simple dialog get', () => {
        let r = getEU('dialogs/' + dialogId);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("id").to.equal(dialogId);

        dialog = r.json();
    });

    describe('Check that authorized actions have real URLs', () => {
        if (dialog == null) return;
        expect(dialog, 'dialog').to.have.property("guiActions").with.lengthOf(1);
        expect(dialog.guiActions[0], 'gui action').to.have.property("isAuthorized").to.equal(true);
        expect(dialog.guiActions[0], 'url').to.have.property("url").to.include("https://");
    });
    
    describe('Check that unauthorized actions to have default URLs', () => {
        if (dialog == null) return;
        expect(dialog, 'dialog').to.have.property("apiActions").with.lengthOf(1);
        expect(dialog.apiActions[0], 'api action').to.have.property("isAuthorized").to.equal(false);
        expect(dialog.apiActions[0], 'url').to.have.property("endpoints");
        for (let i=0; i<dialog.apiActions[0].length; i++) {
            console.log(dialog.apiActions[i]);
            expect(dialog.apiActions[i], 'endpoint').to.have.property("url").to.equal("urn:dialogporten:unauthorized");
        }
    });

    describe("Cleanup", () => {
        if (dialog == null) return;
        let r = purgeSO("dialogs/" + dialogId);
        expect(r.status, 'response status').to.equal(204);
    });

    describe("Check if we get 404 Not found", () => {
        if (dialog == null) return;
        let r = getEU('dialogs/' + dialogId);
        expectStatusFor(r).to.equal(404);
    });
}