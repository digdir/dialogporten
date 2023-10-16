import { group } from 'k6';
import { isErrorLoggingEnabled, logError } from './logging.js';

export function describe(name, fn) {
    let success = true;
  
    group(name, () => {
      try {
        fn();
        success = true;
      } 
      catch (error) {        
        if (error.name !== 'AssertionError') {
          // Goja (the JS engine used by K6) seems to clobber the stack when rethrowing exceptions
          console.error(error.stack);
          throw error;
        }
        let errmsg = `${name} failed, ${error.message}`;
        if (error.expected) {
            errmsg += ` expected:${error.expected} actual:${error.actual}`;
        }

        // For functional tests, we want to be able to associate the error message with the failed test. 
        if (isErrorLoggingEnabled) {
          console.warn(name);
          logError(name, errmsg);
        }
      }
    });
  
    return success;
  }