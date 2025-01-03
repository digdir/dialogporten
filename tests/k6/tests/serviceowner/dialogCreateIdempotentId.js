import {describe, expect, expectStatusFor, postSO, purgeSO} from '../../common/testimports.js'
import {default as dialogToInsert} from './testdata/01-create-dialog.js';


export default function () {

    const dialogs = [];
    let idempotentIndex = 0;
    const navOrg = {
        orgName: "nav",
        orgNo: "889640782",
        scopes: "digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search digdir:dialogporten.serviceprovider.legacyhtml altinn:system/notifications.condition.check digdir:dialogporten.correspondence"
    };

    describe('Attempt to create dialog with unused idempotentId', () => {
        let dialog = dialogToInsert();
        let idempotentId = "idempotent" + idempotentIndex++;

        dialog.idempotentId = idempotentId;
        let r = postSO('dialogs', dialog);
        expectStatusFor(r).to.equal(201);

        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/);

        dialogs.push({
            id: r.json(),
            org: null
        });
    })

    describe('Attempt to create dialog with same idempotentId different Org', () => {
        let idempotentId = "idempotent" + idempotentIndex++;
        let dialog = dialogToInsert();
        dialog.idempotentId = idempotentId;
        dialog.serviceResource = "urn:altinn:resource:app_nav_barnehagelister";
        dialog.activities[2].performedBy.actorId = "urn:altinn:organization:identifier-no:889640782";

        let responseNav = postSO('dialogs', dialog, null, navOrg);
        expectStatusFor(responseNav).to.equal(201);

        expect(responseNav, 'response').to.have.validJsonBody();
        expect(responseNav.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/);

        dialogs.push({
            id: responseNav.json(),
            org: navOrg
        })

        dialog = dialogToInsert();
        dialog.idempotentId = idempotentId;

        let responseDigdir = postSO('dialogs', dialog);
        expectStatusFor(responseDigdir).to.equal(201);

        expect(responseDigdir, 'response').to.have.validJsonBody();
        expect(responseDigdir.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/);

        dialogs.push({
            id: responseDigdir.json(),
            org: null
        });
    })

    describe('Attempt to create dialog with used idempotentId', () => {
        let dialog = dialogToInsert();
        let idempotentId = "idempotent" + idempotentIndex++;

        dialog.idempotentId = idempotentId;
        let r = postSO('dialogs', dialog);
        expectStatusFor(r).to.equal(201);

        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/);

        let dialogId = r.json();
        dialogs.push({
            id: dialogId,
            org: null
        });

        r = postSO('dialogs', dialog);
        expectStatusFor(r).to.equal(409);

        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.property('errors');
        expect(r.json()['errors'], 'response json errors').to.property('Conflict');
        expect(r.json()['errors']['Conflict'][0], 'response json Conflict').to.contain(dialogId);
    })

    describe('Attempt to create dialog with too long idempotentId', () => {
        let dialog = dialogToInsert();
        let idempotentId = "this idempotent id is way to long the length of this idempotent id exceeds the 36 character limit";

        dialog.idempotentId = idempotentId;

        let r = postSO('dialogs', dialog);
        expectStatusFor(r).to.equal(400);

        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property('errors');

    })

    describe('Cleanup', () => {
        let i;
        for (i = 0; i < dialogs.length; i++) {
            let r = purgeSO('dialogs/' + dialogs[i].id, null, dialogs[i].org);
            expectStatusFor(r).to.equal(204);
        }
        expect(dialogs.length).to.equal(i);
    });
}
