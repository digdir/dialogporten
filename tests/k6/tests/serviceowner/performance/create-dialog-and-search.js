import { postSO, getEU, expectStatusFor, expect, describe } from "../../../common/testimports.js";
import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { default as dialogToInsert } from '../../performancetest_data/01-create-dialog.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const filenameServiceowners = '../../performancetest_data/.serviceowners-with-tokens.csv';

const serviceOwners = new SharedArray('serviceOwners', function () {
  return papaparse.parse(open(filenameServiceowners), { header: true, skipEmptyLines: true }).data;
});

const filenameEndusers = '../../performancetest_data/.endusers-with-tokens.csv';

const endUsers = new SharedArray('endUsers', function () {
    return papaparse.parse(open(filenameEndusers), { header: true, skipEmptyLines: true }).data;
  });

export const options = {
    scenarios: {
      create_dialogs: {
        executor: 'constant-vus',
        tags: { name: 'create dialog'},
        exec: 'createDialogs',
        vus: __ENV.svus,
        duration: __ENV.duration
      },
      simple_search: {
        executor: 'constant-vus',
        tags: { name: 'simple search'},
        exec: 'simpleSearches',
        vus: __ENV.evus,
        duration: __ENV.duration,
      }


    },
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: {
        'http_req_duration{name:create dialog}': [],
        'http_reqs{name:create dialog}': [],
        'http_req_duration{name:simple search}': [],
        'http_reqs{name:simple search}': [],
        'http_req_duration{name:get dialog}': [],
        'http_reqs{name:get dialog}': [],
    },
};

export function createDialogs() {
  if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
    createDialog(serviceOwners[0], endUsers[0]);
  }
  else {
    createDialog(randomItem(serviceOwners), randomItem(endUsers));
  }
}

export function createDialog(serviceOwner, endUser) {  
    var paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceOwner.token
        },
        tags: { name: 'create dialog' }
    }

    describe('create dialog', () => {
        let r = postSO('dialogs', dialogToInsert(endUser.ssn, endUser.resource), paramsWithToken);  
        expect(r.status, 'response status').to.equal(201);
    });
    
}

export function simpleSearches() {
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
      paramsWithToken.tags.name = 'simple search'
      let r = getEU('dialogs' + defaultFilter, paramsWithToken);
      expectStatusFor(r).to.equal(200);
      expect(r, 'response').to.have.validJsonBody();
      if ( r.json().items.length > 0 ) {
        let dialogId = r.json().items[0].id;
        if (dialogId) {
          paramsWithToken.tags.name = 'get dialog'
          let d = getEU('dialogs/' + dialogId, paramsWithToken);
          expectStatusFor(d).to.equal(200);
          expect(d, 'response').to.have.validJsonBody();
        }
      }
  });
}