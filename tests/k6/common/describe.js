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
          throw error;
        }
        let errmsg = `${name} failed, ${error.message}`;
        if (error.expected) {
            errmsg += ` expected:${error.expected} actual:${error.actual}`;
        }
        console.error(errmsg);
        success = false;
      }
    });
  
    return success;
  }