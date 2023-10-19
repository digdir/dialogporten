import { describe, expect, getSO } from '../../common/testimports.js'

export default function () {
    describe('Perform simple dialog search', () => {
        let r = getSO('dialogs');
        expect(r.status, 'response status').to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf.at.least(1);
    });    
}