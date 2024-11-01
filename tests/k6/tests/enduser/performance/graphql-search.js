import { postGQ, expect, expectStatusFor, describe } from "../../../common/testimports.js";
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { getGraphqlParty } from '../../performancetest_data/graphql-search.js';
import { getEndusers, getDefaultThresholds } from '../../performancetest_common/common.js'


const filenameEndusers = '../../performancetest_data/.endusers-with-tokens.csv';

const endUsers = getEndusers(filenameEndusers);

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['graphql search'])
};

export default function() {
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
        graphqlSearch(endUsers[0]);
    }
    else {
        graphqlSearch(randomItem(endUsers));
    }
}

export function graphqlSearch(enduser) {
    let paramsWithToken = {
        headers: {
            Authorization: "Bearer " + enduser.token,
        },
        tags: { name: 'graphql search' }
    }
    describe('Perform graphql dialog list', () => {
        //let r = ('dialogs' + defaultFilter, paramsWithToken);
        let r = postGQ(getGraphqlParty(enduser.ssn), paramsWithToken); 
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
    });
}

