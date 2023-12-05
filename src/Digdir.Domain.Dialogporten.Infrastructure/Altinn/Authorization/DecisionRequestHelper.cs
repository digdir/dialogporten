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
        var actions = CreateActionCategories(request.AuthorizationAttributesByActions, out var actionNameIdMap);
        var resources = CreateResourceCategories(request.ServiceResource, request.DialogId, request.Party, request.AuthorizationAttributesByActions, out var resourceNameIdMap);

        var multiRequests = CreateMultiRequests(actionNameIdMap, resourceNameIdMap, request.AuthorizationAttributesByActions);

        var xacmlJsonRequest = new XacmlJsonRequest()
        {
            AccessSubject = accessSubject,
            Action = actions,
            Resource = resources,
            MultiRequests = multiRequests,
        };

        return new XacmlJsonRequestRoot { Request = xacmlJsonRequest };
    }

    public static DialogDetailsAuthorizationResult CreateDialogDetailsResponse(XacmlJsonRequestRoot xamlJsonRequestRoot, XacmlJsonResponse? xamlJsonResponse)
    {
        var response = new DialogDetailsAuthorizationResult
        {
            AuthorizationAttributesByAuthorizedActions = new Dictionary<string, List<string>>()
        };

        if (xamlJsonResponse?.Response == null)
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

                if (!response.AuthorizationAttributesByAuthorizedActions.TryGetValue(actionName, out var authorizationAttributes))
                {
                    authorizationAttributes = new List<string>();
                    response.AuthorizationAttributesByAuthorizedActions.Add(actionName, authorizationAttributes);
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

    private static List<XacmlJsonCategory> CreateAccessSubjectCategory(IEnumerable<Claim> claims)
    {
        var attributes = claims
            .Select(x => x switch
            {
                { Type: PidClaimType } => new XacmlJsonAttribute { AttributeId = AttributeIdSsn, Value = x.Value },
                { Type: var type } when type.StartsWith(AltinnUrnNsPrefix, StringComparison.Ordinal) => new() { AttributeId = type, Value = x.Value },
                { Type: ConsumerClaimType } when x.TryGetOrgNumber(out var organizationNumber) => new() { AttributeId = AttributeIdOrganizationNumber, Value = organizationNumber },
                _ => null
            })
            .Where(x => x is not null)
            .Cast<XacmlJsonAttribute>()
            .ToList();

        if (attributes.Any(x => x.AttributeId == AttributeIdSsn))
        {
            attributes.RemoveAll(x => x.AttributeId == AttributeIdOrganizationNumber);
        }

        return new() { new() { Id = SubjectId, Attribute = attributes } };
    }

    private static List<XacmlJsonCategory> CreateActionCategories(
        Dictionary<string, List<string>> actions,
        out Dictionary<string, string> actionIdByName)
    {
        var actionCategories = actions
            .Select(x => x.Key)
            .Select((action, index) => new XacmlJsonCategory
            {
                Id = $"a{index + 1}",
                Attribute = new() { new() { AttributeId = AttributeIdAction, Value = action } }
            })
            .ToList();
        actionIdByName = actionCategories.ToDictionary(x => x.Attribute.First().Value, x => x.Id);
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
        resourceNameIdMap = Enumerable.Empty<string>()
            .Append(Constants.MainResource)
            .Concat(actions
                .SelectMany(x => x.Value)
                .Distinct()
                .Where(x => x != Constants.MainResource)
            )
            .Select((resource, index) => (resource, index))
            .ToDictionary(x => x.resource, x => $"r{x.index + 1}");

        var resources = resourceNameIdMap
            .Select(x => CreateResourceCategory(x.Value, serviceResource, dialogId, partyAttribute, x.Key))
            .ToList();

        return resources;
    }

    private static XacmlJsonCategory CreateResourceCategory(string id, string serviceResource, Guid? dialogId, XacmlJsonAttribute? partyAttribute, string? subResource = null)
    {
        var (ns, value) = SplitNsAndValue(serviceResource);
        var attributes = new List<XacmlJsonAttribute>
        {
            new() { AttributeId = ns, Value = value }
        };

        if (partyAttribute is not null)
        {
            attributes.Add(partyAttribute);

            //// TEMPORARY HACK TO ADD PARTY ID DUE TO https://github.com/Altinn/altinn-authorization/issues/600
            const string partyAttributeId = "urn:altinn:partyid";
            if (partyAttribute.Value == "310029246")
            {
                attributes.Add(new() { AttributeId = partyAttributeId, Value = "51526960" });
            }
            else if (partyAttribute.Value == "15876497724")
            {
                attributes.Add(new() { AttributeId = partyAttributeId, Value = "50888718" });
            }
            //// END TEMPORARY HACK TO ADD PARTY ID
        }

        if (dialogId is not null)
        {
            attributes.Add(new() { AttributeId = AttributeIdResourceInstance, Value = dialogId.ToString() });
        }

        if (subResource is not null)
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

    private static XacmlJsonAttribute? ExtractPartyAttribute(string party)
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
        else
        {
            return null;
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

    public static class NonScalable
    {
        // TODO!
        // This contains the helpers for the preliminary implementation which doesn't scale, and should only be used in very low volume situations
        // (such as the PoC). It is not intended for production use.
        //
        // Remove this as soon as we have implemented https://github.com/digdir/dialogporten/issues/42

        public static XacmlJsonRequestRoot CreateDialogSearchRequest(DialogSearchAuthorizationRequest request)
        {
            var requestActions = new Dictionary<string, List<string>>
            {
                { "read", new List<string> { Constants.MainResource } }
            };

            var accessSubject = CreateAccessSubjectCategory(request.ClaimsPrincipal.Claims);
            var actions = CreateActionCategories(requestActions, out _);
            var resources = CreateResourceCategoriesForSearch(request.ConstraintServiceResources, request.ConstraintParties);

            var multiRequests = CreateMultiRequestsForSearch(resources);

            var xacmlJsonRequest = new XacmlJsonRequest()
            {
                AccessSubject = accessSubject,
                Action = actions,
                Resource = resources,
                MultiRequests = multiRequests,
            };

            return new XacmlJsonRequestRoot { Request = xacmlJsonRequest };
        }

        public static DialogSearchAuthorizationResult CreateDialogSearchResponse(
            XacmlJsonRequestRoot xamlJsonRequestRoot, XacmlJsonResponse? xamlJsonResponse)
        {
            var response = new DialogSearchAuthorizationResult();

            if (xamlJsonResponse?.Response == null)
            {
                return response;
            }

            try
            {
                for (var i = 0; i < xamlJsonRequestRoot.Request.MultiRequests.RequestReference.Count; i++)
                {
                    if (xamlJsonResponse.Response[i].Decision != PermitResponse)
                    {
                        continue;
                    }

                    // Get the name of the resource.
                    var resourceId = $"r{i + 1}";
                    var serviceResource = "urn:altinn:resource:" + xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                            .First(a => a.AttributeId == "urn:altinn:resource").Value;

                    string party;
                    var partyOrgNr = xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                            .FirstOrDefault(a => a.AttributeId == "urn:altinn:organizationnumber");
                    if (partyOrgNr != null)
                    {
                        party = "/org/" + partyOrgNr.Value;
                    }
                    else
                    {
                        var partySsn = xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                            .First(a => a.AttributeId == "urn:altinn:ssn");
                        party = "/person/" + partySsn.Value;
                    }

                    if (!response.PartiesByResources.TryGetValue(serviceResource, out var parties))
                    {
                        parties = new List<string>();
                        response.PartiesByResources.Add(serviceResource, parties);
                    }

                    parties.Add(party);

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

        private static List<XacmlJsonCategory> CreateResourceCategoriesForSearch(
            List<string> serviceResources,
            List<string> parties)
        {
            var resources = new List<XacmlJsonCategory>();
            var resourceCounter = 0;
            foreach (var party in parties)
            {
                var partyAttribute = ExtractPartyAttribute(party);

                foreach (var serviceResource in serviceResources)
                {
                    var rid = $"r{++resourceCounter}";
                    resources.Add(CreateResourceCategory(rid, serviceResource, null, partyAttribute));
                }
            }

            return resources;
        }

        private static XacmlJsonMultiRequests CreateMultiRequestsForSearch(
            List<XacmlJsonCategory> resources)
        {
            var multiRequests = new XacmlJsonMultiRequests
            {
                RequestReference = new List<XacmlJsonRequestReference>()
            };

            for (var i = 1; i <= resources.Count; i++)
            {
                multiRequests.RequestReference.Add(new XacmlJsonRequestReference
                {
                    ReferenceId = new List<string> { SubjectId, $"r{i}", "a1" }
                });
            }

            return multiRequests;
        }
    }
}
