import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

import { getDefaultThresholds } from '../../performancetest_common/getDefaultThresholds.js';
import { createDialog } from '../../performancetest_common/createDialog.js';
import { serviceOwners, endUsers } from '../../performancetest_common/readTestdata.js';

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'p(99.5)', 'p(99.9)', 'count'],
    thresholds: getDefaultThresholds(['http_req_duration', 'http_reqs'],['create dialog'])
};

export default function() {
    if ((options.vus === undefined || options.vus === 1) && (options.iterations === undefined || options.iterations === 1)) {
      createDialog(serviceOwners[0], endUsers[0]);
    }
    else {
        createDialog(randomItem(serviceOwners), randomItem(endUsers));
    }
  }

