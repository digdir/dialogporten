import { expect, expectStatusFor, describe, getSO, purgeSO } from './testimports.js'
import { sentinelValue } from './config.js';

export default function () {

    let dialogId = null;
    const tokenOptions = {
        scopes: "digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search digdir:dialogporten.serviceprovider.admin digdir:dialogporten.correspondence"
    }
    describe('Post run: checking for unpurged dialogs', () => {
        let r = getSO('dialogs/?Search=' + sentinelValue, null, tokenOptions);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        var response = r.json();
        if (response.items && response.items.length > 0) {
            console.error("Found " + response.items.length + " unpurged dialogs, make sure that all tests clean up after themselves. Purging ...");
            response.items.forEach((item) => {
                console.warn("Sentinel purging dialog with id: " + item.id)
                let r = purgeSO('dialogs/' + item.id, null, tokenOptions);
                if (r.status != 204) {
                    console.error("Failed to purge dialog with id: " + item.id);
                    console.log(r);
                }
            });

            // Fail the test after purging for visibility
            expect(response.items.length, 'unpurged dialogs').to.equal(0);
        }
    });
}
