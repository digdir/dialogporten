---
---
```jsonc
// Modellene er hovedsaklig like for SBS og tjenesteier, men er på ulike endepunkter (pga. autorisasjon)


// GET /dialogporten/api/v1/enduser/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "id": "e0300961-85fb-4ef2-abff-681d77f9960e",
    "org": "digdir", // Identifikator for tjenestetilbyder
    "serviceResource": "urn:altinn:resource:super-simple-service", 
    "process": {
        "id": "some-arbitrary-id",
        "order": 1,
        "name": [ { "code": "nb_NO", "value": "Navn på prosess." } ]
    },
    "party": "urn:altinn:organization:identifier-no::991825827",
    "externalReference": "123456789",
    "status": "in-progress",
    "extendedStatus": "SKE-ABC",
    "dates": {
        "createdDateTime": "2022-12-01T10:00:00.000Z",
        "updatedDateTime": "2022-12-01T10:00:00.000Z",
        // Sist meldingen ble "lest", altså ekspandert i UI eller hentet via detailsUrl i API. Hvis ikke oppgitt, eller 
        // readDateTime < updatedDateTime vises typisk dialogen som ulest/oppdatert i GUI.
        "readDateTime": "2022-12-01T10:00:00.000Z", 
        "dueDateTime": "2022-12-01T12:00:00.000Z"
    },
    "content": [
        {
            "type": "Title",
            "value": [{ "code": "nb_NO", "value": "En eksempel på en tittel" } ]
        },
        {
            "type": "Summary",
            "value": [{ "code": "nb_NO", "value": "Kort tekst for oppsummering" } ]
        },
        {
            "type": "AdditionalInfo",
            "value": [{ "code": "nb_NO", "value": "Ytterligere informasjon med <em>begrenset</em> HTML-støtte.
                Dette innholdet vises kun i detaljvisning av dialogen." } ]
        },
        {
            "type": "SenderName",
            "value": [{ "code": "nb_NO", "value": "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } ]
        }
    ],
    // Et token med EdDSA-algoritme, signert av et sertifikat tilgjengelig på et .well-known-endepunkt. 
    // Brukes i utgangspunktet for writeActions, front channel embeds og SignalR
    "dialogToken": "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ....",
    "dialogElements": [
        {
            "dialogElementId": "5b5446a7-9b65-4faf-8888-5a5802ce7de7",
            "dialogElementType": "form-type-1",
            "displayName": [ { "code": "nb_NO", "value": "Innsendt skjema" } ],
            "urls": [
                {
                    "consumerType": "gui",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/5b5446a7.pdf",
                    "contentType": "application/pdf"
                },
                {
                    "consumerType": "api",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/5b5446a7.xml",
                    "contentType": "application/json"
                }
            ]
        },
        {
            "authorizationAttribute": "urn:altinn:subresource:somesubresource",
            "urls": [
                {
                    "consumerType": "gui",
                    "frontChannelEmbed": true,
                    "url": "https://example.com/api/dialogs/123456789/user-instructions",

                    // Ved front channel embeds må content type oppgis. I første omgang støttes 
                    // bare text/html, men på sikt kan flere typer innføres. Arbeidsflate/SBS
                    // vil ha ansvaret for å sikkerhet håndtere innholdet
                    "contentType": "text/html"
                }
            ]
        }, 
        {
            "dialogElementId": "cd6bf231-2347-4131-8ccc-513a6345ef0b",
            "dialogElementType": "form-type-1",
            "displayName": [ { "code": "nb_NO", "value": "Innsendt korrigering #1" } ],
            "urls": [
                {
                    "consumerType": "gui",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/cd6bf231.pdf",
                    "contentType": "application/pdf"
                },
                {
                    "consumerType": "api",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/cd6bf231.xml",
                    "contentType": "application/json"
                }
            ]
        },
        {
            "dialogElementId": "22366651-c1b6-4812-a97d-5ed43fc4fe57",
            "dialogElementType": "error-list",   
            "relatedDialogElementId": "cd6bf231-2347-4131-8ccc-513a6345ef0b",
            "authorizationAttribute": "urn:altinn:subresource:someothersubresource",
            "uris": [
                {
                    "consumerType": "api",
                    "uri": "https://example.com/api/dialogs/123456789/dialogelements/22366651.xml",
                    "contentType": "application/json"
                }
            ]
        },
        {
            "dialogElementId": "a8e0ed0d-1b26-4132-9823-28a5e8ecb24e",            
            "displayName": [ { "code": "nb_NO", "value": "Innsendt korrigering #2" } ],
            "dialogElementType": "skd:form-type-1",
            "urls": [
                {
                    "consumerType": "gui",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/a8e0ed0d.pdf",
                    "contentType": "application/pdf"
                },
                {
                    "consumerType": "api",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/a8e0ed0d.xml",
                    "contentType": "application/xml"
                }
            ]            
        },
        {
            "dialogElementId": "a12fce1f-b2de-4837-bdd8-8743f80d74fc",            
            "displayName": [ { "code": "nb_NO", "value": "Vedtaksbrev" } ],            
            "dialogElementType": "skd:form-type-1",
            "authorizationAttribute": "urn:altinn:subresource:somesubresource",
            // Indikerer om autentisert bruker er autorisert til å lese (har action "read") - finnes bare i response-modell for 
            // sluttbrukere og populeres av Dialogporten utfra policy. Dette er et hint for GUI-implementasjoner som da kan 
            // velge å skjule/gråe ut elementer som ikke er tilgjengelige.
            "isAuthorized": false,
            "urls": [
                {
                    "consumerType": "gui",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/a12fce1f.pdf",
                    "mimeType": "application/pdf"                        
                },
                {
                    "consumerType": "api",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/a12fce1f.xml"
                }
            ]
        }
    ],
    "actions": {
        "gui": [ 
            { 
                "action": "open", // Denne kan refereres i XACML-policy
                "priority": "primary", // Dette bestemmer hvordan handlingen presenteres.
                "title": [ { "code": "nb_NO", "value": "Åpne i dialogtjeneste" } ],
                "url": "https://example.com/some/deep/link/to/dialogs/123456789"
            },
            {
                "action": "confirm",
                "authorizationAttribute": "urn:altinn:subresource:somesubresource",
                // Indikerer om autentisert bruker er autorisert å aksessere oppgitt url  - finnes bare i response-modell for 
                // sluttbrukere og populeres av Dialogporten utfra policy. Dette er et hint for GUI-implementasjoner som da kan 
                // velge å skjule/gråe ut elementer som ikke er tilgjengelige.
                "isAuthorized": false,
                "priority": "secondary",
                "title": [ { "code": "nb_NO", "value": "Bekreft mottatt" } ],
                "writeAction": {
                    "method": "post",
                    "prompt": [ { "code": "nb_NO", "value": "Vil du sende lesebekreftelse?" } ]
                },
                // Et token med EdDSA-algoritme, signert av et sertifikat tilgjengelig på et .well-known-endepunkt
                // Blir kun generert på dialogElementer med frontChannelEmbed eller write actions
                "dialogToken": "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ....",
                "url": "https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"
            },
            { 
                "action": "delete",
                "priority": "tertiary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],
                "deleteAction": true,
                // Et token med EdDSA-algoritme, signert av et sertifikat tilgjengelig på et .well-known-endepunkt
                // Blir kun generert på dialogElementer med frontChannelEmbed eller write actions
                "dialogToken": "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ....",
                "url": "https://example.com/some/deep/link/to/dialogs/123456789"
            }
        ],
        "api": [ 
            { 
                "action": "open",
                "endpoints": [ 
                    {
                        "version": "v2",
                        "actionUrl": "https://example.com/api/v2/dialogs/123456789",
                        "method": "GET",
                        "responseSchema": "https://schemas.altinn.no/dialogs/v2/dialogs.json", 
                        "documentationUrl": "https://api-docs.example.com/v2/dialogservice/open-action"                         
                    },
                    {
                        "version": "v1",
                        "deprecated": true, 
                        "sunsetDate": "2024-12-31T23:59:59.999Z",
                        "actionUrl": "https://example.com/api/v1/dialogs/123456789",
                        "method": "GET",
                        "responseSchema": "https://schemas.altinn.no/dialogs/v1/dialogs.json", 
                        "documentationUrl": "https://api-docs.example.com/v1/dialogservice/open-action",
                    },
                ]
            },
            { 
                "action": "confirm",
                "endpoints": [
                    {
                        "version": "v1",
                        "method": "POST",
                        "isAuthorized": false,
                        "actionUrl": "https://example.com/api/dialogs/123456789/confirmReceived",
                        "documentationUrl": "https://api-docs.example.com/dialogservice/confirm-action"
                    }
                ]                
            },
            {
                "action": "submit", // Denne kan refereres i XACML-policy
                "endpoints": [
                    {
                        "version": "v1",
                        "action": "submit", // Denne kan refereres i XACML-policy
                        "actionUrl": "https://example.com/api/dialogs/123456789",
                        "method": "POST",
                        "requestSchema": "https://schemas.example.com/dialogservice/v1/dialogservice.json", 
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
                        "actionUrl": "https://example.com/api/dialogs/123456789"
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
            "performedBy": [ { "code": "nb_NO", "value": "Anne Olsen" } ],
            "extendedActivityType": "SKE-1234-received-precheck-ok",
            "dialogElementId": "5b5446a7-9b65-4faf-8888-5a5802ce7de7",
            "activityDescription": [ { "code": "nb_NO", "value": "Innsending er mottatt og sendt til behandling" } ]
        }
        { 
            "activityId": "7f91fb5e-4c79-4c01-82aa-84911ef13b75",
            "activityDateTime": "2022-12-01T10:15:00.000Z",
            "activityType": "seen",
            "performedBy": [ { "code": "nb_NO", "value": "Anne Olsen" } ],
        },
        { 
            "activityId": "e13b308b-3873-460b-8486-205ce934f4b0",
            "activityDateTime": "2022-12-01T10:16:00.000Z",
            "activityType": "forwarded",
            "performedBy": [ { "code": "nb_NO", "value": "Anne Olsen" } ],
            // Mottaker av delegering
            "recipient": [ { "code": "nb_NO", "value": "Kari Pettersen" } ],
        },
        { 
            "activityId": "ab06af62-6067-477f-b18c-bf54222273b9",            
            "activityDateTime": "2022-12-01T11:00:00.000Z",
            "activityType": "feedback",
            // Feedback-typer har vanligvis en referanse til en submission-aktivitet som dette er feedback for
            "relatedActivityId": "fc6406df-6163-442a-92cd-e487423f2fd5",
            "extendedActivityType": "SKE-2456-need-form-RF1337",
            "activityDescription": [ { "code": "nb_NO", "value": "Behandling er utført. Ytterligere opplysninger kreves." } ],
        },
        { 
            "activityId": "f6ef1a96-df3a-4d38-830f-853b5d090e16",
            "activityDateTime": "2022-12-01T12:00:00.000Z",
            "activityType": "submission",
            "extendedActivityType": "SKE-2456-received-precheck-ok",
            "dialogElementId": "cd6bf231-2347-4131-8ccc-513a6345ef0b",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Innsending av ytterligere opplysninger er mottatt og sendt til behandling." 
            } ]
        },
        { 
            "activityId": "7d971b46-fb66-4a97-8f5e-333c1df54678",
            "activityDateTime": "2022-12-01T13:00:00.000Z",
            "activityType": "error",
            // Feilmeldinger har også vanligvis en referanse til en tidligere aktivitet og/eller 
            // dialogelement som var årsak til at feilsituasjonen oppstod
            "relatedActivityId": "f6ef1a96-df3a-4d38-830f-853b5d090e16",
            // Feilmeldinger kan også ha et eget dialogelement som inneholder en strukturert feilmelding 
            "dialogElementId": "22366651-c1b6-4812-a97d-5ed43fc4fe57",            
            "activityErrorCode": "SKE-error-12345",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Saksbehandling har avdekket feil i innsending." 
            } ]
        },
        { 
            "activityId": "4ce2e110-21c5-4783-94ed-b2a8695abb8a",
            "activityDateTime": "2022-12-01T14:00:00.000Z",
            "activityType": "submission",
            "extendedActivityType": "SKE-2456-received-precheck-ok",
            "dialogElementId": "a8e0ed0d-1b26-4132-9823-28a5e8ecb24e",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Innsending av ytterligere opplysninger er mottatt og sendt til behandling." 
            } ]
        },
        { 
            "activityId": "20c94e10-b95d-4cd0-b469-b4caa4532c4e",
            "activityDateTime": "2022-12-01T15:00:00.000Z",
            "activityType": "feedback",
            "extendedActivityType": "SKE-2456-final-ok",
            "dialogElementId": "a12fce1f-b2de-4837-bdd8-8743f80d74fc",
            "activityDescription": [ { 
                "code": "nb_NO", 
                "value": "Saksbehandling er utført og vedtak er fattet, se vedlagt vedtaksbrev." 
            } ]
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
        "visibleDateTime": "2022-12-01T12:00:00.000Z"
    },
    // HAL til relatert informasjon
    "_links": {
        "self": { "href": "/dialogporten/api/v1/enduser/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e" },        
        
        // eget endepunkt for varslingslogg for dialogen
        "notificationlog": { "href": "/dialogporten/api/v1/enduser/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e/notificationlog" }, 

        // Dyplenke til portalvisning for dialogen i Dialogporten
        "serviceresource": { "href": "/resourceregistry/api/v1/resource/super-simple-service/" }, 

        // Dyplenke til portalvisning for dialogen i Dialogporten
        "selfgui": { "href": "https://www.dialogporten.no/?expandDialog=e0300961-85fb-4ef2-abff-681d77f9960e" }, 

        // Dyplenke til portalvisning for dialogen hos tjenesteeier
        "externalgui": { "href": "https://example.com/some/deep/link/to/dialogs/123456789" } 
    }
}
```