---
menutitle: Eksempel 02
title: Eksempel-flyt basert på Primærnæringsoppgave Egg
layout: page
toc: true
---

## Introduksjon

Denne er en mulig implementasjon av eksemplet mottat fra Skatteetaten "Innsending av primærnæringsoppgave Egg". Denne tar utgangspunkt i alternativ 1, som indikerer at hver innsending/utsending er distinkte aktiviteter og følgelig bør ha et innslag i aktivitetshistorikken.

Partene er Nortura og Skatteetaten.

## Oversikt over steg

1. Nortura instansierer en dialog som forbereder et oppgitt antall oppgaver som skal sendes inn (50). Dette kan gjøres direkte mot EggAPI, her er det eksemplifisert med et kall til instansierings API-et
2. EggAPI oppretter en dialog som indikerer at man er klare for å motta de 50 oppgavene
3. Nortura kjenner til protokollen fra før, og trenger ikke hente noe fra Dialogporten, og sender inn alle 50 oppgavene én og én (eller samlet, det er opp til EggAPI)
4. Etter at det er mottatt 50 oppgaver, som teknisk valideres i EggAPI, gis beskjed i Dialogporten på mottatte oppgaver
5. Etter en stund oppdager Nortura at det er feil på én av innsendingene, og foretar en ny innsending for å korrigere
6. EggAPI gir beskjed i Dialogporten på at korrigering er mottatt
7. Det går en stund, og fagsystemet oppdager en feil i et par av innsendingnee, og gir beskjed om dette i Dialogporten
8. Nortura sender inn korrigering
9. EggAPI gir beskjed i Dialogporten på at korrigering er mottatt
10. EggAPI gir beskjed om at rapportering er godkjent, og lukker dialogen

{% include note.html type="warning" content="Modellene som vises under er forenklede ift de reelle modellene som må benyttes. Events som genereres er utelatt - for eksempler på disse se case-01" %}

## 1. Nortura instansierer en dialog

```jsonc
// POST /dialogporten/api/v1/serviceowner/instantiate
{
    "serviceResource": "rf1305_purchases_from_primary_industry_eggs",
    "party": "org/938752648",
    "inputParameters": { 
        "antallOppgaver": 50
    }
}
```

## 2. EggAPI oppretter en dialog

```jsonc
// POST /dialogporten/api/v1/serviceowner/dialogs
{
    "id": "e0300961-85fb-4ef2-abff-681d77f9960e",
    "serviceResource": "rf1305_purchases_from_primary_industry_eggs",     
    "party": "org/938752648",
    "status": "in-progress", 
    "content": {
        "body": [ { "code": "nb_NO", "value": "Vi er klare for å motta de 50 oppgavene du har oppgitt at du vil rapportere" } ],
        "title": [ { "code": "nb_NO", "value": "RF-1305 Kjøp fra primærnæring – egg" } ],
    },  
    "actions": {
        "gui": [ 
            { 
                "action": "open",
                "priority": "primary",
                "title": [ { "code": "nb_NO", "value": "Fyll ut 50 oppgaver i vår GUI-tjeneste" } ],
                "url": "https://www.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e"
            },
            { 
                "action": "delete",
                "priority": "secondary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],
                "isDeleteAction": true, 
                // Blir kalt fra Dialogporten med DELETE i bakkanal. Må returnere 204 eller en RFC7807-kompatibel feilmelding.
                "url": "https://www.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e" 
            }
        ],
        "api": [ 
            { 
                "action": "submit", 
                "actionUrl": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e",
                "method": "POST",

                // noe ala "https://www.altinn.no/api/metadata/formtask/3153/140522/forms/3713/36442/xsd´,
                // som enten 
                // - forventer alle forsendingene på én gang (her kan schema tom. genereres dynamisk som har en minItems/maxItems satt til 50)
                // - forventer én og én
                "requestSchema": "https://schemas.skatteetaten.no/tjenester/rf1305/submission.json", 
                "responseSchema": "https://schemas.skatteetaten.no/tjenester/rf1305/receipt.json"
            },
            { 
                "action": "delete",
                "method": "DELETE",

                // Merk dette vil kreve at org gjør bakkanal-kall for å slette dialogen
                "actionUrl": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityId": "fc6406df-6163-442a-92cd-e487423f2fd5",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": [ { "code": "nb_NO", "value": "Foretaket AS" } ],
            "extendedActivityType": "rf1305-received-precheck-ok",
            "dialogElementId": "936f4753-6271-431a-b618-208ac66ce0ef",
            "activityDescription": [ { "code": "nb_NO", "value": "Opprettet dialog for rapportering av 50 oppgaver" } ],

            "activityDetailsUrls": {
                "api": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/submissions/936f4753-6271-431a-b618-208ac66ce0ef"
            }
        }
    ]
}
```

## 3. Nortura sender inn oppgavene

(Dialogporten ikke involvert)

## 4. Det gis beskjed i Dialogporten på mottatte oppgaver

```jsonc
// PATCH /dialogporten/api/v1/serviceowner/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "status": "waiting", 
    "content": {
        "body": [ { "code": "nb_NO", "value": "Vi har mottatt alle 50 oppgavene fra deg, og jobber med saksbehandling - her er det bare å smøre seg med tålmodighet! Du kan sende inn en korrigering hvis du ønsker." } ],
    },  
    "actions": {
        "gui": [ 
            { 
                "action": "open",
                "priority": "primary",
                "title": [ { "code": "nb_NO", "value": "Send inn korrigering i vår GUI-tjeneste" } ],
                "url": "https://www.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/correction/936f4753-6271-431a-b618-208ac66ce0ef"
            },
            { 
                "action": "open", // Kan ha samme action, Dialogporten bryr seg ikke - action har bare med tilgangskontroll å gjøre
                "priority": "secondary",
                "title": [ { "code": "nb_NO", "value": "Se status på saksbehandling" } ],
                "url": "https://www.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/status"
            },
        ],
        "api": [ 
            { 
                "action": "submit",
                "actionUrl": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/correction/936f4753-6271-431a-b618-208ac66ce0ef",
                "method": "POST",
                // Her kan man kanskje ha et egen JSON schema for korrigeringer
                "requestSchema": "https://schemas.skatteetaten.no/tjenester/rf1305/submission_correction.json", 
                "responseSchema": "https://schemas.skatteetaten.no/tjenester/rf1305/receipt_correction.json"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityId": "7f91fb5e-4c79-4c01-82aa-84911ef13b75",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": [ { "code": "nb_NO", "value": "Foretaket AS" } ],
            "extendedActivityType": "rf1305-received-precheck-ok",
            "dialogElementId": "d0d0b171-5bbf-44fa-8785-737ed46e7ae6",
            "activityDescription": [ { "code": "nb_NO", "value": "50 oppgaver er mottatt og maskinelt kontrollert" } ],

            "activityDetailsUrls": {
                // Her kan det ligge en eller annen liste over de mottatt forsendelsene, med egne identifikatorer
                "api": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/submissions/d0d0b171-5bbf-44fa-8785-737ed46e7ae6"
            }
        }
    ]
}
```

## 5. Nortura sender inn korrigeringer

(Dialogporten ikke involvert)

## 6. EggAPI sender kvittering på at korrigering er mottatt

```jsonc
// PATCH /dialogporten/api/v1/serviceowner/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "activityHistory": [
        { 
            "activityId": "20c94e10-b95d-4cd0-b469-b4caa4532c4e",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": [ { "code": "nb_NO", "value": "Foretaket AS" } ],
            "extendedActivityType": "rf1305-correction-received-precheck-ok",
            "dialogElementId": "4d63ec64-4dd2-40f9-bc72-353ea45bb0cc",
            "activityDescription": [ { "code": "nb_NO", "value": "4 korrigering(er) er mottatt og maskinelt kontrollert" } ],
            "activityDetailsUrls": {
                // Her kan det ligge en eller annen liste over mottatte korrigeringer
                "api": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/submissions/4d63ec64-4dd2-40f9-bc72-353ea45bb0cc"
            }
        }
    ]
}
```

## 7. Fagsystemet oppdager feil, og sender feilmelding

```jsonc
// PATCH /dialogporten/api/v1/serviceowner/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "activityHistory": [
        { 
            "activityId": "4d005d87-4ba8-4bf7-bcac-7862744a3180",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "error",
            "extendedActivityType": "rf1305-submissions-error",
            "dialogElementId": "4e6c9c96-435a-434d-a415-723cbbc6f3d4",
            "relatedActivityId": "7f91fb5e-4c79-4c01-82aa-84911ef13b75", // Enten den opprinnelige innsendingen, eller korrigeringen
            "activityErrorCode": "errors-detected-in:12,34",
            "activityDescription": [ { "code": "nb_NO", "value": "Feil ble funnet i oppgave nr: 12, 34. Vennligst send inn korrigeringer." } ],
            "activityDetailsUrls": {
                // Her kan det ligge en eller annen liste over oppgaver som er feil, med ytterligere opplysninger
                "api": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/error/4e6c9c96-435a-434d-a415-723cbbc6f3d4"
            }
        }
    ]
}
```

## 8. Nortura sender inn korrigeringer

(Dialogporten ikke involvert)

## 9. EggAPI sender kvittering på at korrigering er mottatt

```jsonc
// PATCH /dialogporten/api/v1/serviceowner/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "activityHistory": [
        { 
            "activityId": "da6087e9-ec7c-4a30-9605-2b753a104c21",
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "submission",
            "performedBy": [ { "code": "nb_NO", "value": "Foretaket AS" } ],
            "extendedActivityType": "rf1305-correction-received-precheck-ok",
            "dialogElementId": "1fe9fd6d-a544-4c7e-a989-1857613a90a8",
            "activityDescription": [ { "code": "nb_NO", "value": "2 korrigering(er) er mottatt og maskinelt kontrollert" } ],
            "activityDetailsUrls": {
                // Her kan det ligge en eller annen liste over mottatte korrigeringer
                "api": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/submissions/1fe9fd6d-a544-4c7e-a989-1857613a90a8"
            }
        }
    ]
}
```

## 10. EggAPI lukker dialogen

```jsonc
// PATCH /dialogporten/api/v1/serviceowner/dialogs/e0300961-85fb-4ef2-abff-681d77f9960e
{
    "status": "completed",
    "actions": {
        "gui": [ 
            { 
                "action": "open",
                "priority": "primary",
                "title": [ { "code": "nb_NO", "value": "Se kvittering" } ],
                "url": "https://www.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/finalreceipt"
            }
        ],
        "api": [ 
            { 
                "action": "open",
                "actionUrl": "https://api.skatteetaten.no/tjenester/rf1305/e0300961-85fb-4ef2-abff-681d77f9960e/finalreceipt",
                "method": "GET",
                "responseSchema": "https://schemas.skatteetaten.no/tjenester/rf1305/finalreceipt.json"
            }
        ]
    },
    "activityHistory": [
        { 
            "activityDateTime": "2022-12-01T10:00:00.000Z",
            "activityType": "closed",
            "extendedActivityType": "rf1305-closed",
            "activityDescription": [ { "code": "nb_NO", "value": "Rapportering godkjent og fullført" } ]
        }
    ]
}
```