import { runAllTests } from "../tests/all-tests.js";
import { default as summary } from "../common/summary.js";


export let options = {};

export default function () { runAllTests(); }

export function handleSummary(data) {
    return summary(data);
}
