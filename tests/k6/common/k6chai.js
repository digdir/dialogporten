
import { default as chai, expect } from 'https://jslib.k6.io/k6chaijs/4.3.4.3/index.js';

function expectStatusFor(response) {
    return {
        to: {
            equal(expectedStatus) {

                try {
                    expect(response.status, "response status").to.equal(expectedStatus);
                }
                catch (e) {
                    let errorDetails = "";
                    try {
                        if (response.body) {
                            let body = response.json();
                            if (body["errors"]) {
                                errorDetails = ", errorDetails: " + JSON.stringify(body["errors"]);
                            }
                        }
                    } catch (e) {};

                    e.message += errorDetails;
                    throw e;
                }
            }
        },
    };
}


chai.use(function(chai, utils) {
    chai.Assertion.addMethod('hasLocalizedText', function(expectedValue, cultureCode) {
      const obj = this._obj; // current object under assertion
  
      // Ensure the current object is an array (i.e., the 'name' property in your case)
      new chai.Assertion(obj).to.be.an('array');
  
      let foundItem;
      if (cultureCode) {
        // Check if an item with the specified cultureCode and value exists
        foundItem = obj.find(item => item.cultureCode.toLowerCase() === cultureCode.toLowerCase() && item.value === expectedValue);
      } else {
        // Check if any item with the specified value exists
        foundItem = obj.find(item => item.value === expectedValue);
      }
  
      // Assertion
      this.assert(
        foundItem !== undefined,
        `expected #{this} to have a localized text of ${expectedValue}${cultureCode ? ` with culture code ${cultureCode}` : ''}`,
        `expected #{this} not to have a localized text of ${expectedValue}${cultureCode ? ` with culture code ${cultureCode}` : ''}`
      );
    });
  });

export { chai, expect, expectStatusFor }