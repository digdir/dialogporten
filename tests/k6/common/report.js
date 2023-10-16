export { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

let replacements = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    "'": '&#39;',
    '"': '&quot;',
};

function escapeHTML(str) {
    return str.replace(/[&<>'"]/g, function (char) {
        return replacements[char];
    });
}

/**
 * Generate a junit xml string from the summary of a k6 run considering each checks as a test case
 * @param {*} data
 * @param {String} suiteName Name of the test ex., filename
 * @returns junit xml string
 */
export function generateJUnitXML(data, suiteName) {
    var failures = 0;
    var cases = [];
    var time = (data.state.testRunDurationMs) / 1000;
    var checks = [];
    if (data.root_group.checks.length > 0) {
        checks = data.root_group.checks;
    } else if (data.root_group.hasOwnProperty('groups') && data.root_group.groups.length > 0) {
        var groups = data.root_group.groups;
        groups.forEach((group) => {
            if (group.groups.length > 0) {
                var subGroups = group.groups;
                subGroups.forEach((subGroup) => {
                    subGroup.checks.forEach((check) => {
                        checks.push(check);
                    });
                });
            } else {
                group.checks.forEach((check) => {
                    checks.push(check);
                });
            }
        });
    }
    checks.forEach((check) => {
        if (check.passes >= 1 && check.fails === 0) {
            cases.push(`<testcase name="${escapeHTML(check.name)}"/>`);
        } else {
            failures++;
            cases.push(`<testcase name="${escapeHTML(check.name)}" time="${time / checks.length}"><failure message="failed"/></testcase>`);
        }
    });
    return (
        `<?xml version="1.0" encoding="UTF-8" ?>\n` +
        `<testsuites>\n` +
        `<testsuite name="${escapeHTML(suiteName)}" tests="${cases.length}" failures="${failures}">\n` +
        `${cases.join('\n')}\n</testsuite>\n</testsuites>`
    );
}
