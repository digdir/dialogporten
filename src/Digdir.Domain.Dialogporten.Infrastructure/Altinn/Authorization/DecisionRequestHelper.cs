using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal static class DecisionRequestHelper
{
    private const string SubjectId = "s1";
    private const string AltinnUrnNsPrefix = "urn:altinn:";
    private const string PidClaimType = "pid";
    private const string ConsumerClaimType = "consumer";
    private const string PartyPrefixOrg = "/org/";
    private const string PartyPrefixPerson = "/person/";
    private const string AttributeIdSsn = "urn:altinn:ssn";
    private const string AttributeIdOrganizationNumber = "urn:altinn:organizationnumber";
    private const string AttributeIdAction = "urn:oasis:names:tc:xacml:1.0:action:action-id";
    private const string AttributeIdResource = "urn:altinn:resource";
    private const string AttributeIdResourceInstance = "urn:altinn:resourceinstance";
    private const string AttributeIdSubResource = "urn:altinn:subresource";
    private const string PermitResponse = "Permit";

    public static XacmlJsonRequestRoot CreateDialogDetailsRequest(DialogDetailsAuthorizationRequest request)
    {
        var accessSubject = CreateAccessSubjectCategory(request.ClaimsPrincipal.Claims);
        var actions = CreateActionCategories(request.AltinnActions, out var actionIdByName);
        var resources = CreateResourceCategories(request.ServiceResource, request.DialogId, request.Party, request.AltinnActions, out var resourceIdByName);

        var multiRequests = CreateMultiRequests(request.AltinnActions, actionIdByName, resourceIdByName);

        var xacmlJsonRequest = new XacmlJsonRequest()
        {
            AccessSubject = accessSubject,
            Action = actions,
            Resource = resources,
            MultiRequests = multiRequests,
        };

        return new XacmlJsonRequestRoot { Request = xacmlJsonRequest };
    }

    public static DialogDetailsAuthorizationResult CreateDialogDetailsResponse(HashSet<AltinnAction> altinnActions, XacmlJsonResponse? xamlJsonResponse) =>
        new()
        {
            AuthorizedAltinnActions = altinnActions
                .Zip(xamlJsonResponse?.Response ?? Enumerable.Empty<XacmlJsonResult>(), (action, response) => (action, response))
                .Where(x => x.response.Decision == PermitResponse)
                .Select(x => x.action)
                .ToHashSet()
        };

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
        HashSet<AltinnAction> altinnActions, out Dictionary<string, string> actionIdByName)
    {
        actionIdByName = altinnActions
            .Select(x => x.Name)
            .Distinct()
            .Select((action, index) => (action, id: $"a{index + 1}"))
            .ToDictionary(x =>
                x.action,
                x => x.id);

        return actionIdByName
            .Select(x => new XacmlJsonCategory
            {
                Id = x.Value,
                Attribute = new() { new() { AttributeId = AttributeIdAction, Value = x.Key } }
            })
            .ToList();
    }

    private static List<XacmlJsonCategory> CreateResourceCategories(
        string serviceResource,
        Guid dialogId,
        string party,
        HashSet<AltinnAction> altinnActions, out Dictionary<string, string> resourceIdByName)
    {
        resourceIdByName = altinnActions
            .Select(x => x.AuthorizationAttribute)
            .Distinct()
            .Select((authorizationAttribute, index) => (authorizationAttribute, id: $"r{index + 1}"))
            .ToDictionary(x =>
                x.authorizationAttribute,
                x => x.id);


        var partyAttribute = ExtractPartyAttribute(party);
        return resourceIdByName
            .Select(x =>
                CreateResourceCategory(x.Value, serviceResource, dialogId, partyAttribute, x.Key))
            .ToList();
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
        if (lastColonIndex == -1 || lastColonIndex == serviceResource.Length - 1)
        {
            // If we don't recognize the format, we just return the whole string as the value and assume
            // that the caller wants to refer a resource in the Resource Registry namespace.
            return (AttributeIdResource, serviceResource);
        }

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
        HashSet<AltinnAction> altinnActions,
        Dictionary<string, string> actionIdByName,
        Dictionary<string, string> resourceIdByName)
    {
        var multiRequests = new XacmlJsonMultiRequests
        {
            RequestReference = new List<XacmlJsonRequestReference>()
        };


        foreach (var (actionName, actionId) in actionIdByName)
        {
            foreach (var resourceName in altinnActions.Where(x => x.Name == actionName).Select(x => x.AuthorizationAttribute))
            {
                multiRequests.RequestReference.Add(new XacmlJsonRequestReference
                {
                    ReferenceId = new List<string> { SubjectId, resourceIdByName[resourceName], actionId }
                });
            }
        }

        return multiRequests;
    }

    public static class NonScalable
    {
        // This contains the helpers for the preliminary implementation which doesn't scale, and should only be used in very low volume situations
        // (such as the PoC). It is not intended for production use.
        //
        // Remove this as soon as we have implemented https://github.com/digdir/dialogporten/issues/42

        public static XacmlJsonRequestRoot CreateDialogSearchRequest(DialogSearchAuthorizationRequest request)
        {
            var requestActions = new HashSet<AltinnAction>
            {
                new (Constants.ReadAction, Constants.MainResource)
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

            for (var i = 0; i < xamlJsonRequestRoot.Request.MultiRequests.RequestReference.Count; i++)
            {
                if (xamlJsonResponse.Response[i].Decision != PermitResponse)
                {
                    continue;
                }

                // Get the name of the resource.
                var resourceId = $"r{i + 1}";
                var serviceResource = $"{AttributeIdResource}:" + xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                        .First(a => a.AttributeId == AttributeIdResource).Value;

                string party;
                var partyOrgNr = xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                        .FirstOrDefault(a => a.AttributeId == AttributeIdOrganizationNumber);
                if (partyOrgNr != null)
                {
                    party = PartyPrefixOrg + partyOrgNr.Value;
                }
                else
                {
                    var partySsn = xamlJsonRequestRoot.Request.Resource.First(r => r.Id == resourceId).Attribute
                        .First(a => a.AttributeId == AttributeIdSsn);
                    party = PartyPrefixPerson + partySsn.Value;
                }

                if (!response.PartiesByResources.TryGetValue(serviceResource, out var parties))
                {
                    parties = new List<string>();
                    response.PartiesByResources.Add(serviceResource, parties);
                }

                parties.Add(party);

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
