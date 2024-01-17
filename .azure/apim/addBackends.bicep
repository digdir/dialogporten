param apiManagementName string
param containerAppEnvName string
param webApiSoName string
param webApiEuName string

resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvName
}

resource apim 'Microsoft.ApiManagement/service@2023-03-01-preview' existing = {
  name: apiManagementName
}

resource webApiSo 'Microsoft.App/containerApps@2023-05-01' existing = {
  name: webApiSoName
}

resource webApiEu 'Microsoft.App/containerApps@2023-05-01' existing = {
  name: webApiEuName
}

#disable-next-line no-hardcoded-env-urls
var managementBaseUrl = environment().resourceManager

var webApiSoFqdn = 'https://${webApiSoName}.${containerAppEnv.properties.defaultDomain}'
resource serviceownerBackend 'Microsoft.ApiManagement/service/backends@2022-08-01' = {
  name: 'serviceownerBackend'
  parent: apim
  properties: {
    title: 'WebAPI Service Owner'
    description: 'Backend link to serviceowner API'
    protocol: 'http'
    url: webApiSoFqdn
    resourceId: '${managementBaseUrl}${webApiSo.id}'
  }
}

var webApiEuFqdn = 'https://${webApiEuName}.${containerAppEnv.properties.defaultDomain}'
resource enduserBackend 'Microsoft.ApiManagement/service/backends@2022-08-01' = {
  name: 'enduserBackend'
  parent: apim
  properties: {
    title: 'WebAPI Enduser'
    description: 'Backend link to enduser API'
    protocol: 'http'
    url: webApiEuFqdn
    resourceId: '${managementBaseUrl}${webApiEu.id}'
  }
}

resource apimPolicy 'Microsoft.ApiManagement/service/policies@2023-03-01-preview' = {
  parent: apim
  name: 'policy'
  properties: {
    #disable-next-line prefer-interpolation
    value: concat('''
    <!-- IMPORTANT: 
      - Policy elements can appear only within the <inbound>, <outbound>, <backend> section elements.
      - Only the <forward-request> policy element can appear within the <backend> section element.
      - To apply a policy to the incoming request (before it is forwarded to the backend service), place a corresponding policy element within the <inbound> section element.
      - To apply a policy to the outgoing response (before it is sent back to the caller), place a corresponding policy element within the <outbound> section element.
      - To add a policy position the cursor at the desired insertion point and click on the round button associated with the policy.
      - To remove a policy, delete the corresponding policy statement from the policy document.
      - Policies are applied in the order of their appearance, from the top down. -->
    <policies>
      <inbound>
        <choose>
    <when condition="@(context.Request.Url.Path != null &amp;&amp; Regex.IsMatch(context.Request.Url.Path, @&quot;^api/[^/]+/enduser/&quot;) || context.Request.Url.Path.StartsWith(&quot;swagger&quot;))">
    ''',
      '<set-backend-service backend-id="${enduserBackend.name}" />',
      '''
          </when>
          <when condition="@(context.Request.Url.Path != null &amp;&amp; Regex.IsMatch(context.Request.Url.Path, @&quot;^api/[^/]+/serviceowner/&quot;))">
    ''',
      '<set-backend-service backend-id="${serviceownerBackend.name}" />',
      '''
          </when>
          <otherwise>
            <return-response>
              <set-status code="404" reason="Not found" />
              <!-- <set-body>Vurder en problem-details her?</set-body> -->
            </return-response>
          </otherwise>
        </choose>
      </inbound>
      <backend>
        <forward-request />
      </backend>
      <outbound />
      <on-error />
    </policies>
    ''')
    format: 'xml'
  }
}

resource defaultApi 'Microsoft.ApiManagement/service/apis@2023-03-01-preview' = {
  parent: apim
  name: 'default'
  properties: {
    displayName: 'default'
    apiRevision: '1'
    subscriptionRequired: false
    protocols: [
      'https'
    ]
    authenticationSettings: {
      oAuth2AuthenticationSettings: []
      openidAuthenticationSettings: []
    }
    isCurrent: true
    path: ''
  }

  resource operations 'operations' = [for operation in [ 'DELETE', 'GET', 'POST', 'PUT', 'PATCH', 'OPTIONS', 'HEAD', 'TRACE' ]: {
    name: toLower(operation)
    properties: {
      displayName: operation
      method: operation
      urlTemplate: '/*'
      templateParameters: []
      responses: []
    }
  }]
}
