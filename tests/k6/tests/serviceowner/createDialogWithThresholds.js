import { default as run } from "./performance/create-dialog.js";

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    vus: 1,
    duration: "30s",
    thresholds: {
        "http_req_duration": ["p(95)<100", "p(99)<300"],    
    }
}

export default function (data) { run(data); }