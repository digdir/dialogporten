import { describe, expect, expectStatusFor, postSO, postSOAsync, deleteSO } from '../../common/testimports.js'
import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {

    let dialogId = null;
 
    describe('Perform dialog create', () => {
        let r = postSO('dialogs', dialogToInsert());
        expectStatusFor(r).to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        
        dialogId = r.json();
    });

    describe('Attempt to add a few child entities concurrently', async () => {

        let promises = [];
        
        for (var i=0; i<4; i++) {
            let activity = { type: "Information", description: [ { value: i.toString(), cultureCode: "nb-no"}]};
            promises.push(postSOAsync('dialogs/' + dialogId + '/activities?' + i, activity))
        }

        try {
            const results = await Promise.all(promises);
            results.forEach((r) => {
                expect(r.json(), 'all status codes for concurrently added child entities').to.equal(201);
            });
        } catch (_) {}
   
    });

    describe('Cleanup', () => {
        let r = deleteSO('dialogs/' + dialogId);
        expectStatusFor(r).to.equal(204);
    });
}