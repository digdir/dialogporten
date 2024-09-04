import { runAllTests } from "../tests/all-tests.js";
import { default as summary } from "../common/summary.js";
import { chai, describe } from '../common/testimports.js'


export let options = {};

export default function () {
    try {
        runAllTests();
    } catch (error) {
        describe('Exception during test suite', () => {
            // disable truncating so we can display the entire stack trace
            chai.config.truncateThreshold = 0;
            chai.assert(true == false, error.stack);
        });
    }
}

export function handleSummary(data) {
    return summary(data);
}
