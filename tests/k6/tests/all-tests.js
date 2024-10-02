import { default as serviceOwnerTests } from './serviceowner/all-tests.js';
import { default as enduserTests } from './enduser/all-tests.js';

export function runAllTests() {
    serviceOwnerTests();
    enduserTests();
};
