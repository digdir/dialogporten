import { default as run, setup as testSetup } from "../tests/serviceowner/performance/createremove-no-delay.js";
import { default as summary } from "../common/summary.js";
export let options = {
    vus: 10,  // Number of virtual users
    duration: '1m',  // Test duration
};

export function setup() {
    return testSetup();
}

export default function (data) { run(data); }

export function handleSummary(data) {
    return summary(data);
}
