import { default as run } from "./performance/enduser-search.js";

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    vus: 1,
    duration: "30s",
    thresholds: {
        "http_req_duration{name:enduser search}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get dialog}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get dialog activities}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get dialog activity}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get seenlogs}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get transmissions}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get transmission}": ["p(95)<300", "p(99)<500"],
        "http_req_duration{name:enduser get labellog}": ["p(95)<300", "p(99)<500"],
        "http_reqs{name:enduser search}": [],
        "http_reqs{name:enduser get dialog activities}": [],
        "http_reqs{name:enduser get dialog activity}": [],
        "http_reqs{name:enduser get seenlogs}": [],
        "http_reqs{name:enduser get transmissions}": [],
        "http_reqs{name:enduser get transmission}": [],
        "http_reqs{name:enduser get dialog}": [], 
        "http_reqs{name:enduser get labellog}": [], 
    }
}

export default function (data) { run(data); }

