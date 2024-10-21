@description('The name of the availability test')
param name string

@description('The location where the resources will be deployed')
param location string

@description('Tags to apply to resources')
param tags object

@description('The ID of the Application Insights resource')
param appInsightsId string


@description('The URL of the availability test')
param url string

resource availabilityTest 'Microsoft.Insights/webtests@2022-06-15' = {
  name: name
  location: location
  tags: union(tags, { 
    'hidden-link:${appInsightsId}': 'Resource'
  })
  kind: 'standard'
  properties: {
    Enabled: true
    SyntheticMonitorId: name
    Name: name
    Description: 'Availability test for ${name}'
    Frequency: 120 // Run test every 2 minutes
    Timeout: 60 // 1 minute timeout
    Kind: 'standard'
    RetryEnabled: true
    Locations: [
      { Id: 'emea-nl-ams-azr' } // Amsterdam
      { Id: 'emea-se-sto-edge' } // Stockholm
      { Id: 'emea-gb-db3-azr' } // Dublin
    ]
    Request: {
      RequestUrl: url
      HttpVerb: 'GET'
      ParseDependentRequests: false
    }
    ValidationRules: {
      ExpectedHttpStatusCode: 200
      SSLCheck: true
      SSLCertRemainingLifetimeCheck: 7
    }
  }
}
