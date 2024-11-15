/**
 * This file contains common functions for performing simple searches
 * and GraphQL searches.
 */
import { randomItem, uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { expect, expectStatusFor } from "../../common/testimports.js";
import { describe } from '../../common/describe.js';
import { getEU, postGQ, getSO } from '../../common/request.js';
import { getGraphqlParty } from '../performancetest_data/graphql-search.js';

/**
 * Retrieves the content for a dialog.
 * Get dialog, dialog activities, seenlogs, labellog, and transmissions.
 * @param {Object} response - The response object.
 * @param {Object} paramsWithToken - The parameters with token.
 * @returns {void}
 */
function retrieveDialogContent(response, paramsWithToken, getFunction = getEU) {
    const items = response.json().items;
    if (!items?.length) return;
        
    const dialogId = items[0].id;
    if (!dialogId) return;
        
    getContent(dialogId, paramsWithToken, 'get dialog', '', getFunction);
    getContentChain(dialogId, paramsWithToken, 'get dialog activities', 'get dialog activity', '/activities/', getFunction);
    getContentChain(dialogId, paramsWithToken, 'get seenlogs', 'get seenlog', '/seenlog/', getFunction);
    if (getFunction == getEU) {
        getContent(dialogId, paramsWithToken, 'get labellog', '/labellog', getFunction);
    }
    getContentChain(dialogId, paramsWithToken, 'get transmissions', 'get transmission', '/transmissions/', getFunction);
}

/**
 * Performs a enduser search.
 * @param {Object} enduser - The end user.
 * @returns {void}
 */
export function enduserSearch(enduser) {
    var traceparent = uuidv4();
    let paramsWithToken = {
        headers: {
            Authorization: "Bearer " + enduser.token,
            traceparent: traceparent
        },
        tags: { name: 'enduser search', traceparent: traceparent } 
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
 * @param {function} getFunction - The get function to use.
 * @returns {void}
 */
export function getContent(dialogId, paramsWithToken, tag, path = '', getFunction = getEU) {
    const listParams = {
        ...paramsWithToken,
        tags: { ...paramsWithToken.tags, name: tag }
    };
    getUrl('dialogs/' + dialogId + path, listParams, getFunction);
}

/**
 * Retrieves the content chain.
 * @param {string} dialogId - The dialog id.
 * @param {Object} paramsWithToken - The parameters with token.
 * @param {string} tag - Tagging the request.
 * @param {string} subtag - Tagging the sub request.
 * @param {string} endpoint - The endpoint to append to the URL.   
 * @param {function} getFunction - The get function to use.
 * @returns {void}
 */
export function getContentChain(dialogId, paramsWithToken, tag, subtag, endpoint, getFunction = getEU) {
    const listParams = {
        ...paramsWithToken,
        tags: { ...paramsWithToken.tags, name: tag }
    };
    let d = getUrl('dialogs/' + dialogId + endpoint, listParams, getFunction);
    let json = d.json();
    if (json.length > 0) {
        const detailParams = {
            ...paramsWithToken,
            tags: { ...paramsWithToken.tags, name: subtag }
        };
        getUrl('dialogs/' + dialogId + endpoint + randomItem(json).id, detailParams, getFunction);
    }
}

/**
 * Performs a GET request to the specified URL with the provided parameters.
 * @param {string} url - The URL to send the GET request to.
 * @param {Object} paramsWithToken - The parameters with token.
 * @param {function} getFunction - The get function to use.
 * @returns {Object} The response object.
 */
export function getUrl(url, paramsWithToken, getFunction = getEU) {
    let r = getFunction(url, paramsWithToken); 
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
    let traceparent = uuidv4();
    let paramsWithToken = {
        headers: {
            Authorization: "Bearer " + enduser.token,
            traceparent: traceparent,
            'User-Agent': 'dialogporten-k6-graphql-search'
        },
        tags: { name: 'graphql search', traceparent: traceparent }
    };
    describe('Perform graphql dialog list', () => {
        let r = postGQ(getGraphqlParty(enduser.ssn), paramsWithToken);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
    });
}

/**
 * Performs a serviceowner search.
 * @param {P} serviceowner 
 * @param {*} enduser
 * @param {*} tag_name 
 */
export function serviceownerSearch(serviceowner, enduser, tag_name, doSubqueries = true) {
    let traceparent = uuidv4();
    let paramsWithToken = {
        headers: {
            Authorization: "Bearer " + serviceowner.token,
            traceparent: traceparent
        },
        tags: { name: tag_name, traceparent: traceparent, enduser: enduser.ssn }
    }

    let enduserid = encodeURIComponent(`urn:altinn:person:identifier-no:${enduser.ssn}`);
    let serviceResource = encodeURIComponent(`urn:altinn:resource:${serviceowner.resource}`);
    let defaultFilter = `?enduserid=${enduserid}&serviceResource=${serviceResource}`;
    describe('Perform serviceowner dialog list', () => {
        let r = getSO('dialogs' + defaultFilter, paramsWithToken);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        if (doSubqueries) {
            retrieveDialogContent(r, paramsWithToken, getSO);
        }
        return r
    });
}
