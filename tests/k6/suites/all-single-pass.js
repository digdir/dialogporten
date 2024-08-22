import { runAllTests } from "../tests/all-tests.js";
import { default as summary } from "../common/summary.js";
import { chai, describe } from '../common/testimports.js'


export let options = {};

export default function () {
    try {
        runAllTests();
    } catch (error) {
        describe('Exception during test suite', () => {
            console.log(error.stack);
            chai.assert(true == false, 'See the full stack trace at the top of the log');
        });
    }
}

export function handleSummary(data) {
    return summary(data);
}
