import { default as enduserTests } from './enduser/all-tests.js';
import { default as serviceOwnerTests } from './serviceowner/all-tests.js';

export function runAllTests() {
    enduserTests();
    serviceOwnerTests();
};
