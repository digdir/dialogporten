import { describe, expect, expectStatusFor, getSO, postSO, putSO, patchSO, deleteSO, uuidv4 } from '../../common/testimports.js'

export default function () {

    let paramsWithoutAuthorizationHeader = {
        headers: {
            'Accept': 'application/json',
            'User-Agent': 'dialogporten-k6',
            'Authorization': null // this will signal that the default Authorization header is to be removed
        }
    };

    let paramsWithInvalidAuthorizationHeader = {
        headers: {
            'Accept': 'application/json',
            'User-Agent': 'dialogporten-k6',
            'Authorization': 'Bearer thisisnotavalidtoken'
        }
    };

    let expect401InvalidRequest = function(r) {
        expectStatusFor(r).to.equal(401);
        expect(r.headers, 'reponse headers').has.property('Www-Authenticate');
        expect(r.headers['Www-Authenticate'], 'WWW-Authenticate header').contain('Bearer');
    }

    let expect401InvalidToken = function(r) {
        expectStatusFor(r).to.equal(401);
        expect(r.headers, 'reponse headers').has.property('Www-Authenticate');
        expect(r.headers['Www-Authenticate'], 'WWW-Authenticate header').contain('Bearer error="invalid_token"');
    }

    let permutations = [
        [paramsWithoutAuthorizationHeader, expect401InvalidRequest, 'without'],
        [paramsWithInvalidAuthorizationHeader, expect401InvalidToken, 'with invalid']
    ];

    permutations.forEach(([params, expectation, message]) => {
        describe(`Attempt search ${message} token`, () => {
            let r = getSO("dialogs", params);
            expectation(r);
        });

        describe(`Attempt get single ${message} token`, () => {
            let r = getSO("dialogs/" + uuidv4(), params);
            expectation(r);
        });

        describe(`Attempt dialog create ${message} token`, () => {
            let r = postSO("dialogs", {}, params);
            expectation(r);
        });

        describe(`Attempt dialog put ${message} token`, () => {
            let r = putSO("dialogs/" + uuidv4(), {}, params);
            expectation(r);
        });

        describe(`Attempt dialog patch ${message} token`, () => {
            let r = patchSO("dialogs/" + uuidv4(), {}, params);
            expectation(r);
        });

        describe(`Attempt dialog delete ${message} token`, () => {
            let r = deleteSO("dialogs/" + uuidv4(), params);
            expectation(r);
        });

        describe(`Attempt dialog element get ${message} token`, () => {
            let r = getSO("dialogs/" + uuidv4() + "/elements/" + uuidv4(), params);
            expectation(r);
        });

        describe(`Attempt dialog element post ${message} token`, () => {
            let r = postSO("dialogs/" + uuidv4() + "/elements/", {}, params);
            expectation(r);
        });

        describe(`Attempt activity history get ${message} token`, () => {
            let r = getSO("dialogs/" + uuidv4() + "/activities/" + uuidv4(), params);
            expectation(r);
        });

        describe(`Attempt activity history post ${message} token`, () => {
            let r = postSO("dialogs/" + uuidv4() + "/activities", {}, params);
            expectation(r);
        });

    });
}
