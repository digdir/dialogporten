import { SharedArray } from 'k6/data';
import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';

export function getEndusers(filenameEndusers) {

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
    return endUsers;
}

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
    }
    for (const counter of counters) {
        for (const label of labels) {
            thresholds[`${counter}{name:${label}}`] = [];
        }
    }
    return thresholds;
}
