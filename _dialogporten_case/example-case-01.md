---
menutitle: Eksempel 01
title: Eksempel-flyt basert på Skatts case ("superenkel innsending") med events
layout: page
toc: true
---

## Oversikt over steg

Dette er en variant hvor SBS-et initierer dialogen med å gjøre et kall direkte til Skatteetatens API. Overordnet er prosessen som beskrives under som sådan:

1. SBS sender inn noe 
2. Skatt sender svar tilbake (som varsles) med forespørsel om mer info
3. SBS sender inn mer
4. Skatt sender svar og lukker saken 

{% include note.html type="warning" content="Modellene som vises under er forenklede ift de reelle modellene som må benyttes" %}

## 1. SBS gjør en innsending

SBS sender inn noe på vegne av organisasjonsnummer 91234578 VIRKSOMHET AS. Her er det delt opp i to kall, men det er helt vilkårlig.

```
POST api.skatteetaten.no/skattemeldingsdialog/ 
{
  ... en eller annen modell
}
// (returnerer dialogId: f4e6df3c-7434-44c3-875e-8dca1cdf0b20)

POST skatt.api.no/skattemeldingsdialog/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/innsending/ 
{
    ... en eller annen modell
}
// (returnerer "dialogElementId" (domenespesifikt begrep) på innsendt skattemelding: 58dafb71-8838-4077-84e2-ecd3ea069812)
```
    
## 2. Skatteetaten mottar innsending  

Dialogen opprettes i DP, og settes i en tilstand som indikerer at den er under behandling hos Skatt. 

```jsonc 
// Merk! Forenklet modell
//POST dialogporten.no/api/v1/serviceowner/dialogs/ 
{
    "id": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "party": "org/91234578",
    "serviceResource": "super-simple-service",
    "status": "waiting",
    "actions": [ "open", "cancel-application", "read-more-about-process" ],
    "activityHistory": [
        {            
            // Vilkårlig ID. Må ikke oppgis, men anbefales for å sikre idempotens.
            "activityId": "1470986f-6d48-4caa-916e-0dc77e08bc8b", 
            "activityType": "submission",
            "activityDescription": "Søknad sendt til behandling",
            "dialogElementId": "58dafb71-8838-4077-84e2-ecd3ea069812",
            "activityDetailsUrl": "<url-referanse til innsending>"
        }
    ]
}
```

## 3. Dialogporten sender events

Opprettelsen av dialogen medfører at det genereres en eller flere events. Denne følger [external event-modellen](https://docs.altinn.studio/technology/architecture/capabilities/runtime/integration/events/#example-4--external-event) i Altinn Events.

```jsonc
{
    "specversion": "1.0",
    "id": "91f2388f-bd8c-4647-8684-fd9f68af5b15", // unik event-id
    "type": "dialogporten.dialog.created.v1",
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service", // serviceResource
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20", // dialog-id
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20"
}

```

Ethvert nytt innslag i activityHistory genererer også events. Siden dette ble oppgitt i POST-en over, genereres en event:

```jsonc
{
    "specversion": "1.0",
    "id": "00301bf0-1497-426c-ab0a-b52636853139",
    "type": "dialogporten.dialog.activity.submission.v1", // siste ledd svarer til activityType
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/activityhistory/1470986f-6d48-4caa-916e-0dc77e08bc8b",    
    // Under er felter hentet fra activityHistory-innslag som mulliggjør at et SBS kan agere uten å måtte slå opp dialogen eller aktivitetslogg-innslaget
    "data": { 
        "activityId": "1470986f-6d48-4caa-916e-0dc77e08bc8b",
        "extendedActivityType": "SKE-1234-received-precheck-ok",
        "dialogElementId": "58dafb71-8838-4077-84e2-ecd3ea069812"
    }
} 
```

## 4. Medarbeider åpner tilfeldigvis dialogen

Mens Skatteetaten behandler oppgaven går en eller annen ansatt for VIRKSOMHET AS inn Felles arbeidsflate (GUI) og finner dialogen (fordi vedkommende er autorisert gjennom policy), som han/hun åpner. Dette innebærer at Dialogporten gjør et innslag i activityHistory på dialogen ...


```jsonc
{ 
    "activityId": "387cfaa8-8113-43c1-a457-603be651ecb9", // Dialogporten genererer en activityId
    "activityDateTime": "2022-12-01T10:15:00.000Z",
    "activityType": "seen",
    "performedBy": [ { "code": "nb_NO", "value": "Anne Olsen" } ],,
}
```

... som igjen medfører at det genereres en hendelse ...

```jsonc
{
    "specversion": "1.0",
    "id": "6b19629c-79d9-45cf-884d-601b7d9d2041",
    "type": "dialogporten.dialog.activity.seen.v1", 
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/activityhistory/387cfaa8-8113-43c1-a457-603be651ecb9",    
    "data": { 
        "activityId": "387cfaa8-8113-43c1-a457-603be651ecb9"
    }
}   
```

`seen`-aktiviteter oppstår første gang dialog åpnes/lastes etter opprettelse, og første gang dialogen åpnes/lastes etter et `feedback`-innslag er lagt i aktivitetsloggen. Tjenestetilbyder har tilgang til å abonnerere på disse hendelsene. Merk at dette ikke må forstås som at elementet er _lest_, da dette er informasjon Dialogporten ikke sitter på.

## 5. Skatt har behandlet innsendingen

Saksbehandlingen har avdekket at det er behov for å innhente ytterligere opplysninger, så skatt sender et svar hvor det etterspørres mer informasjon:

```jsonc    
// Merk! Forenklet modell
// PATCH dialogporten.no/api/v1/serviceowner/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20
{
    "status": "in-progress",
    "content": "Vi har behandlet søknaden din, og ser vi trenger mer opplysninger.",
    // ny action "fill-in" som leder til et nytt innsendingssteg
    "actions": [ "fill-in", "open", "cancel-application", "read-more-about-process" ],  
    "activityHistory": [
        {
            "activityId": "a4df1787-a6d1-453e-8ab1-47c06e7d90bd",
            "activityType": "feedback",
            "relatedActivityId": "1470986f-6d48-4caa-916e-0dc77e08bc8b", // peker på aktiviteten som beskrve innsendingen
            "extendedActivityType": "form-rf1234-required", // maskinlesbar
            "activityDescription": "Behandling foretatt, mer informasjon forespurt",
            "activityDetailsUrl": "<evt. referanse som gir mer informasjon om hva som har skjedd>"
        }
    ]
}
```

## 6. Dialogporten genererer events basert på endringen
 

```jsonc
// activityHistory-innslag for "feedback"
{
    "specversion": "1.0",
    "id": "1bbf3ebd-8a4a-44c6-a579-e3a97c10db27",
    "type": "dialogporten.dialog.activity.feedback.v1",
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/activityhistory/a4df1787-a6d1-453e-8ab1-47c06e7d90bd",    
    "data": { 
        "activityId": "a4df1787-a6d1-453e-8ab1-47c06e7d90bd",
        "extendedActivityType": "form-rf1234-required",
        "relatedActivityId": "1470986f-6d48-4caa-916e-0dc77e08bc8b"
    }
} 

// Event for endring av dialog
{
    "specversion": "1.0",
    "id": "522a658c-167f-4600-80f1-2f0782cfcec1",
    "type": "dialogporten.dialog.updated.v1", // status ble endret
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20"
}

```

## 7. SBS mottar hendelse, laster dialogen, og sender inn ytterligere opplysninger

SBS-et har et abonnement som plukker opp at det har kommet en tilbakemelding i dialogen. SBS-et ser utfra `externalType` i evetnet at det er behov for å sende inn et ytterligere skjema (tjenestespesifikk logikk).  En medarbeider varsles, som laster/åpner dialogen gjennom SBS-et, 

```
GET https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/
```

og ser at det er kommet et svar med forespørsel om mer informasjon. Siden det er mottatt en feedback siden forrige gang dialogen ble åpnet, vil lastingen av dialogen (forrige request) igjen føre til at det genereres en hendelse som i trinn 4.

Actionen 'fill-in' som har dukket opp gir SBS-et informasjon om hvilket endepunkt de ytterligere opplysningene skal sendes til, og kan også inneholde informasjon om forventet datamodell (dette er typisk noe SBS-et allerede har kjennskap til)

Gjennom SBS-et sender medarbeideren inn opplysningene som manglet:

```
POST skatt.api.no/skattemeldingsdialog/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/ytterligere-innsending/ 
{
    ... en eller annen modell
}
// returnerer en eller annen modell, og evt en identifikator for innsendingen
```

## 8. Skatteetaten mottar ytterligere opplysninger

Skatteetaten mottar innsendingen, som valideres maskinelt. Dialogen kan nå avsluttes. To innslag i aktivitetsloggen legges inn samtidig. 

```jsonc
//PATCH dialogporten.no/api/v1/serviceowner/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20
{
    "status": "completed",
    "content": "Søknaden er behandlet og vedtaksbrev er vedlagt.",
    // ny action "appeal" som starter en klageprosess
    "actions": [ "open", "appeal" ],
    "attachments": [
        {
            "displayName": "Vedtaksbrev",
            "sizeInBytes": 123456,
            "contentType": "application/pdf",            
            "url": "https://api.skatt.no/attachments/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/vedtaksbrev.pdf",
        }
    ],
    "activityHistory": [
        {            
            "activityId": "21241c7e-819f-462b-b8a4-d5d32352311a", 
            "activityType": "submission",
            "extendedActivityType": "additional-info-received",
            "activityDescription": "Søknad med ytterligere opplysninger mottatt",
            "dialogElementId": "54ae8387-7320-4693-8423-0ceb0efaf5fa",
            "activityDetailsUrl": "<url-referanse til ytterligere innsending>"
        },
        {
            "activityId": "5d70003d-3018-4c15-a49c-4fa3d51da1fa",
            "activityType": "closed",
            "activityDescription": "Sak avsluttet",
            "activityDetailsUrl": "<evt. referanse som gir mer informasjon om vedtak, klageadgang etc>"
        }
    ]
}
```

## 9. Dialogporten genererer hendelser basert på de nye innslagene i activityHistory samt endringene

```jsonc
// activityHistory-innslag for "submission"
{
    "specversion": "1.0",
    "id": "1bbf3ebd-8a4a-44c6-a579-e3a97c10db27",
    "type": "dialogporten.dialog.activity.submission.v1",
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/activityhistory/21241c7e-819f-462b-b8a4-d5d32352311a",    
    "data": { 
        "activityId": "21241c7e-819f-462b-b8a4-d5d32352311a", // peker til vår 
        "extendedActivityType": "additional-info-received",
        "dialogElementId": "54ae8387-7320-4693-8423-0ceb0efaf5fa"
    }
} 

// activityHistory-innslag for "closed"
{
    "specversion": "1.0",
    "id": "1bbf3ebd-8a4a-44c6-a579-e3a97c10db27",
    "type": "dialogporten.dialog.activity.closed.v1",
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/activityhistory/5d70003d-3018-4c15-a49c-4fa3d51da1fa",    
    "data": { 
        "activityId": "5d70003d-3018-4c15-a49c-4fa3d51da1fa"
    }
} 

// Event for endring av dialog
{
    "specversion": "1.0",
    "id": "9f584a96-ab05-476d-aa48-aaa8248802e6",
    "type": "dialogporten.dialog.updated.v1",
    "time": "2023-02-20T08:00:06.4014168Z",
    "resource": "urn:altinn:resource:super-simple-service",
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    "subject": "org/91234578",
    "source": "https://dialogporten.no/api/v1/enduser/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20"
}


```