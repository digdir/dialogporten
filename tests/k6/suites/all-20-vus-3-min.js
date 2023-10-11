import { runAllTests } from "../tests/all-tests.js";

export let options = {
    vus: 20,  // Number of virtual users
    duration: '3m',  // Test duration
};

export default function () { runAllTests(); }