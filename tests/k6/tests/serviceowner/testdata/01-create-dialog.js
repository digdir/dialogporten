import { uuidv4 } from '../../../common/testimports.js'
import { getDefaultEnduserSsn } from "../../../common/token.js";

export default function () {
    return {
        "serviceResource": "urn:altinn:resource:ttd-dialogporten-automated-tests", // urn starting with urn:altinn:resource:
        "party": "urn:altinn:person:identifier-no:" + getDefaultEnduserSsn(), // or urn:altinn:organization:identifier-no:<9 digits>
        "status": "new", // valid values: new, inprogress, waiting, signing, cancelled, completed
        "extendedStatus": "urn:any/valid/uri",
        "dueAt": "2033-11-25T06:37:54.2920190Z", // must be UTC
        "expiresAt": "2053-11-25T06:37:54.2920190Z", // must be UTC
        "visibleFrom": "2032-11-25T06:37:54.2920190Z", // must be UTC
        "searchTags": [
            { "value": "something searchable" },
            { "value": "something else searchable" }
        ],
        "content": [
            { "type": "Title", "value": [ { "languageCode": "nb", "value": "Skjema for rapportering av et eller annet" } ] },
            { "type": "SenderName", "value": [ { "languageCode": "nb", "value": "Avsendernavn" } ] },
            { "type": "Summary", "value": [ { "languageCode": "nb", "value": "Et sammendrag her. Maks 200 tegn, ingen HTML-støtte. Påkrevd. Vises i liste." } ] },
            { "type": "AdditionalInfo", "mediaType": "text/plain", "value": [ { "languageCode": "nb", "value": "Utvidet forklaring (enkel HTML-støtte, inntil 1023 tegn). Ikke påkrevd. Vises kun i detaljvisning." } ] },
            { "type": "ExtendedStatus", "value": [ { "languageCode": "nb", "value": "Utvidet Status" } ] },
        ],
        "guiActions": [
            {
                "action": "read",
                "url": "https://digdir.no",
                "priority": "Primary",
                "title": [
                    {
                        "value": "Gå til dialog",
                        "languageCode": "nb"
                    }
                ]
            },
            {
                "action": "read",
                "url": "https://digdir.no",
                "priority": "Secondary",
                "httpMethod": "POST",
                "title": [
                    {
                        "value": "Utfør handling uten navigasjon",
                        "languageCode": "nb"
                    }
                ],
                "prompt": [ { "value": "Er du sikker?", "languageCode": "nb" } ]
            }
        ],
        "apiActions": [
            {
                "action": "some_unauthorized_action",
                "endPoints": [
                    {
                        "url": "https://digdir.no",
                        "httpMethod": "GET"
                    },
                                    {
                        "url": "https://digdir.no/deprecated",
                        "httpMethod": "GET",
                        "deprecated": true
                    }
                ]
            }
        ],
        "attachments": [
            {
                "displayName": [
                    {
                        "languageCode": "nb",
                        "value": "Et vedlegg"
                    }
                ],
                "urls": [
                    {
                        "consumerType": "gui",
                        "url": "https://foo.com/foo.pdf",
                        "mediaType": "application/pdf"
                    }
                ]
            },
            {
                "displayName": [
                    {
                        "languageCode": "nb",
                        "value": "Et annet vedlegg"
                    }
                ],
                "urls": [
                    {
                        "consumerType": "gui",
                        "url": "https://foo.com/foo.pdf",
                        "mediaType": "application/pdf"
                    }
                ]
            },
            {
                "displayName": [
                    {
                        "languageCode": "nb",
                        "value": "Nok et vedlegg"
                    }
                ],
                "urls": [
                    {
                        "consumerType": "gui",
                        "url": "https://foo.com/foo.pdf",
                        "mediaType": "application/pdf"
                    }
                ]
            }
        ],
        "activities": [
            {
                "type": "Error",
                "performedBy": "Politiet",
                "description": [
                    {
                        "value": "Lovbrudd",
                        "languageCode": "nb"
                    },
                    {
                        "value": "Ofensa",
                        "languageCode": "es"
                    }
                ]
            },
            {
                "type": "Submission",
                "performedBy": "NAV",
                "description": [
                    {
                        "value": "Brukeren har levert et viktig dokument.",
                        "languageCode": "nb"
                    }
                ]
            },
            {
                "type": "Submission",
                "performedBy": "Skatteetaten",
                "description": [
                    {
                        "value": "Brukeren har begått skattesvindel",
                        "languageCode": "nb"
                    },
                    {
                        "value": "Tax fraud",
                        "languageCode": "en"
                    }
                ]
            }
        ]
    };
};
