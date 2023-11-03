import { group } from 'k6';

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
        console.warn(errmsg);
      }
    });
  
    return success;
  }