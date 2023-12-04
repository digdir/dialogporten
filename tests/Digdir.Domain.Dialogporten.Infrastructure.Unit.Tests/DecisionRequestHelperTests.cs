using System.Security.Claims;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Features.V1.Authorization;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;
using Xunit;

namespace Digdir.Domain.Dialogporten.Infrastructure.Unit.Tests;

public class DecisionRequestHelperTests
{
    private const string ConsumerClaimValue = "{\"authority\":\"iso6523-actorid-upis\",\"ID\":\"0192:991825827\"}";

    [Fact]
    public void CreateDialogDetailsRequestShouldReturnCorrectRequest()
    {
        // Arrange
        var request = CreateDialogDetailsAuthorizationRequest(
            GetAsClaims(
                ("pid", "12345678901"),

                // This should not be copied as subject claim since there's a "pid"-claim
                ("consumer", ConsumerClaimValue)
            ),
            "/org/912345678");
        var dialogId = request.DialogId;

        // Act
        var result = DecisionRequestHelper.CreateDialogDetailsRequest(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Request);
        Assert.NotNull(result.Request.Resource);
        Assert.NotNull(result.Request.Action);
        Assert.NotNull(result.Request.AccessSubject);
        Assert.NotNull(result.Request.MultiRequests);

        // Check AccessSubject attributes
        var accessSubject = result.Request.AccessSubject.First();
        Assert.Equal("s1", accessSubject.Id);
        Assert.Contains(accessSubject.Attribute, a => a.AttributeId == "urn:altinn:foo" && a.Value == "bar");
        Assert.Contains(accessSubject.Attribute, a => a.AttributeId == "urn:altinn:ssn" && a.Value == "12345678901");
        Assert.DoesNotContain(accessSubject.Attribute, a => a.AttributeId == "urn:altinn:organizationnumber");

        // Check Action attributes
        Assert.Contains(result.Request.Action, a => a.Id == "a1" && a.Attribute.Any(attr => attr.AttributeId == "urn:oasis:names:tc:xacml:1.0:action:action-id" && attr.Value == "read"));
        Assert.Contains(result.Request.Action, a => a.Id == "a2" && a.Attribute.Any(attr => attr.AttributeId == "urn:oasis:names:tc:xacml:1.0:action:action-id" && attr.Value == "write"));
        Assert.Contains(result.Request.Action, a => a.Id == "a3" && a.Attribute.Any(attr => attr.AttributeId == "urn:oasis:names:tc:xacml:1.0:action:action-id" && attr.Value == "sign"));
        Assert.Contains(result.Request.Action, a => a.Id == "a4" && a.Attribute.Any(attr => attr.AttributeId == "urn:oasis:names:tc:xacml:1.0:action:action-id" && attr.Value == "elementread"));

        // Check Resource attributes
        var resource1 = result.Request.Resource.FirstOrDefault(r => r.Id == "r1");
        Assert.NotNull(resource1);
        Assert.Contains(resource1.Attribute, a => a.AttributeId == "urn:altinn:resource" && a.Value == "some-service");
        Assert.Contains(resource1.Attribute, a => a.AttributeId == "urn:altinn:resourceinstance" && a.Value == dialogId.ToString());
        Assert.Contains(resource1.Attribute, a => a.AttributeId == "urn:altinn:organizationnumber" && a.Value == "912345678");

        var resource2 = result.Request.Resource.FirstOrDefault(r => r.Id == "r2");
        Assert.NotNull(resource2);
        Assert.Contains(resource2.Attribute, a => a.AttributeId == "urn:altinn:subresource" && a.Value == "element1");

        var resource3 = result.Request.Resource.FirstOrDefault(r => r.Id == "r3");
        Assert.NotNull(resource3);
        Assert.Contains(resource3.Attribute, a => a.AttributeId == "urn:altinn:subresource" && a.Value == "element2");

        // Check MultiRequests
        Assert.Equal(5, result.Request.MultiRequests.RequestReference.Count);
        Assert.Contains(result.Request.MultiRequests.RequestReference, rr => ContainsSameElements(rr.ReferenceId, new List<string> { "s1", "r1", "a1" }));
        Assert.Contains(result.Request.MultiRequests.RequestReference, rr => ContainsSameElements(rr.ReferenceId, new List<string> { "s1", "r1", "a2" }));
        Assert.Contains(result.Request.MultiRequests.RequestReference, rr => ContainsSameElements(rr.ReferenceId, new List<string> { "s1", "r2", "a3" }));
        Assert.Contains(result.Request.MultiRequests.RequestReference, rr => ContainsSameElements(rr.ReferenceId, new List<string> { "s1", "r3", "a4" }));
        Assert.Contains(result.Request.MultiRequests.RequestReference, rr => ContainsSameElements(rr.ReferenceId, new List<string> { "s1", "r4", "a4" }));
    }

    [Fact]
    public void CreateDialogDetailsRequestShouldReturnCorrectRequestForConsumerOrgAndPersonParty()
    {
        // Arrange
        var request = CreateDialogDetailsAuthorizationRequest(
            GetAsClaims(
                // Should be copied as subject claim since there's not a "pid"-claim
                ("consumer", ConsumerClaimValue)
            ),
            "/person/12345678901");

        // Act
        var result = DecisionRequestHelper.CreateDialogDetailsRequest(request);

        // Assert
        // Check that we have the organizationnumber
        var accessSubject = result.Request.AccessSubject.First();
        Assert.Contains(accessSubject.Attribute, a => a.AttributeId == "urn:altinn:organizationnumber" && a.Value == "991825827");

        // Check that we have the ssn attribute as resource owner
        var resource1 = result.Request.Resource.FirstOrDefault(r => r.Id == "r1");
        Assert.NotNull(resource1);
        Assert.Contains(resource1.Attribute, a => a.AttributeId == "urn:altinn:ssn" && a.Value == "12345678901");
    }

    [Fact]
    public void CreateDialogDetailsResponseShouldReturnCorrectResponse()
    {
        // Arrange
        var request = CreateDialogDetailsAuthorizationRequest(
            GetAsClaims(
                // Should be copied as subject claim since there's not a "pid"-claim
                ("consumer", ConsumerClaimValue)
            ),
            "/person/12345678901");

        // Add an additional action to the request that the mocked response should give a non-permit response for
        request.Actions.Add("failaction", new List<string> { Constants.MainResource });

        var jsonRequestRoot = DecisionRequestHelper.CreateDialogDetailsRequest(request);
        var jsonResponse = CreateMockedXamlJsonResponse(jsonRequestRoot);

        // Act
        var response = DecisionRequestHelper.CreateDialogDetailsResponse(jsonRequestRoot, jsonResponse);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(request.Actions.Count - 1, response.AuthorizedActions.Count);
        Assert.Equal(Constants.MainResource, response.AuthorizedActions["read"].First());
        Assert.Equal(Constants.MainResource, response.AuthorizedActions["write"].First());
        Assert.Equal("element1", response.AuthorizedActions["sign"].First());
        Assert.Equal("element2", response.AuthorizedActions["elementread"].First());
        Assert.Equal("element3", response.AuthorizedActions["elementread"].Last());
        Assert.DoesNotContain(response.AuthorizedActions.Keys, k => k == "failaction");
    }

    private static DialogDetailsAuthorizationRequest CreateDialogDetailsAuthorizationRequest(List<Claim> principalClaims, string party)
    {
        var allClaims = new List<Claim>
        {
            new("urn:altinn:foo", "bar")
        };
        allClaims.AddRange(principalClaims);
        return new DialogDetailsAuthorizationRequest
        {
            ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(allClaims, "test")),
            ServiceResource = "urn:altinn:resource:some-service",
            DialogId = Guid.NewGuid(),

            // This should be copied resources with attributes "urn:altinn:organizationnumber" if starting with "/org/"
            // and "urn:altinn:ssn" if starting with "/person/"
            Party = party,
            Actions = new Dictionary<string, List<string>>
            {
                { "read", new List<string> { Constants.MainResource } },
                { "write", new List<string> { Constants.MainResource } },
                { "sign", new List<string> { "element1" } },
                { "elementread", new List<string> { "element2", "element3" } }
            }
        };
    }

    private static XacmlJsonResponse CreateMockedXamlJsonResponse(XacmlJsonRequestRoot request)
    {
        var response = new XacmlJsonResponse
        {
            Response = new List<XacmlJsonResult>()
        };

        foreach (var requestReference in request.Request.MultiRequests.RequestReference)
        {
            // Check if this request reference refers to the action with name "failaction", in which case we should return a non-permit response
            // We need to use the actionId since the action name is not included in the request reference
            var actionId = requestReference.ReferenceId.First(x => x.StartsWith('a'));
            var actionName = request.Request.Action.First(a => a.Id == actionId).Attribute.First().Value;

            var decision = actionName == "failaction" ? "Deny" : "Permit";

            response.Response.Add(new XacmlJsonResult
            {
                Decision = decision
            });
        }

        return response;
    }

    private static List<Claim> GetAsClaims(params (string, string)[] claims)
    {
        return claims.Select(c => new Claim(c.Item1, c.Item2)).ToList();
    }

    private static bool ContainsSameElements(IEnumerable<string> collection, IEnumerable<string> expectedElements)
    {
        return expectedElements.All(expected => collection.Contains(expected)) && collection.Count() == expectedElements.Count();
    }
}
