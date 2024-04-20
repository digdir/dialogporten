import { generateJUnitXML, textSummary } from "./report.js";

export default function (data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }) + "\n",
        //'summary.txt': textSummary(data, { indent: ' ', enableColors: false }) + "\n",
        //'summary.json': JSON.stringify(data),
        'junit.xml': generateJUnitXML(data)
    };
}