import {
    describe,
    expect,
    expectStatusFor,
    getSO,
    postSO
} from '../../common/testimports.js'
import {default as dialogToInsert} from './testdata/01-create-dialog.js';
import { getDefaultEnduserSsn } from "../../common/token.js";

export default function () {

    let dialogId = null;
    let endUserId = "urn:altinn:person:identifier-no:" + getDefaultEnduserSsn();
    let invalidEndUserId = "urn:altinn:person:identifier-no:08895699684";

    describe('Perform dialog create', () => {
        let r = postSO('dialogs', dialogToInsert());
        expectStatusFor(r).to.equal(201);
        expect(r, 'response').to.have.validJsonBody();

        dialogId = r.json();
    });

    describe('Perform dialog get with endUserId', () => {
        let r = getSO('dialogs/' + dialogId + '?endUserId=' + endUserId);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'dialog').to.have.property("id").to.equal(dialogId);
    });

    describe('Perform dialog get with invalid endUserId', () => {
        let r = getSO('dialogs/' + dialogId + '?endUserId=' + invalidEndUserId);
        expectStatusFor(r).to.equal(404);
    });
}
