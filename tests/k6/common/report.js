export { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';
import { getError, getAllErrors, __ERRORS_BY_TAG } from './logging.js';

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

    let failures = 0;
    let cases = [];
    let time = (data.state.testRunDurationMs) / 1000;
    let checks = [];
    if (data.root_group.checks.length > 0) {
        checks = data.root_group.checks;
    } else if (data.root_group.hasOwnProperty('groups') && data.root_group.groups.length > 0) {
        var groups = data.root_group.groups;
        groups.forEach((group) => {
            if (group.groups.length > 0) {
                var subGroups = group.groups;
                subGroups.forEach((subGroup) => {
                    subGroup.checks.forEach((check) => {
                        check
                        checks.push(check);
                    });
                });
            } else {
                group.checks.forEach((check) => {
                    check["groupName"] = group.name;
                    checks.push(check);
                });
            }
        });
    }
    checks.forEach((check) => {
        if (check.passes >= 1 && check.fails === 0) {
            cases.push(`<testcase classname="${escapeHTML(check.name)}" name="${escapeHTML(check.name)}" time="0"/>`);
        } else {
            failures++;
            console.warn(check.groupName);
            
            let errmsg = getError(check.groupName);
            cases.push(`<testcase classname="${escapeHTML(check.name)}" name="${escapeHTML(check.name)}" time="0"><failure message="${errmsg}"/></testcase>`);
        }
    });

console.warn(getAllErrors());
console.warn(__ERRORS_BY_TAG);

    return (
        `<?xml version="1.0" encoding="UTF-8" ?>\n` +
        `<testsuites>\n` +
        `<testsuite package="${escapeHTML(suiteName)}" name="${escapeHTML(suiteName)}" id="0" tests="${cases.length}" failures="${failures}" time="${time}">\n` +
        `${cases.join('\n')}\n</testsuite>\n</testsuites>`
    );
}
