---
---
```jsonc
// Input modell som tjenesteeiere oppgir for å endre/oppdatere en dialog.

// I dette eksemplet er det en dialogtjeneste hvor det å "sende inn" er en egen handling, som typisk ikke er 
// tilgjengelig før alt er fylt ut, validert og signert. "Send inn" blir satt til primærhandlingen i GUI. Her oppgis det 
// til å skal være en POST i frontchannel (default for frontchannel er GET) siden det å klikke på knappen medfører 
// tilstandsendring.  Bruker blir da sendt til en eller annen kvitteringsside hos tjenesteeier, som da også har satt 
// dialogen som "completed" via et bakkanal-kall

// PATCH /dialogporten/api/v1/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "actions": {
        "gui": [            
            { 
                "action": "send",
                "type": "primary",
                "title": [ { "code": "nb_NO", "value": "Send inn" } ],
                "url": "https://example.com/some/deep/link/to/dialogues/123456789/send",
                "httpMethod": "POST"
            },
            { 
                "action": "open", 
                "type": "secondary",
                "title": [ { "code": "nb_NO", "value": "Se over før innsending" } ],
                "url": "https://example.com/some/deep/link/to/dialogues/123456789"
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
                        "actionUrl": "https://example.com/api/dialogues/123456789",
                        "method": "GET",
                        "responseSchema": "https://schemas.altinn.no/dialogs/v1/dialogs.json",
                        "documentationUrl": "https://api-docs.example.com/dialogueservice/open-action"
                    }
                ]
            },
            { 
                "action": "confirm",
                "endpoints": [
                    {
                        "method": "POST",
                        "actionUrl": "https://example.com/api/dialogues/123456789/confirmReceived",
                        "documentationUrl": "https://api-docs.example.com/dialogueservice/confirm-action"
                    }
                ]
            },
            { 
                "action": "submit",
                "endpoints": [
                    {
                        "actionUrl": "https://example.com/api/dialogues/123456789/submit",
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
                        "method": "DELETE",
                        "actionUrl": "https://example.com/api/dialogues/123456789"
                    }
                ]
            }
        ]
    },
    // Merk at vi her bryter med vanlig PATCH/merge-semantikk, så her legges bare til et nytt innslag
    "activityHistory": [
        { 
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "information",
            "activityType": "SKE-34355",
            "performedBy": "person:12018212345",
            "activityDescription": [ { "code": "nb_NO", "value": "Dokumentet 'X' ble signert og kan sendes inn" } ]
        }
    ]
}
```