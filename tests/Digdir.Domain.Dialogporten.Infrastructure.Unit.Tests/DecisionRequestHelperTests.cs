using System.Security.Claims;
using Digdir.Domain.Dialogporten.Domain.Authorization;
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
                { "read", new List<string> { DialogDetailsAuthorizationRequest.MainResource } },
                { "write", new List<string> { DialogDetailsAuthorizationRequest.MainResource } },
                { "sign", new List<string> { "element1" } },
                { "elementread", new List<string> { "element2", "element3" } }
            }
        };
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
