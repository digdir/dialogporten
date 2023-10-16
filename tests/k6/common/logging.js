export var __ERRORS_BY_TAG = {};
export var __ERROR_LOGGING_ENABLED = false;

export function isErrorLoggingEnabled() {
    return __ERROR_LOGGING_ENABLED;
}

export function enableErrorLogging() {
    __ERROR_LOGGING_ENABLED = true;
}

export function logError(tag, errMsg) {
    console.warn(tag);
    __ERRORS_BY_TAG[tag] = errMsg;
}
 export function getError(tag) {
    return __ERRORS_BY_TAG[tag];
 }

 export function getAllErrors() {
    return __ERRORS_BY_TAG;
 }