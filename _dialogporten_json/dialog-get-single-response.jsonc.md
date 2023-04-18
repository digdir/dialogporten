---
---
```jsonc
// Modellene er hovedsaklig like for SBS og tjenesteier, med unntak:
// - "configuration" vises kun til tjenesteeier


// GET /dialogporten/api/v1/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "id": "e0300961-85fb-4ef2-abff-681d77f9960e",
    "org": "digdir", // Identifikator for tjenestetilbyder
    "serviceResourceIdentifier": "example_dialogue_service", 
    "dialogueGroup": {
        "id": "some-arbitrary-id",
        "order": 1,
        "name": [ { "code": "nb_NO", "value": "Navn på dialoggruppe." } ]
    },
    "party": "org/991825827",
    "externalReference": "123456789",
    "status": "in-progress",
    "externalStatus": "SKE-ABC",
    "dates": {
        "createdDateTime": "2022-12-01T10:00:00.000Z",
        "updatedDateTime": "2022-12-01T10:00:00.000Z",
        // Sist meldingen ble "lest", altså ekspandert i UI eller hentet via detailsUrl i API. Hvis ikke oppgitt, eller 
        // readDateTime < updatedDateTime vises typisk dialogen som ulest/oppdatert i GUI.
        "readDateTime": "2022-12-01T10:00:00.000Z", 
        "dueDateTime": "2022-12-01T12:00:00.000Z"
    },
    "content": {
        "body": [ { "code": "nb_NO", 
            "value": "Innhold med <em>begrenset</em> HTML-støtte. Dette innholdet vises når dialogen ekspanderes." } ],
        "title": [ { "code": "nb_NO", "value": "En eksempel på en tittel" } ],

        // Overstyrt tittel til bruk i søke/liste-visning. Kan potensielt være synlig uten tilgang til dialogen.
        "searchTitle": [ { "code": "nb_NO", "value": "En eksempel på en tittel brukt i listevisning" } ],
        "senderName": [ { "code": "nb_NO", "value": "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } ]            
    },
    "attachments": [
        {
            "displayName": [ { "code": "nb_NO", "value": "dette er et vedlegg" } ],
            "sizeInBytes": 123456,
            "contentType": "application/pdf",
            "url": "https://example.com/api/dialogues/123456789/attachments/1",
            "subresource": "attachment1"
        }
    ],
    "actions": {
        // Indikerer hvilke actions autentisert bruker er autorisert for å utføre - finnes bare i response-modell for 
        // sluttbrukere og populeres av Dialogporten utfra policy. Dette er et hint for GUI-implementasjoner som da kan 
        // velge å skjule/gråe ut actions som ikke vil kunne gjennomføres.
        "authorizedActions": ["open"],
        "gui": [ 
            { 
                "action": "open", // Denne kan refereres i XACML-policy
                "type": "primary", // Dette bestemmer hvordan handlingen presenteres.
                "title": [ { "code": "nb_NO", "value": "Åpne i dialogtjeneste" } ],
                "url": "https://example.com/some/deep/link/to/dialogues/123456789"
            },
            {
                "action": "confirm",
                "subresource": "somesubresource", 
                "type": "secondary",
                "title": [ { "code": "nb_NO", "value": "Bekreft mottatt" } ],
                "isBackChannel": true,
                "url": "https://example.com/some/deep/link/to/dialogues/123456789/confirmReceived"
            },
            { 
                "action": "delete",
                "type": "tertiary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],
                "isDeleteAction": true,
                "url": "https://example.com/some/deep/link/to/dialogues/123456789"
            }
        ],
        "api": [ 
            { 
                "action": "open",
                "endpoints": [ 
                    {
                        "version": "v2",
                        "actionUrl": "https://example.com/api/v2/dialogues/123456789",
                        "method": "GET",
                        "responseSchema": "https://schemas.altinn.no/dialogs/v2/dialogs.json", 
                        "documentationUrl": "https://api-docs.example.com/v2/dialogueservice/open-action"                         
                    },
                    {
                        "version": "v1",
                        "deprecated": true, 
                        "sunsetDate": "2024-12-31T23:59:59.999Z",
                        "actionUrl": "https://example.com/api/v1/dialogues/123456789",
                        "method": "GET",
                        "responseSchema": "https://schemas.altinn.no/dialogs/v1/dialogs.json", 
                        "documentationUrl": "https://api-docs.example.com/v1/dialogueservice/open-action",
                    },
                ]
            },
            { 
                "action": "confirm",
                "endpoints": [
                    {
                        "version": "v1",
                        "method": "POST",
                        "actionUrl": "https://example.com/api/dialogues/123456789/confirmReceived",
                        "documentationUrl": "https://api-docs.example.com/dialogueservice/confirm-action"
                    }
                ]                
            },
            {
                "action": "submit", // Denne kan refereres i XACML-policy
                "endpoints": [
                    {
                        "version": "v1",
                        "action": "submit", // Denne kan refereres i XACML-policy
                        "actionUrl": "https://example.com/api/dialogues/123456789",
                        "method": "POST",
                        "requestSchema": "https://schemas.example.com/dialogueservice/v1/dialogueservice.json", 
                        "responseSchema": "https://schemas.altinn.no/dialogs/v1/dialogs.json" 
                    }
                ]
            },
            { 
                "action": "delete",
                "endpoints": [
                    {
                        "version": "v1",
                        "method": "DELETE",
                        "actionUrl": "https://example.com/api/dialogues/123456789"
                    }
                ]
            }
        ]
    },
    // Se dialogporten-create-request.json for feltforklaringer
    "activityHistory": [
        {
            "activityId": "fc6406df-6163-442a-92cd-e487423f2fd5",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": "person:12018212345",
            "externalType": "SKE-1234-received-precheck-ok",
            "externalId": "b323cef4-adbd-4d2c-b33d-5c0f3b11171b",
            "activityDescription": [ { "code": "nb_NO", "value": "Innsending er mottatt og sendt til behandling" } ],
            "activityDetailsUrls": {
                "api": [
                    {
                        "version": "v1",
                        "url": "https://example.com/api/dialogues/123456789/received_submissions/b323cef4-adbd-4d2c-b33d-5c0f3b11171b"
                    }
                ],
                "gui": "https://example.com/dialogues/123456789/view_submission/b323cef4-adbd-4d2c-b33d-5c0f3b11171b"
            },
        }
        { 
            "activityId": "7f91fb5e-4c79-4c01-82aa-84911ef13b75",
            "activityDateTime": "2022-12-01T10:15:00.000Z",
            "activityType": "seen",
            "performedBy": "person:12018212345",
        },
        { 
            "activityId": "e13b308b-3873-460b-8486-205ce934f4b0",
            "activityDateTime": "2022-12-01T10:16:00.000Z",
            "activityType": "forwarded",
            "performedBy": "person:12018212345",
            // Mottaker av delegering
            "recipient": "person:24058412345",
        },
        { 
            "activityId": "ab06af62-6067-477f-b18c-bf54222273b9",            
            "activityDateTime": "2022-12-01T11:00:00.000Z",
            "activityType": "feedback",
            // Feedback-typer har vanligvis en referanse til en submission-aktivitet som dette er feedback for
            "relatedActivityId": "fc6406df-6163-442a-92cd-e487423f2fd5",
            "externalType": "SKE-2456-need-form-RF1337",
            "activityDescription": [ { "code": "nb_NO", "value": "Behandling er utført. Ytterligere opplysninger kreves." } ],
            "activityDetailsUrls": {
                // Feltene "api" og "gui" er begge valgfrie, såvel som feltet activityDetailsUrls i seg selv. I dette
                // eksemplet er det ikke ytterligere opplysninger til SBS-er (input/output-modeller er indikert av action)

                // Her kan det for GUI-brukere lenkes til en eller annen dokumentasjon som forklarer mer om hva som
                // kreves, hvorfor prosessen krever dette nå etc.
                "gui": "https://docs.example.com/forms/additional_info_for_service/faq"
            }
        },
        { 
            "activityId": "f6ef1a96-df3a-4d38-830f-853b5d090e16",
            "activityDateTime": "2022-12-01T12:00:00.000Z",
            "activityType": "submission",
            "externalType": "SKE-2456-received-precheck-ok",
            "externalId": "22366651-c1b6-4812-a97d-5ed43fc4fe57",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Innsending av ytterligere opplysninger er mottatt og sendt til behandling." 
            } ],
            "activityDetailsUrls": {
                "api": [
                    {
                        "version": "v1",
                        "url": "https://example.com/api/dialogues/123456789/received_submissions/22366651-c1b6-4812-a97d-5ed43fc4fe57"
                    }
                ],
                "gui": "https://example.com/dialogues/123456789/view_submission/22366651-c1b6-4812-a97d-5ed43fc4fe57"
            }
        },
        { 
            "activityId": "7d971b46-fb66-4a97-8f5e-333c1df54678",
            "activityDateTime": "2022-12-01T13:00:00.000Z",
            "activityType": "error",
            // Feilmeldinger har også vanligvis en referanse til en tidligere aktivitet som var årsak til at feilsituasjonen oppstod
            "relatedActivityId": "f6ef1a96-df3a-4d38-830f-853b5d090e16",
            "externalId": "22366651-c1b6-4812-a97d-5ed43fc4fe57",
            "activityErrorCode": "SKE-error-12345",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Saksbehandling har avdekket feil i innsending. Følg lenken for mer detaljer." 
            } ],
            "activityDetailsUrls": {
                // For API-brukere kan denne f.eks. inneholde en eller annen modell som inneholder en 
                // strukturert liste over feil som har oppstått.
                "api": [
                    {
                        "version": "v1",
                        "url": "https://example.com/api/dialogues/123456789/received_submissions/22366651-c1b6-4812-a97d-5ed43fc4fe57/errors"
                    }
                ],
                // Her kan det for GUI-brukere lenkes til en eller annen dokumentasjon som forklarer mer om hva som
                // er feil. 
                "gui": "https://docs.example.com/forms/errors/SKE-error-12345"
            }
        },
        { 
            "activityId": "4ce2e110-21c5-4783-94ed-b2a8695abb8a",
            "activityDateTime": "2022-12-01T14:00:00.000Z",
            "activityType": "submission",
            "externalType": "SKE-2456-received-precheck-ok",
            "externalId": "d1cbd317-277f-4521-b9b5-ee7d6b29ceaf",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Innsending av ytterligere opplysninger er mottatt og sendt til behandling." 
            } ],
            "activityDetailsUrls": {
                "api": [
                    {
                        "version": "v1",
                        "url": "https://example.com/api/dialogues/123456789/received_submissions/d1cbd317-277f-4521-b9b5-ee7d6b29ceaf"
                    }
                ],
                "gui": "https://example.com/dialogues/123456789/view_submission/d1cbd317-277f-4521-b9b5-ee7d6b29ceaf"
            }
        },
        { 
            "activityId": "20c94e10-b95d-4cd0-b469-b4caa4532c4e",
            "activityDateTime": "2022-12-01T15:00:00.000Z",
            "activityType": "feedback",
            "relatedActivityId": "4ce2e110-21c5-4783-94ed-b2a8695abb8a",
            "externalType": "SKE-2456-final-ok",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Saksbehandling er utført og vedtak er fattet, se vedlegg. 
                          Følg lenken for generell informasjon om videre prosess." 
            } ],
            "activityDetailsUrls": {
                // Her kan det  lenkes til en eller annen dokumentasjon som forklarer mer om vedtaket, 
                // informasjon om videre prosess, klageadgang også videre.
                "gui": "https://docs.example.com/forms/additional_info_for_service/faq"
            }
        },
        { 
            "activityId": "b6d96fc1-edac-407e-aa96-147f07878092",
            "activityDateTime": "2022-12-22T15:00:00.000Z",
            // En "closed"-oppføring knyttes som regel med at en dialog settes som "cancelled" eller "completed", 
            // og indikerer at den konkrete dialogen er avsluttet.
            "activityType": "closed",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Klagefrist utløpt, sak avsluttet." 
            } ]
        }
    ],
    // Dette er ulike innstillinger som kun kan oppgis og er synlig for tjenesteeier. Se de-create-request for informasjon om feltene.
    "configuration": {        
        "serviceProviderScopesRequired": [ "serviceprovider:myservice" ],
        "visibleDateTime": "2022-12-01T12:00:00.000Z"
    },
    // HAL til relatert informasjon
    "_links": {
        "self": { "href": "/dialogporten/api/v1/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e" },        
        
        // eget endepunkt for varslingslogg for dialogen
        "notificationlog": { "href": "/dialogporten/api/v1/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e/notificationlog" }, 

        // Dyplenke til portalvisning for dialogen i Dialogporten
        "serviceresource": { "href": "/resourceregistry/api/v1/resource/example_dialogue_service/" }, 

        // Dyplenke til portalvisning for dialogen i Dialogporten
        "selfgui": { "href": "https://www.dialogporten.no/?expandDialog=e0300961-85fb-4ef2-abff-681d77f9960e" }, 

        // Dyplenke til portalvisning for dialogen hos tjenesteeier
        "externalgui": { "href": "https://example.com/some/deep/link/to/dialogues/123456789" } 
    }
}
```