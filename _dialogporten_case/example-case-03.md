---
menutitle: Eksempel 03
title: Eksempel-case som beskriver to signeringssteg for ulike parter
layout: page
toc: true
---

## Introduksjon

Denne er en mulig implementasjon av hvilken som helst innsendingstjeneste (her kalt "Omsetningsrapportering") som har har et utfyllingssteg samt to signeringssteg for hhv daglig leder hos parten og registrert regnskapsfører.

Partene er Rørlegger AS, Regnskapsfører AS og Etaten, som tilbyr denne innsendingstjenesten både gjennom GUI og API.

## Oversikt over steg

1. En regnskapsmedarbeider hos Rørlegger AS initierer en innsending. Dette kan gjøres direkte mot Etaten, her er det eksemplifisert med et kall til instansierings API-et. Dette kan være gjennom et SBS som kjenner protokollen, eller gjennom at medarbeideren finner og starter tjenesten i en tjenestekatalog.
2. Etaten oppretter dialogen i Dialogporten
3. Regnskapsmedarbeideren foretar en skjemautfylling (enten via GUI eller API)
4. Etaten mottar innsendingen, foretar maskinell kontroll, og sender den til signering av daglig leder
5. Daglig leder varsles, logger inn i Dialogporten, og signerer innsendingen
6. Etaten mottar signeringen, og sender den til signering av regnskapsfører
7. Regnskapsfører varsles, logger inn i Dialogporten, og signerer innsendingen
8. Etaten mottar siste signering og avslutter innsendingen

{% include note.html type="warning" content="Modellene som vises under er forenklede ift de reelle modellene som må benyttes. Events som genereres er utelatt - for eksempler på disse se case-01" %}

## 1. Rørlegger AS instansierer en dialog

```jsonc
// POST /dialogporten/api/v1/instantiate
{
    "serviceResourceIdentifier": "omsetningsrapportering",
    "party": "org/912345678" // Rørlegger AS
}
```

## 2. Etaten oppretter en dialog

```jsonc
// POST /dialogporten/api/v1/dialogs
{
    "id": "b13bb496-e6ab-4444-8442-e15be9f96e9c",
    "serviceResourceIdentifier": "omsetningsrapportering",     
    "party": "org/912345678",
    "status": "under-progress", 
    "content": {
        "body": [ { "code": "nb_NO", "value": "Skjema er klart for utfylling" } ],
        "title": [ { "code": "nb_NO", "value": "Omsetningsrapportering" } ],
    },  
    "actions": {
        "gui": [ 
            { 
                "action": "open",
                "type": "primary",
                "title": [ { "code": "nb_NO", "value": "Fyll ut" } ],
                "url": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c"
            },
            { 
                "action": "delete",
                "type": "secondary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],
                "isDeleteAction": true, 
                // Blir kalt fra Dialogporten med DELETE i bakkanal. Må returnere 204 eller en RFC7807-kompatibel feilmelding.
                "url": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c" 
            }
        ],
        "api": [ 
            { 
                "action": "submit", 
                "actionUrl": "https://api.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c",
                "method": "POST",
                "requestSchema": "https://schemas.etaten.no/tjenester/omsetningsrapportering/submission.json", 
                "responseSchema": "https://schemas.etaten.no/tjenester/omsetningsrapportering/receipt.json"
            },
            { 
                "action": "delete",
                "method": "DELETE",
                // Merk dette vil kreve at org gjør bakkanal-kall for å slette dialogen
                "actionUrl": "https://api.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": "person:12345678901",
            "activityDescription": [ { "code": "nb_NO", "value": "Opprettet dialog" } ]
        }
    ]
}
```

## 3. Regnskapsmedarbeider sender inn skjerma

(Dialogporten ikke involvert)

## 4. Skjema settes til signering i Dialogporten

```jsonc
// PATCH /dialogporten/api/v1/dialogs/b13bb496-e6ab-4444-8442-e15be9f96e9c
{
    "status": "signing", 
    "content": {
        "body": [ { "code": "nb_NO", "value": "Skjema er klart til signering av daglig leder." } ],
    },  
    "actions": {
        "gui": [ 
            { 
                "action": "sign",
                "authorizationRequirement": "general_manager_tasks", // Krever tilgang til "sign" på ressursen "general_manager_tasks"
                "type": "primary",
                "title": [ { "code": "nb_NO", "value": "Se over og signer" } ],
                "url": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/sign_gm?latest_submission=12031cd2-5eb5-4ddf-b4f9-9d30e403d92c"
            }
        ],
        "api": [ 
            { 
                "action": "sign",
                "authorizationRequirement": "general_manager_tasks", // Krever tilgang til "sign" på ressursen "general_manager_tasks"
                "actionUrl": "https://api.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/sign?latest_submission=12031cd2-5eb5-4ddf-b4f9-9d30e403d92c",
                "method": "POST",
                // Dialogporten har her ikke noe spesielt forhold til signering som handling. Her kan man kanskje se for seg 
                // en generell signeringsmekanisme som omfatter bruk av et felles schema.
                "requestSchema": "https://schemas.data.norge.no/esignering/personal_signing.json", 
                "responseSchema": "https://schemas.data.norge.no/esignering/personal_signing_response.json"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityId": "12031cd2-5eb5-4ddf-b4f9-9d30e403d92c",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": "person:12345678901",
            "activityDescription": [ { "code": "nb_NO", "value": "Skjema sendt inn og klar til signering" } ],
            "activityDetailsUrls": {
                // Her ligger innsendt skjema som kan hentes ned SBS for å vise hva som skal signeres
                "api": "https://api.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/submissions/12031cd2-5eb5-4ddf-b4f9-9d30e403d92c"
            }
        }
    ]
}

// Etaten sender også et eksplisitt varsel. 
// Dialogporten vil også alltid generere events for muteringer i Dialogporten, som også potensielt vil kunne generere varslinger.

// POST /dialogporten/api/v1/dialogs/b13bb496-e6ab-4444-8442-e15be9f96e9c/notifications
{
    // Valgfri identifikator som tjenestetilbyder kan oppgi. Kan brukes for å unngå duplikatutsending.
    "notificationId": "79921fae-631a-4f8b-8db5-e359f2336658",

    // Ressurs i XACML som det kreves tilgang til
    "authorizationRequirement": "general_manager_tasks",
    "sms": {
            "text": [ { "code": "nb_NO", "value": "Du har mottatt et signeringsoppdrag for Omsetningsrapportering. Logg inn i Dialogporten for å signere." } ],
            
            // Hvis avsender-felt skal være noe annet enn navn på tjenesteeier kan dette oppgis her. 
            // Valideres opp mot whitelist knyttet til autentisert org.
            "from": [ { "code": "nb_NO", "value": "Etaten" } ]
    },
    "email": {
            "subject": [ { "code": "nb_NO", "value": "Signeringsoppdrag for Omsetningsrapportering" } ],
            "template": "general-notification",
            "tokens": [
                { "name": "header", "value": [ { "code": "nb_NO", "value": "Signeringsoppdrag for Omsetningsrapportering" } ] },
                { "name": "body", "value": [ { "code": "nb_NO", "value": "Hei {{recipient.fullname}}!<br><br>Du er bedt om å signere for {{party.fullname}} i Dialogporten" } ] } 
            ]
    },
    "push": {
        // Basert på https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration/showNotification
        "title": [ { "code": "nb_NO", "value": "Signeringsoppdrag for Omsetningsrapportering" } ],
        "body": [ { "code": "nb_NO", 
            "value": "Du er bedt om å signere for {{party.fullname}} i Dialogporten" } ],
        // Valgfri URL som bruker blir sendt til hvis notifikasjonen klikkes på. 
        "notificationClickUrl": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/sign_gm?latest_submission=12031cd2-5eb5-4ddf-b4f9-9d30e403d92c"
    }
}

```

## 5. Daglig leder signerer

(Dialogporten ikke involvert)

## 6. Etaten mottar signeringen, og sender skjemaet til signering for regnskapsfører

```jsonc
// PATCH /dialogporten/api/v1/dialogs/b13bb496-e6ab-4444-8442-e15be9f96e9c
{
    "status": "signing", 
    "content": {
        "body": [ { "code": "nb_NO", "value": "Skjema er klart til signering av regnskapsfører." } ],
    },  
    "actions": {
        "gui": [ 
            { 
                "action": "sign",
                "authorizationRequirement": "accountant_tasks", // Krever tilgang til "sign" på ressursen "accountant_tasks"
                "type": "primary",
                "title": [ { "code": "nb_NO", "value": "Se over og signer" } ],
                "url": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/sign_ac?latest_submission=12031cd2-5eb5-4ddf-b4f9-9d30e403d92c"
            }
        ],
        "api": [ 
            { 
                "action": "sign",
                "authorizationRequirement": "accountant_tasks", // Krever tilgang til "sign" på ressursen "accountant_tasks"
                "actionUrl": "https://api.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/sign_ac?latest_submission=12031cd2-5eb5-4ddf-b4f9-9d30e403d92c",
                "method": "POST",
                // Dialogporten har her ikke noe spesielt forhold til signering som handling. Her kan man kanskje se for seg 
                // en generell signeringsmekanisme som omfatter bruk av et felles schema.
                "requestSchema": "https://schemas.data.norge.no/esignering/personal_signing.json", 
                "responseSchema": "https://schemas.data.norge.no/esignering/personal_signing_response.json"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityId": "71d7bfc2-741a-457d-8585-ba5af447c0ec",
            "relatedActivityId": "12031cd2-5eb5-4ddf-b4f9-9d30e403d92c", // Aktiviteten som refererer innsendingen som ble signert
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": "person:23456789012", // Daglig leder
            "activityDescription": [ { "code": "nb_NO", "value": "Mottatt signatur av daglig leder" } ]
        }
    ]
}

// POST /dialogporten/api/v1/dialogs/b13bb496-e6ab-4444-8442-e15be9f96e9c/notifications
{
    // Valgfri identifikator som tjenestetilbyder kan oppgi. Kan brukes for å unngå duplikatutsending.
    "notificationId": "a61c66f5-1dcf-4a27-8c17-633cd2eb8b8d",

    // Ressurs i XACML som det kreves tilgang til
    "authorizationRequirement": "accountant_tasks",
    "sms": {
            "text": [ { "code": "nb_NO", "value": "Du har mottatt et signeringsoppdrag for Omsetningsrapportering. Logg inn i Dialogporten for å signere." } ],
            
            // Hvis avsender-felt skal være noe annet enn navn på tjenesteeier kan dette oppgis her. 
            // Valideres opp mot whitelist knyttet til autentisert org.
            "from": [ { "code": "nb_NO", "value": "Etaten" } ]
    },
    "email": {
            "subject": [ { "code": "nb_NO", "value": "Signeringsoppdrag for Omsetningsrapportering" } ],
            "template": "general-notification",
            "tokens": [
                { "name": "header", "value": [ { "code": "nb_NO", "value": "Signeringsoppdrag for Omsetningsrapportering" } ] },
                { "name": "body", "value": [ { "code": "nb_NO", "value": "Hei {{recipient.fullname}}!<br><br>Du er bedt om å signere for {{party.fullname}} i Dialogporten" } ] } 
            ]
    },
    "push": {
        // Basert på https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration/showNotification
        "title": [ { "code": "nb_NO", "value": "Signeringsoppdrag for Omsetningsrapportering" } ],
        "body": [ { "code": "nb_NO", 
            "value": "Du er bedt om å signere for {{party.fullname}} i Dialogporten" } ],
        // Valgfri URL som bruker blir sendt til hvis notifikasjonen klikkes på. 
        "notificationClickUrl": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/sign_ac?latest_submission=12031cd2-5eb5-4ddf-b4f9-9d30e403d92c"
    }
}

```

## 7. Regnskapsfører signerer

(Dialogporten ikke involvert)

## 8. Etaten mottar siste signering, og lukker dialogen

```jsonc
// PATCH /dialogporten/api/v1/dialogs/b13bb496-e6ab-4444-8442-e15be9f96e9c
{
    "status": "completed",
    "actions": {
        "gui": [ 
            { 
                "action": "open",
                "type": "primary",
                "title": [ { "code": "nb_NO", "value": "Se kvittering" } ],
                "url": "https://www.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/finalreceipt"
            }
        ],
        "api": [ 
            { 
                "action": "open",
                "actionUrl": "https://api.etaten.no/tjenester/omsetningsrapportering/b13bb496-e6ab-4444-8442-e15be9f96e9c/finalreceipt",
                "method": "GET",
                "responseSchema": "https://schemas.etaten.no/tjenester/omsetningsrapportering/finalreceipt.json"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityId": "91c76996-c73d-4eee-bd6e-dd129640f7a2",
            "relatedActivityId": "12031cd2-5eb5-4ddf-b4f9-9d30e403d92c", // Aktiviteten som refererer innsendingen som ble signert
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": "person:31067890121", // Regnskapsfører
            "activityDescription": [ { "code": "nb_NO", "value": "Mottatt signatur av regnskapsfører" } ]
        },
        { 
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "closed",
            "externalType": "rf1305-closed",
            "activityDescription": [ { "code": "nb_NO", "value": "Rapportering godkjent og fullført" } ]
        }
    ]
}
```

## XACML-policy

Her vises en XACML-policy i et tenkt JSON-basert format. Se https://github.com/Altinn/altinn-studio/issues/5016 for mer diskusjon rundt XACML-formater.

```jsonc
[
    {
        "Subjects": [
            "role:UTINN",
            "role:DAGL",
        ],
        "Resources": [
            "serviceresource:omsetningsrapportering"
        ],
        "Actions": [
            "open",
            "submit",
            "delete"
        ],
        "Description": "Utfyller/innsender og daglig leder kan åpne, fylle ut og slette skjemaet",
        "IsDelegable": true
    },
    {
        "Subjects": [
            "role:DAGL",
        ],
        "Resources": [
            "serviceresource:omsetningsrapportering/general_manager_tasks",
        ],
        "Actions": [
            "sign"
        ],
        "Description": "Kun daglig leder kan signere på vegne av virksomheten",
        "IsDelegable": false
    },
    {
        "Subjects": [
            "role:A0239",
        ],
        "Resources": [
            "serviceresource:omsetningsrapportering/accountant_tasks",
        ],
        "Actions": [
            "sign"
        ],
        "Description": "Kun regnskapsfører med signeringsrett kan signere på vegne av registrert regnskapsfører",
        "IsDelegable": false
    }
]
```