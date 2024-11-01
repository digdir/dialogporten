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

export function getDefaultThresholds(counters, labels) {
    let thresholds = {
        http_req_failed: ['rate<0.01']
    }
    counters.flatMap(t => labels.map(l => thresholds[`${t}{name:${l}}`] = []));
    return thresholds;
}
