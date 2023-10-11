import { runAllTests } from "../tests/all-tests.js";

export let options = {
    vus: 10,  // Number of virtual users
    duration: '20s',  // Test duration
};

export default function () { runAllTests(); }