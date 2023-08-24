---
---
```jsonc
// Input modell som tjenesteeiere oppgir for å endre/oppdatere en dialog.

// I dette eksemplet er det en dialogtjeneste hvor det å "sende inn" er en egen handling, som typisk ikke er 
// tilgjengelig før alt er fylt ut, validert og signert. "Send inn" blir satt til primærhandlingen i GUI. Her oppgis det 
// til å skal være en POST i frontchannel (default for frontchannel er GET) siden det å klikke på knappen medfører 
// tilstandsendring.  Bruker blir da sendt til en eller annen kvitteringsside hos tjenesteeier, som da også har satt 
// dialogen som "completed" via et bakkanal-kall

// PATCH /dialogporten/api/v1/serviceowner/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "actions": {
        "gui": [            
            { 
                "action": "send",
                "priority": "primary",
                "title": [ { "code": "nb_NO", "value": "Send inn" } ],
                "url": "https://example.com/some/deep/link/to/dialogs/123456789/send",
                "httpMethod": "POST"
            },
            { 
                "action": "open", 
                "priority": "secondary",
                "title": [ { "code": "nb_NO", "value": "Se over før innsending" } ],
                "url": "https://example.com/some/deep/link/to/dialogs/123456789"
            }, 
            { 
                "action": "delete",
                "priority": "tertiary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],
                "isDeleteAction": true, 
                "url": "https://example.com/some/deep/link/to/dialogs/123456789" 
            }
        ],
        "api": [ 
            { 
                "action": "open",
                "endpoints": [
                    {
                        "actionUrl": "https://example.com/api/dialogs/123456789",
                        "method": "GET",
                        "responseSchema": "https://schemas.altinn.no/dialogs/v1/dialogs.json",
                        "documentationUrl": "https://api-docs.example.com/dialogservice/open-action"
                    }
                ]
            },
            { 
                "action": "confirm",
                "endpoints": [
                    {
                        "method": "POST",
                        "actionUrl": "https://example.com/api/dialogs/123456789/confirmReceived",
                        "documentationUrl": "https://api-docs.example.com/dialogservice/confirm-action"
                    }
                ]
            },
            { 
                "action": "submit",
                "endpoints": [
                    {
                        "actionUrl": "https://example.com/api/dialogs/123456789/submit",
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
                        "method": "DELETE",
                        "actionUrl": "https://example.com/api/dialogs/123456789"
                    }
                ]
            }
        ]
    },
    // Merk at vi her bryter med vanlig PATCH/merge-semantikk; her skal det kun legges til et element
    // Det reelle API-et vil støtte POST på et activityhistory-endepunkt, samt JSON PATCH for å kunne gjøre alt atomisk
    "activityHistory": [
        { 
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "information",
            "activityType": "SKE-34355",
            "performedBy": [ { "code": "nb_NO", "value": "Anne Olsen" } ],
            "activityDescription": [ { "code": "nb_NO", "value": "Dokumentet 'X' ble signert og kan sendes inn" } ]
        }
    ]
}
```