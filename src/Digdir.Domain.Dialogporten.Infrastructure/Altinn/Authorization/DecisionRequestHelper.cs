using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal static class DecisionRequestHelper
{
    private const string SubjectId = "s1";
    private const string MainResourceId = "r1";
    private const string AltinnUrnNsPrefix = "urn:altinn:";
    private const string PidClaimType = "pid";
    private const string ConsumerClaimType = "consumer";
    private const string PartyPrefixOrg = "/org/";
    private const string PartyPrefixPerson = "/person/";
    private const string AttributeIdSsn = "urn:altinn:ssn";
    private const string AttributeIdOrganizationNumber = "urn:altinn:organizationnumber";
    private const string AttributeIdAction = "urn:oasis:names:tc:xacml:1.0:action:action-id";
    private const string AttributeIdResourceInstance = "urn:altinn:resourceinstance";
    private const string AttributeIdSubResource = "urn:altinn:subresource";
    private const string PermitResponse = "Permit";

    public static XacmlJsonRequestRoot CreateDialogDetailsRequest(DialogDetailsAuthorizationRequest request)
    {
        var accessSubject = CreateAccessSubjectCategory(request.ClaimsPrincipal.Claims);
        var actions = CreateActionCategories(request.Actions, out var actionNameIdMap);
        var resources = CreateResourceCategories(request.ServiceResource, request.DialogId, request.Party, request.Actions, out var resourceNameIdMap);

        var multiRequests = CreateMultiRequests(actionNameIdMap, resourceNameIdMap, request.Actions);

        var xacmlJsonRequest = new XacmlJsonRequest()
        {
            AccessSubject = accessSubject,
            Action = actions,
            Resource = resources,
            MultiRequests = multiRequests,
        };

        return new XacmlJsonRequestRoot { Request = xacmlJsonRequest };
    }

    private static List<XacmlJsonCategory> CreateAccessSubjectCategory(IEnumerable<Claim> claims)
    {
        var attributes = new List<XacmlJsonAttribute>();

        var hasPidClaimType = false;
        foreach (var claim in claims)
        {
            if (claim.Type.StartsWith(AltinnUrnNsPrefix, StringComparison.Ordinal))
            {
                attributes.Add(new XacmlJsonAttribute
                {
                    AttributeId = claim.Type,
                    Value = claim.Value
                });
            }
            else if (claim.Type == PidClaimType)
            {
                attributes.Add(new XacmlJsonAttribute
                {
                    AttributeId = AttributeIdSsn,
                    Value = claim.Value
                });

                hasPidClaimType = true;
            }
            else if (claim.Type == ConsumerClaimType)
            {
                // If we have a pid claim type (ie. ID-porten), the consumer claim is not relevant for authorization
                if (hasPidClaimType) continue;

                if (claim.TryGetOrgNumber(out var organizationNumber))
                {
                    attributes.Add(new XacmlJsonAttribute
                    {
                        AttributeId = AttributeIdOrganizationNumber,
                        Value = organizationNumber
                    });
                }
            }

            // Re-check if we parsed the consumer_org before we got to the pid
            if (hasPidClaimType)
            {
                attributes.RemoveAll(a => a.AttributeId == AttributeIdOrganizationNumber);
            }
        }

        return new List<XacmlJsonCategory>
        {
            new()
            {
                Id = SubjectId,
                Attribute = attributes
            }
        };
    }

    private static List<XacmlJsonCategory> CreateActionCategories(
        Dictionary<string, List<string>> actions,
        out Dictionary<string, string> resourceNameIdMap)
    {
        var actionCategories = new List<XacmlJsonCategory>();
        var actionCounter = 1;
        resourceNameIdMap = new Dictionary<string, string>();

        foreach (var action in actions.Keys)
        {
            var aid = $"a{actionCounter++}";
            actionCategories.Add(new XacmlJsonCategory
            {
                Id = aid,
                Attribute = new List<XacmlJsonAttribute>
                {
                    new() { AttributeId = AttributeIdAction, Value = action }
                }
            });
            resourceNameIdMap.Add(action, aid);
        }

        return actionCategories;
    }

    private static List<XacmlJsonCategory> CreateResourceCategories(
        string serviceResource,
        Guid dialogId,
        string party,
        Dictionary<string, List<string>> actions,
        out Dictionary<string, string> resourceNameIdMap)
    {
        var partyAttribute = ExtractPartyAttribute(party);
        var resources = new List<XacmlJsonCategory>
        {
            CreateResourceCategory(MainResourceId, serviceResource, dialogId, partyAttribute, Constants.MainResource)
        };

        resourceNameIdMap = new Dictionary<string, string>
        {
            { Constants.MainResource, MainResourceId }
        };

        var resourceCounter = 2;
        var subResources = actions.SelectMany(a => a.Value)
                                   .Distinct()
                                   .Where(r => r != Constants.MainResource);

        foreach (var subResource in subResources)
        {
            var rid = $"r{resourceCounter++}";
            resources.Add(CreateResourceCategory(rid, serviceResource, dialogId, partyAttribute, subResource));
            resourceNameIdMap.Add(subResource, rid);
        }

        return resources;
    }

    private static XacmlJsonCategory CreateResourceCategory(string id, string serviceResource, Guid dialogId, XacmlJsonAttribute partyAttribute, string subResource)
    {
        var (ns, value) = SplitNsAndValue(serviceResource);
        var attributes = new List<XacmlJsonAttribute>
        {
            new() { AttributeId = ns, Value = value },
            new() { AttributeId = AttributeIdResourceInstance, Value = dialogId.ToString() },
            partyAttribute
        };

        if (subResource != Constants.MainResource)
        {
            attributes.Add(new XacmlJsonAttribute { AttributeId = AttributeIdSubResource, Value = subResource });
        }

        return new XacmlJsonCategory
        {
            Id = id,
            Attribute = attributes
        };
    }

    private static (string, string) SplitNsAndValue(string serviceResource)
    {
        var lastColonIndex = serviceResource.LastIndexOf(':');

        var ns = serviceResource[..lastColonIndex];
        var value = serviceResource[(lastColonIndex + 1)..];

        return (ns, value);
    }

    private static XacmlJsonAttribute ExtractPartyAttribute(string party)
    {
        var partyAttribute = new XacmlJsonAttribute();

        if (party.StartsWith(PartyPrefixOrg, StringComparison.Ordinal))
        {
            partyAttribute.AttributeId = AttributeIdOrganizationNumber;
            partyAttribute.Value = party[PartyPrefixOrg.Length..];

        }
        else if (party.StartsWith(PartyPrefixPerson, StringComparison.Ordinal))
        {
            partyAttribute.AttributeId = AttributeIdSsn;
            partyAttribute.Value = party[PartyPrefixPerson.Length..];
        }

        return partyAttribute;
    }

    private static XacmlJsonMultiRequests CreateMultiRequests(
        Dictionary<string, string> actionNameIdMap,
        Dictionary<string, string> resourceNameIdMap,
        Dictionary<string, List<string>> requestActions)
    {
        var multiRequests = new XacmlJsonMultiRequests
        {
            RequestReference = new List<XacmlJsonRequestReference>()
        };

        foreach (var (actionName, actionId) in actionNameIdMap)
        {
            var relevantResources = requestActions[actionName];

            foreach (var resourceName in relevantResources)
            {
                multiRequests.RequestReference.Add(new XacmlJsonRequestReference
                {
                    ReferenceId = new List<string> { SubjectId, resourceNameIdMap[resourceName], actionId }
                });
            }
        }

        return multiRequests;
    }

    public static DialogDetailsAuthorizationResult CreateDialogDetailsResponse(XacmlJsonRequestRoot xamlJsonRequestRoot, XacmlJsonResponse? xamlJsonResponse)
    {
        var response = new DialogDetailsAuthorizationResult
        {
            AuthorizedActions = new Dictionary<string, List<string>>()
        };

        if (xamlJsonResponse == null)
        {
            return response;
        }

        try
        {
            // Iterate over the RequestReference to get the action and resource names asked
            // The responses match the indices of the request
            var index = -1;
            foreach (var requestReference in xamlJsonRequestRoot.Request.MultiRequests.RequestReference)
            {
                index++;
                if (xamlJsonResponse.Response[index].Decision != PermitResponse)
                {
                    continue;
                }

                // We have a permitted action, now find the action and resource names from the referenceId
                var actionId = requestReference.ReferenceId.First(x => x.StartsWith('a'));
                var actionName = xamlJsonRequestRoot.Request.Action.First(a => a.Id == actionId).Attribute
                    .First(a => a.AttributeId == AttributeIdAction).Value;

                // Get the name of the resource. If the id is not the main resource, get the subresource name
                var resourceId = requestReference.ReferenceId.First(x => x.StartsWith('r'));

                var resourceName = resourceId == MainResourceId
                    ? Constants.MainResource
                    : xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                        .First(a => a.AttributeId == AttributeIdSubResource).Value;

                if (!response.AuthorizedActions.TryGetValue(actionName, out var authorizationAttributes))
                {
                    authorizationAttributes = new List<string>();
                    response.AuthorizedActions.Add(actionName, authorizationAttributes);
                }

                authorizationAttributes.Add(resourceName);

            }
        }
        // If for some reason the response is broken, we will probably get null reference exceptions from the First()
        // calls above. In that case, we just return the empty response to deny access. Application Insights will log the exception.
        catch (Exception)
        {
            // ignored
        }

        return response;
    }
}
