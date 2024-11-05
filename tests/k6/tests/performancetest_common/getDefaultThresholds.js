/**
 * Creates default thresholds configuration for K6 tests.
 * @param {string[]} counters - Array of counter names
 * @param {string[]} labels - Array of label names
 * @returns {Object} Threshold configuration object
 * @throws {Error} If inputs are invalid
 */


export function getDefaultThresholds(counters, labels) {
    if (!Array.isArray(counters) || !Array.isArray(labels)) {
        throw new Error('Both counters and labels must be arrays');
    }
    let thresholds = {
        http_req_failed: ['rate<0.01']
    };
    for (const counter of counters) {
        for (const label of labels) {
            thresholds[`${counter}{name:${label}}`] = [];
        }
    }
    return thresholds;
}
