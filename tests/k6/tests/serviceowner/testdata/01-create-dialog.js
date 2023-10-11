import { uuidv4 } from '../../../common/testimports.js'

export default function () {
    let dialogElementId = uuidv4();
    let activityId = uuidv4();
    return {
        "serviceResource": "urn:altinn:resource:super-simple-service",
        "party": "/org/991825827",
        "status": "waiting",
        "dueAt": "2023-11-25T06:37:54.2920190Z",
        "title": [
            {
                "cultureCode": "nb_NO",
                "value": "Et eksempel på en tittel - due at 3"
            }
        ],
        "apiActions": [
            {
                "action": "some:action",
                "dialogElementId": dialogElementId,
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
        "elements": [
            {
                "id": dialogElementId,
                "type": "some:type",
                "displayName": [
                    {
                        "cultureCode": "nb_NO",
                        "value": "Et vedlegg"
                    }
                ],
                "urls": [
                    {
                        "consumerType": "gui",
                        "url": "https://foo.com/foo.pdf",
                        "mimeType": "application/pdf"
                    }
                ]
            },
            {
                "relatedDialogElementId": dialogElementId,
                "type": "some:type",
                "displayName": [
                    {
                        "cultureCode": "nb_NO",
                        "value": "Et annet vedlegg"
                    }
                ],
                "urls": [
                    {
                        "consumerType": "gui",
                        "url": "https://foo.com/foo.pdf",
                        "mimeType": "application/pdf"
                    }
                ]
            }
        ],
        "history": [
            {
                "id": activityId,
                "typeId": "submission",
                "extendedType": "some:extended:type"
            },
            {
                "relatedActivityId": activityId,
                "type": "submission",
                "extendedType": "some:extended:type",
                "dialogElementId": dialogElementId
            }
        ],
        "activities": [
            {
                "type": "Error",
                "dialogElementId": dialogElementId,
                "performedBy": [
                    {
                        "value": "Politiet",
                        "cultureCode":"nb_no"
                    },
                    {
                        "value": "La policia",
                        "cultureCode":"es_es"
                    }
                ],
                "description": [
                    {
                        "value": "Lovbrudd",
                        "cultureCode": "nb_no"
                    },
                    {
                        "value": "Ofensa",
                        "cultureCode": "es_es"
                    }
                ]
            },
            {
                "type": "Submission",
                "performedBy": [
                    {
                        "value": "NAV",
                        "cultureCode": "nb_no"
                    }
                ],
                "description": [
                    {
                        "value": "Brukeren har levert et viktig dokument.",
                        "cultureCode": "nb_no"
                    }
                ]
            },
            {
                "type": "Submission",
                "performedBy": [
                    {
                        "value": "Skatteetaten",
                        "cultureCode": "nb_no"
                    },
                    {
                        "value": "IRS",
                        "cultureCode": "en_us"
                    }
                ],
                "description": [
                    {
                        "value": "Brukeren har begått skattesvindel",
                        "cultureCode": "nb_no"
                    },
                    {
                        "value": "Tax fraud",
                        "cultureCode": "en_us"
                    }
                ]
            }
        ]
    };
};