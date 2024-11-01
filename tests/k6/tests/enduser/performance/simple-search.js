import { getEU, expect, expectStatusFor, describe } from "../../../common/testimports.js";
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { getEndusers, getDefaultThresholds } from '../../performancetest_common/common.js'

const filenameEndusers = '../../performancetest_data/.endusers-with-tokens.csv';

const endUsers = getEndusers(filenameEndusers);

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['simple search', 
        'get dialog', 
        'get dialog activities', 
        'get dialog activity', 
        'get seenlogs',
        'get seenlog',
        'get transmissions',
        'get transmission',
        'get labellog'
    ])
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
                getContent(dialogId, paramsWithToken, 'get dialog');
                getContentChain(dialogId, paramsWithToken, 'get dialog activities', 'get dialog activity', '/activities/')
                getContentChain(dialogId, paramsWithToken, 'get seenlogs', 'get seenlog', '/seenlog/')
                getContent(dialogId, paramsWithToken, 'get labellog', '/labellog');
                getContentChain(dialogId, paramsWithToken, 'get transmissions', 'get transmission', '/transmissions/')
            }
        }
    });
}

export function getContent(dialogId, paramsWithToken, tag, path = '') {
    paramsWithToken.tags.name = tag
    getUrl('dialogs/' + dialogId + path, paramsWithToken);
}

export function getContentChain(dialogId, paramsWithToken, tag, subtag, endpoint) {
    paramsWithToken.tags.name = tag;
    let d = getUrl('dialogs/' + dialogId + endpoint, paramsWithToken);
    let json = d.json();
    if (json.length > 0) {
        paramsWithToken.tags.name = subtag;
        getUrl('dialogs/' + dialogId + endpoint + randomItem(json).id, paramsWithToken);
    }
}

export function getUrl(url, paramsWithToken) {
    let r = getEU(url, paramsWithToken);
    expectStatusFor(r).to.equal(200);
    expect(r, 'response').to.have.validJsonBody();
    return r;
}

