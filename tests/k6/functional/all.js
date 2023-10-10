import { default as serviceOwnerTests } from './serviceowner/all.js';
import { default as enduserTests } from './enduser/all.js';

export default function () {
    serviceOwnerTests();
    enduserTests();
};