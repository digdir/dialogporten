using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public sealed class DialogDetailsAuthorizationRequest
{
    public required ClaimsPrincipal ClaimsPrincipal { get; init; }
    public required string ServiceResource { get; init; }
    public required Guid DialogId { get; init; }
    public required string Party { get; init; }

    // Each action applies to a resource. This is the main resource, or another resource indicated by a authorization attribute
    // eg. "urn:altinn:subresource:some-sub-resource" or "urn:altinn:task:task_1"
    public required HashSet<AltinnAction> AltinnActions { get; init; }
    // read - main
    // write - main
    // read - sub1
    // read - sub2


}

/*

{
    "Request": {
        "ReturnPolicyIdList": false,
        "CombinedDecision": false,
        "XPathVersion": null,
        "Category": null,
        "Resource": [
            {
                "CategoryId": null,
                "Id": "r1",
                "Content": null,
                "Attribute": [
                    {
                        "AttributeId": "urn:altinn:resource",
                        "Value": "super-simple-service",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:ssn",
                        "Value": "15876497724",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    }
                ]
            },
            {
                "CategoryId": null,
                "Id": "r2",
                "Content": null,
                "Attribute": [
                    {
                        "AttributeId": "urn:altinn:resource",
                        "Value": "super-simple-service",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:subresource",
                        "Value": "sub1",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:ssn",
                        "Value": "15876497724",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    }
                ]
            },
            {
                "CategoryId": null,
                "Id": "r3",
                "Content": null,
                "Attribute": [
                    {
                        "AttributeId": "urn:altinn:resource",
                        "Value": "super-simple-service",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:subresource",
                        "Value": "sub2",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:organizationnumber",
                        "Value": "991825827",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    }
                ]
            },
            {
                "CategoryId": null,
                "Id": "r4",
                "Content": null,
                "Attribute": [
                    {
                        "AttributeId": "urn:altinn:resource",
                        "Value": "other-service",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:organizationnumber",
                        "Value": "991825827",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    }
                ]
            }
        ],
        "Action": [
            {
                "CategoryId": null,
                "Id": "a1",
                "Content": null,
                "Attribute": [
                    {
                        "AttributeId": "urn:oasis:names:tc:xacml:1.0:action:action-id",
                        "Value": "read",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    }
                ]
            }
        ],
        "AccessSubject": [
            {
                "CategoryId": null,
                "Id": "s1",
                "Content": null,
                "Attribute": [
                    {
                        "AttributeId": "urn:altinn:authenticatemethod",
                        "Value": "NotDefined",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:authlevel",
                        "Value": "3",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:ssn",
                        "Value": "15876497724",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    },
                    {
                        "AttributeId": "urn:altinn:userid",
                        "Value": "1260207",
                        "Issuer": null,
                        "DataType": null,
                        "IncludeInResult": false
                    }
                ]
            }
        ],
        "RecipientSubject": null,
        "IntermediarySubject": null,
        "RequestingMachine": null,
        "MultiRequests": {
            "RequestReference": [
                {
                    "ReferenceId": [
                        "s1",
                        "r1",
                        "a1"
                    ]
                },
                {
                    "ReferenceId": [
                        "s1",
                        "r2",
                        "a1"
                    ]
                },
                {
                    "ReferenceId": [
                        "s1",
                        "r3",
                        "a1"
                    ]
                },
                {
                    "ReferenceId": [
                        "s1",
                        "r4",
                        "a1"
                    ]
                }
            ]
        }
    }
}

*/
