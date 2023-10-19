import { default as run, setup as testSetup } from "../tests/serviceowner/performance/createremove-no-delay.js";
export let options = {
    vus: 10,  // Number of virtual users
    duration: '1m',  // Test duration
};

export function setup() {
    return testSetup();
}

export default function (data) { run(data); }
