import { runAllTests } from "../tests/all-tests.js";
import { generateJUnitXML, textSummary } from "../common/report.js";
import { enableErrorLogging } from "../common/logging.js";

export let options = {};

enableErrorLogging();

export default function () { runAllTests(); }

export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }) + "\n",
        'summary.json': JSON.stringify(data),
        'junit.xml': generateJUnitXML(data, 'dialogporten')
    };
}