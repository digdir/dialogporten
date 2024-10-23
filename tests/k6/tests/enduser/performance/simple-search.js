import { getEU, expect, expectStatusFor, describe } from "../../../common/testimports.js";
import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const filenameEndusers = '../../performancetest_data/.endusers-with-tokens.csv';

const endUsers = new SharedArray('endUsers', function () {
    return papaparse.parse(open(filenameEndusers), { header: true, skipEmptyLines: true }).data;
  });

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: {
        'http_req_duration{name:simple search}': [],
        'http_reqs{name:simple search}': [],
    },
};

export default function() {
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
        simpleSearch(endUsers[0]);
    }
    else {
        while (true) { simpleSearch(randomItem(endUsers)); }
    }
}

export function simpleSearch(enduser) {
    let paramsWithToken = {
        Headers: {
            Authorization: "Bearer " + enduser.token
        },
        tags: { name: 'simple search' }
    }
    let defaultParty = "urn:altinn:person:identifier-no:" + enduser.ssn;
    let defaultFilter = "?Party=" + defaultParty;
    describe('Perform simple dialog list', () => {
        let r = getEU('dialogs' + defaultFilter, paramsWithToken);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
    });
}
