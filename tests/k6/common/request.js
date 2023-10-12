import { default as http } from 'k6/http';
import { baseUrlEndUser, baseUrlServiceOwner } from './config.js'
import { getServiceOwnerTokenFromGenerator, getEnduserTokenFromGenerator } from './token.js'
import { extend } from './extend.js'

function getServiceOwnerRequestParams(params = null) {
    let defaultParams = {
        headers: {
            'Accept': 'application/json',
            'User-Agent': 'dialogporten-k6',
            'Authorization': 'Bearer ' + getServiceOwnerTokenFromGenerator()
        },
        httpDebug: 'full'
    }

    return extend(true, {}, defaultParams, params);
}

function getEnduserRequestParams(params = null) {
    let defaultParams = {
        headers: {
            'Accept': 'application/json',
            'User-Agent': 'dialogporten-k6',
            'Authorization': 'Bearer ' + getEnduserTokenFromGenerator()
        },
        httpDebug: 'full'
    }

    return extend(true, {}, defaultParams, params);
}

export function getSO(url, params = null) {
    return http.get(baseUrlServiceOwner + url, getServiceOwnerRequestParams(params))
}

export function postSO(url, body, params = null) {
    params = extend(true, {}, params, { headers: { 'Content-Type': 'application/json' }});
    return http.post(baseUrlServiceOwner + url, body, getServiceOwnerRequestParams(params));
}

export function putSO(url, body, params = null) {
    params = extend(true, {}, params, { headers: { 'Content-Type': 'application/json' }});
    return http.put(baseUrlServiceOwner + url, body, getServiceOwnerRequestParams(params));
}

export function patchSO(url, body, params = null) {
    params = extend(true, {}, params, { headers: { 'Content-Type': 'application/json' }});
    return http.patch(baseUrlServiceOwner + url, body, getServiceOwnerRequestParams(params));
}

export function deleteSO(url, params = null) {
    return http.request('DELETE', baseUrlServiceOwner + url, getServiceOwnerRequestParams(params));
}

export function getEU(url, params = null) {
    return http.get(baseUrlEndUser + url, getEnduserRequestParams(params))
}

export function postEU(url, body, params = null) {
    params = extend(true, {}, params, { headers: { 'Content-Type': 'application/json' }});
    return http.post(baseUrlEndUser + url, body, getEnduserRequestParams(params));
}

export function putEU(url, body, params = null) {
    params = extend(true, {}, params, { headers: { 'Content-Type': 'application/json' }});
    return http.put(baseUrlEndUser + url, body, getEnduserRequestParams(params));
}

export function patchEU(url, body, params = null) {
    params = extend(true, {}, params, { headers: { 'Content-Type': 'application/json' }});
    return http.patch(baseUrlEndUser + url, body, getEnduserRequestParams(params));
}

export function deleteEU(url, params = null) {
    return http.request('DELETE', baseUrlEndUser + url, getEnduserRequestParams(params));
}
