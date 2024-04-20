export { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';

export function generateJUnitXML(k6Json) {
    const xmlDoc = [];
    xmlDoc.push('<?xml version="1.0" encoding="UTF-8" ?>');
    xmlDoc.push('<testsuites>');

    function processGroup(group) {
        if (group.name) { // skip root group
            xmlDoc.push(`<testsuite name="${group.name}" tests="${group.checks.length}">`);

            group.checks.forEach(check => {
                let failed = check.fails > 0 ;
                xmlDoc.push(`<testcase classname="${group.name}" name="${check.name}">`);
                if (failed) {
                    xmlDoc.push(`<failure message="Check failed. See output K6 task for more details.">${check.name}</failure>`);
                }
                xmlDoc.push('</testcase>');
            });
        }

        group.groups.forEach(subGroup => {
            processGroup(subGroup);
        });

        xmlDoc.push('</testsuite>');
    }

    processGroup(k6Json.root_group);

    xmlDoc.push('</testsuites>');
    return xmlDoc.join('\n');
}
