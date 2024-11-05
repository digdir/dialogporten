/**
 * This file contains common functions for performing simple searches
 * and GraphQL searches.
 */
import { randomItem, uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { expect, expectStatusFor } from "../../common/testimports.js";
import { describe } from '../../common/describe.js';
import { getEU, postGQ } from '../../common/request.js';
import { getGraphqlParty } from '../performancetest_data/graphql-search.js';


/**
 * Retrieves the content for a dialog.
 * Get dialog, dialog activities, seenlogs, labellog, and transmissions.
 * @param {Object} response - The response object.
 * @param {Object} paramsWithToken - The parameters with token.
 * @returns {void}
 */
function retrieveDialogContent(response, paramsWithToken) {
    const items = response.json().items;
    if (!items?.length) return;
        
    const dialogId = items[0].id;
    if (!dialogId) return;
        
    getContent(dialogId, paramsWithToken, 'get dialog');
    getContentChain(dialogId, paramsWithToken, 'get dialog activities', 'get dialog activity', '/activities/')
    getContentChain(dialogId, paramsWithToken, 'get seenlogs', 'get seenlog', '/seenlog/')
    getContent(dialogId, paramsWithToken, 'get labellog', '/labellog');
    getContentChain(dialogId, paramsWithToken, 'get transmissions', 'get transmission', '/transmissions/')
}

/**
 * Performs a simple search.
 * @param {Object} enduser - The end user.
 * @returns {void}
 */
export function enduserSearch(enduser) {
    let paramsWithToken = {
        headers: {
            Authorization: "Bearer " + enduser.token,
            traceparent: uuidv4()
        },
        tags: { name: 'enduser search' }
    }
    let defaultParty = "urn:altinn:person:identifier-no:" + enduser.ssn;
    let defaultFilter = "?Party=" + defaultParty;
    describe('Perform enduser dialog list', () => {
        let r = getEU('dialogs' + defaultFilter, paramsWithToken);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        retrieveDialogContent(r, paramsWithToken);
    });
}

/**
 * Performs a enduser search.
 * @param {string} dialogId - The dialog id.
 * @param {Object} paramsWithToken - The parameters with token.
 * @param {string} tag - Tagging the request.
 * @param {string} path - The path to append to the URL. Can be empty or /labellog.
 * @returns {void}
 */
export function getContent(dialogId, paramsWithToken, tag, path = '') {
    const listParams = {
        ...paramsWithToken,
        tags: { ...paramsWithToken.tags, name: tag }
    };
    getUrl('dialogs/' + dialogId + path, listParams);
}

/**
 * Retrieves the content chain.
 * @param {string} dialogId - The dialog id.
 * @param {Object} paramsWithToken - The parameters with token.
 * @param {string} tag - Tagging the request.
 * @param {string} subtag - Tagging the sub request.
 * @param {string} endpoint - The endpoint to append to the URL.
 * @returns {void}
 */
export function getContentChain(dialogId, paramsWithToken, tag, subtag, endpoint) {
    const listParams = {
        ...paramsWithToken,
        tags: { ...paramsWithToken.tags, name: tag }
    };
    let d = getUrl('dialogs/' + dialogId + endpoint, listParams);
    let json = d.json();
    if (json.length > 0) {
        const detailParams = {
            ...paramsWithToken,
            tags: { ...paramsWithToken.tags, name: subtag }
        };
        getUrl('dialogs/' + dialogId + endpoint + randomItem(json).id, detailParams);
    }
}

/**
 * Performs a GET request to the specified URL with the provided parameters.
 * @param {string} url - The URL to send the GET request to.
 * @param {Object} paramsWithToken - The parameters with token.
 * @returns {Object} The response object.
 */
export function getUrl(url, paramsWithToken) {
    let r = getEU(url, paramsWithToken);
    expectStatusFor(r).to.equal(200);
    expect(r, 'response').to.have.validJsonBody();
    return r;
}

/**
 * Performs a GraphQL search using the provided enduser token.
 * 
 * @param {Object} enduser - The enduser object containing the token.
 * @returns {void}
 */
export function graphqlSearch(enduser) {
    let paramsWithToken = {
        headers: {
            Authorization: "Bearer " + enduser.token,
            traceparent: uuidv4()
        },
        tags: { name: 'graphql search' }
    };
    describe('Perform graphql dialog list', () => {
        let r = postGQ(getGraphqlParty(enduser.ssn), paramsWithToken);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
    });
}
