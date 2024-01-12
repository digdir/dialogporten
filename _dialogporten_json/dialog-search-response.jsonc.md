---
---
```jsonc
// Listemodell som brukes i dialogboks-søk. Presis paginering er antagelig ikke mulig pga. behov for individuell 
// autorisasjon per dialog, men det burde være en eller annen "next"-mekanisme som lar et SBS iterere gjennom en liste.

// GET /dialogporten/api/v1/enduser/dialogs/?search=....
[
    {
        "id": "e0300961-85fb-4ef2-abff-681d77f9960e",
        "org": "digdir",
        "serviceResource": "super-simple-service",
        "externalReference": "someReference",
        "party": "urn:altinn:person:identifier-no::12018212345",        
        "dates": {
            "createdDateTime": "2022-12-01T10:00:00.000Z",
            "updatedDateTime": "2022-12-01T10:00:00.000Z",
            "dueDateTime": "2022-12-01T12:00:00.000Z"  
        },
        "status": "in-progress",
        "extendedStatus": "SKE-ABC",
        // Inneholder ikke "AdditionalInfo", denne må hentes i detailsUrl
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
                "type": "SenderName",
                "value": [{ "code": "nb_NO", "value": "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } ]
            },
        ],
        "detailsUrl": "/dialogporten/api/v1/enduser/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e"
    }
]
```