import { describe, expect, expectStatusFor, getEU } from '../../common/testimports.js'

export default function () {
    describe('Check if we get any parties', () => {
        let r = getEU("parties");
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("authorizedParties").with.lengthOf.at.least(2);
    });
}