import { getEU, expect, expectStatusFor, describe } from "../../../common/testimports.js";
import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const filenameEndusers = '../../performancetest_data/.endusers-with-tokens.csv';

const endUsers = new SharedArray('endUsers', function () {
    try {
        const csvData = papaparse.parse(open(filenameEndusers), { header: true, skipEmptyLines: true }).data;
        if (!csvData.length) {
            throw new Error('No data found in CSV file');
        }
        csvData.forEach((user, index) => {
            if (!user.token || !user.ssn) {
                throw new Error(`Missing required fields at row ${index + 1}`);
            }
        });
        return csvData;
    } catch (error) {
        throw new Error(`Failed to load end users: ${error.message}`);
    }
});

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: {
        http_req_failed: ['rate<0.01'],
        'http_req_duration{name:simple search}': [],
        'http_reqs{name:simple search}': [],
        'http_req_duration{name:get dialog}': [],
        'http_reqs{name:get dialog}': [],
        'http_req_duration{name:get dialog activities}': [],
        'http_reqs{name:get dialog activities}': [],
        'http_req_duration{name:get dialog activity}': [],
        'http_reqs{name:get dialog activity}': [],
        'http_req_duration{name:get seenlogs}': [],
        'http_reqs{name:get seenlogs}': [],
        'http_req_duration{name:get seenlog}': [],
        'http_reqs{name:get seenlog}': [],
        'http_req_duration{name:get transmissions}': [],
        'http_reqs{name:get transmissions}': [],
        'http_req_duration{name:get transmission}': [],
        'http_reqs{name:get transmission}': [],
        'http_req_duration{name:get labellog}': [],
        'http_reqs{name:get labellog}': [],
    },
};

export default function() {
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
        simpleSearch(endUsers[0]);
    }
    else {
        simpleSearch(randomItem(endUsers));
    }
}

export function simpleSearch(enduser) {
    let paramsWithToken = {
        headers: {
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
        if (r.json().items && r.json().items.length > 0) {
            let dialogId = r.json().items[0].id;
            if (dialogId) {
                getDialog(dialogId, paramsWithToken);
                getDialogActivities(dialogId, paramsWithToken);
                getSeenLog(dialogId, paramsWithToken);
                getLabelLog(dialogId, paramsWithToken);
                getDialogTransmissions(dialogId, paramsWithToken);
            }
        }
    });
}

export function getDialog(dialogId, paramsWithToken) {
    paramsWithToken.tags.name = 'get dialog'
    getUrl('dialogs/' + dialogId, paramsWithToken);
}

export function getDialogActivities(dialogId, paramsWithToken) {
    paramsWithToken.tags.name = 'get dialog activities'
    let d = getUrl('dialogs/' + dialogId + '/activities', paramsWithToken);
    let dialog_activites = d.json();
    if (dialog_activites.length > 0) {
        paramsWithToken.tags.name = 'get dialog activity'
        getUrl('dialogs/' + dialogId + '/activities/' + randomItem(dialog_activites).id, paramsWithToken);
    }
}

export function getSeenLog(dialogId, paramsWithToken) {
    paramsWithToken.tags.name = 'get seenlogs'
    let s = getEU('dialogs/' + dialogId + '/seenlog', paramsWithToken);
    let seen_logs = s.json();
    if (seen_logs.length > 0) {
        paramsWithToken.tags.name = 'get seenlog'
        getUrl('dialogs/' + dialogId + '/seenlog/' + randomItem(seen_logs).id, paramsWithToken);
    }
}

export function getLabelLog(dialogId, paramsWithToken) {
    paramsWithToken.tags.name = 'get labellog'
    getEU('dialogs/' + dialogId + '/labellog', paramsWithToken);
}

export function getDialogTransmissions(dialogId, paramsWithToken) {
    paramsWithToken.tags.name = 'get transmissions'
    let d = getUrl('dialogs/' + dialogId + '/transmissions', paramsWithToken);
    let dialog_transmissions = d.json();
    if (dialog_transmissions.length > 0) {
        paramsWithToken.tags.name = 'get transmission'
        getUrl('dialogs/' + dialogId + '/transmissions/' + randomItem(dialog_transmissions).id, paramsWithToken);
    }
}

export function getUrl(url, paramsWithToken) {
    let r = getEU(url, paramsWithToken);
    expectStatusFor(r).to.equal(200);
    expect(r, 'response').to.have.validJsonBody();
    return r;
}

