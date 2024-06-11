# Ny modell for Dialogporten

Dette er et forslag til endringer i dialog-modellen for å bedre understøtte behovet for å kunne bygge dialogen basert på en liste med inn- og utsendinger, med mindre (tvunget) fokus på aggregatet. Det er fremdeles mulig (og anbefalt) å oppdatere aggregatet mht innhold, actions, status etc, men dialogen kan også utvides gjennom enkelt-operasjoner for å legge til inn- og utsendinger som selv har innhold ("content") og egne vedlegg, og kan kobles til egne autorisasjonsregler.  
 
## Highlights

- "Dialogelement" erstattes av "forsendelse" (engelsk: "transmission")
    - Mindre forvirrende - "dialogelement" er brukt om enkeltdialoger, spesielt i kontekst av listevisning (minner om gamle "reportee-element"?)
    - Forsendelser representerer inn- eller utsendinger
    - Bruk av forsendelser er opt-in
    - Vedlegg (som er lenkene til faktisk payload) er egne strukturer innenfor forsendelsene. Vedlegg kan også defineres på dialognivå, som vil bli brukt for dialoger som ikke definerer forsendelser, eller digital post.
    - Forsendelser er ikke muterbare. Dette for å understøtte behovet for å kunne føre en fullstendig og uforanderlig logg over kommunikasjonen
    - Front Channel Embeds er tatt inn i content (ikke relevant for Skatt i denne sammenhengen)
- Aktivitetslogg trenger ikke populeres ifm opprettelse av forsendelser.     
    - Populeres med aktiviteter som ikke er direkte knyttet til _opprettelse_ av en forsendelse (f.eks. åpning, signering, betaling)
    - Kan (men må ikke) referere en forsendelse (som nå er ikke-muterbar; disse referansene kan dermed ikke lenger knekkes)
    - Visualisering i arbeidsflate/SBS aggregereres av
      - Forsendelsene, som i seg selv representerer en logg. 
      - Varslingslogg fra varslingsprodukt (bruk av "associatedWith" for å hente relevante varslingsordrer)
      - Aktivitetsloggen

Dette muliggjør:

- Skatt kan gjøre inn- og utsendinger tilgjengelig i Dialgporten uten å måtte forholde seg til dialog-aggregatet
- En sterkere visualisering av inn- og utsendinger i Arbeidsflate. Gjennom "relatedTransmissionId" vil man kunne knytte inn/utsendings-tråder sammen.
    - I eksemplet under er det to "tråder" (rot i forsendelse som ikke har noen "relatedTransmissionId")
    - Den første tråden er det enkleste caset, hvor det er en "transmission" for utsendingen (godkjenningen) som oppgir "relatedTransmissionId" for innsendingen, og danner dermed et 
      innsending/utsendingspar
    - Det andre eksemplet viser har først et tilsvarende innsending/utsending-par, men har også ytterligere utsendinger som lenkes videre. Her kan man se for seg at evt. korrigeringer
      som sendes som følge av avvisningsmeldingen også kan knyttes i samme "tråd"

Andre konsekvenser av dette:

- Vi fjerner PUT og DELETE på /api/v1/serviceowner/dialogs/{dialogId}/transmission/{elementId}, men beholder GET på liste/enkeltforsende og (for tjenesteeiere) POST
- Vi erstatter events for dialogelementer med tilsvarende events for forsendelser (ink dyplenker)


## Eksempel (enduser-DTO)
```jsonc
{
    "id": "{{dialog-guid}}", // valgfri egenoppgitt identifikator (guid)
    "org": "ske",
    "serviceResource": "urn:altinn:resource:ske_tredjepartsopplysninger_boligsameier",
    "party": "urn:altinn:organization:identifier-no:912345678",
    "createdAt": "2024-01-12T15:43:23.2348051", 
    "updatedAt": "2024-03-08T08:01:03.6301023", // Utledet av siste endring
    "dueAt": "2025-03-01T23:59:59", // Frist. Kan være 31. jan inntil første innsending er foretatt.
    // Status kan i "verste fall" stå som "New" hele tiden. Vil da ligge i brukers innboks og ikke 
    // "Under arbeid", så vi må tilby mekanismer for å la den enkelte bruker (som dette ikke er relevant 
    // for) skjule dialogen
    "status": "New", 
    "content": [
        // Formuleres generisk slik at den kan står uforandret gjennom prosessen
        { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Rapportering av tredjepartsopplysninger for boligsameie" } ] },
        { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Sameier med 9 eller flere boligseksjoner skal levere opplysninger om sameiers andel av felles inntekter, utgifter, formue og gjeld. " } ] },
        { "type": "AdditionalInfo", "value": [ { "cultureCode": "nb_NO", "value": "Frist for innsending er 31. januar. Du vil motta fortløpende tilbakemeldinger på oppgaver du sender inn, og har mulighet til å korrigere disse frem til 1. mars" } ] }
        /*
        // Vi flytter front channel embeds inn i content (ikke relevant for dette eksemplet)
        { "type": "MainContentReference", "mediaType": "application/vnd.dialogporten.frontchannelembed+json;type=markdown", "value": [ { "cultureCode": "nb_NO", "value": "https://someendpoint-supporting-cors-and-dialogtokens.com/somemarkdown_nb.md" } ] }
        */
    ],
    // Actions kan også være uforandret gjennom hele prosessen. 
    "guiActions": [
        {
            "action": "write",
            "isAuthorized": true,
            "url": "https://skatteetaten.no/dyplenke-til-gui-for-innsending-av-oppgaver-for-denne-saken",
            "priority": "Primary",
            "title": [ { "value": "Gå til utfylling av oppgaver", "cultureCode": "nb-no" } ]
        }
    ],
    "apiActions": [
        {
            "action": "write",
            "isAuthorized": true,   
            "type": "urn:ske:fastsetting:innsamling:boligsameie:v2",
            "endpoints": [
                {
                    "version": "v2.1.1",
                    "url": "https://api.skatteetaten.no/endepunkt-for-mottak-eller-korrigering-av-ordinærleveranse",
                    "httpMethod": "POST",
                    "accepts": [ "application/xml" ],
                    "requestSchema": "https://www.skatteetaten.no/contentassets/bacb3ec210ce4ec19aa129f7f1ba5367/v2_1_1/boligsameie_v2_1.xsd"
                }
            ]
        }
    ],
    // Vedlegg er ikke lenger en type dialogelement, men er en egen struktur. 
    // Vedlegg kan defineres på dialog- og forsendelsenivå
    "attachments": [], 
    "dialogToken": "ey3fk24f...",
    // Tidligere dialogelementer, nå forsendelser. Ikke lenger muterbare, men skal representere all 
    // formell kommunikasjon mellom partene
    "transmissions": [
        {
            // Første innsending
            "id": "{{transmissionId-innsending-1}}",
            "isAuthorized": true,          
            "createdAt": "2024-01-12T16:23:23.4398572",
            // Indikerer at dette er en "innsending", siden det kommer fra brukeren. Den andre foreløpig
            // definerte rollen er "serviceOwner", men her kan det potensielt komme flere
            "sender": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "content": [ /* Kanskje kun godkjenne Title og Summary her (og ikke AddtionalInfo, SenderName etc)? */ 
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Innlevering av tredjepartsopplysninger" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Tredjepartsopplysningene ble levert" } ] }
            ],
            // Vedleggene representerer her de forskjellige oppgavene som inngår i en innsending
            "attachments": [
                {
                    "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 56 - Ola Huseier", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-1-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-1-attachment-1.json",  "mimeType": "application/json" }
                    ]
                },
                {
                    "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 57 - Kari Huseier", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-1-attachment-2.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-1-attachment-2.json",  "mimeType": "application/json" }
                    ]
                }
            ]                       
        },
        {
            // Svar på første innsending.             
            "id": "{{transmissionId-utsending-1}}",
            "isAuthorized": true,
            "relatedtransmissionId": "{{transmissionId-innsending-1}}", // knyttet til innsending
            "createdAt": "2024-01-12T16:23:46.8472964",            
            "sender": {
                "role": "serviceOwner" // implisitt er "name" tjenesteeier (gitt av "org" på dialognivå)
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Godkjenning av tredjepartsopplysninger" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Innsendte opplysninger ble godkjente" } ] }
            ],            
            "attachments": [
                {
                    "displayName": [ { "value": "Godkjenningskvittering", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-1-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-1-attachment-1.json",  "mimeType": "application/json" }
                    ]
                }
            ]
            /* 
            // Her er det mulig å ha et flagg for å indikere at Dialogporten omskrive alle URL-er for å 
            // logge at noe er åpnet i aktivitetsloggen, og redirecte med HTTP-kode 302 til faktisk URL. 
            // Vil i begge tilfeller endre "updatedAt" på aggregat. Som default lar vi tjenesteeier
            // selv måtte stå for å populere dette

            "openedLogMode": "manual", // "automatic"
            
            */ 
        },
        {
            // Andre innsending 
            "id": "{{transmissionId-innsending-2}}",
            "isAuthorized": true,           
            "createdAt": "2024-01-14T13:12:54.3871523",
            "sender": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Innlevering av tredjepartsopplysninger" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Innlevering av tredjepartsopplysningene ble levert" } ] }
            ],
            "attachments": [
                {
                    // erstatning av tidligere oppgave
                    "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 56 - Ola Huseier", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-2-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-2-attachment-1.json",  "mimeType": "application/json" }
                    ]
                },
                {
                    "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 58 - Frank Huseier", "cultureCode": "nb-no" } ], // ny oppgave som ble glemt ved forrige innsending
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-2-attachment-2.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-2-attachment-2.json",  "mimeType": "application/json" }
                    ]
                }
            ]
        },
        {
            // Svar på andre innsending
            "id": "{{transmissionId-utsending-2}}",
            "isAuthorized": true,
            "relatedtransmissionId": "{{transmissionId-innsending-2}}", // knyttet til andre innsending
            "createdAt": "2024-01-14T13:12:54.3871523",
            "sender": {
                "role": "serviceOwner"
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Avvisning av tredjepartsopplysninger" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Innsendte opplysninger inneholdt feil, vennligst send korrigering" } ] }
            ],
            "attachments": [
                {
                    "displayName": [ { "value": "Detaljer om feil i innsending", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-2-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-2-attachment-1.json",  "mimeType": "application/json" }
                    ]
                }
            ]           
        },
        {
            // Bruker har ikke sendt ny korrigering etter siste melding om at siste innsending ble 
            // avvist, påminnelse sendes
            "id": "{{transmissionId-utsending-3}}",
            "isAuthorized": true,
            // knyttet til utsending vedr avvisning
            "relatedtransmissionId": "{{transmissionId-utsending-2}}", 
            // overstyrt ressurs som legges til grunn for tilgangsstyring.
            "authorizationAttribute": "urn:altinn:resource:ske_tredjepartsopplysninger_boligsameier_purring",  
            "createdAt": "2024-01-21T13:12:54.3871523",
            "sender": {
                "role": "serviceOwner" // implisitt er "name" tjenesteeier
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Påminnelse om behov for korrigering" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Purring: innsendte opplysninger inneholdt feil, vennligst send korrigering" } ] }
            ],
            "attachments": [
                {
                    "displayName": [ { "value": "Detaljer om feil i innsending", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-3-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-3-attachment-1.json",  "mimeType": "application/json" }
                    ]
                }
            ]
        },
        {
            // Varsel om tvangsmulkt
            "id": "{{transmissionId-utsending-4}}",
            // Den innloggede brukeren har ikke tilgang til å lese dette (har ikke "read" på
            // "urn:altinn:resource:ske_tvangsmulkt" for denne organisasjonen), men får se metadata
            // Arbeidsflate vil vise dette grået ut el.l.
            "isAuthorized": false, 
            // knyttet til purring 
            "relatedtransmissionId": "{{transmissionId-utsending-3}}", 
            // overstyrt ressurs som legges til grunn for tilgangsstyring
            "authorizationAttribute": "urn:altinn:resource:ske_tvangsmulkt", 
            "createdAt": "2024-03-01T08:01:03.6301023",
            "sender": {
                "role": "serviceOwner"
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Varsel om tvangsmulkt" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Korrigering ble ikke sendt innenfor frist." } ] }
            ],
            "attachments": [
                {
                    "displayName": [ { "value": "Varsel om tvangsmulkt", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-4-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-4-attachment-1.json",  "mimeType": "application/json" }
                    ]
                }
            ]
        },
        {
            // Vedtak om tvangsmulkt
            "id": "{{transmissionId-utsending-5}}",
            "isAuthorized": false, 
            // knyttet til utsending "varsel om tvangsmulkt"
            "relatedtransmissionId": "{{transmissionId-utsending-4}}", 
            "authorizationAttribute": "urn:altinn:resource:ske_tvangsmulkt",
            "createdAt": "2024-03-08T08:01:03.6301023",
            "sender": {
                "role": "serviceOwner"
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Vedtak om tvangsmulkt" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Boligsameiet er ilagt tvangsmulkt pga manglende rapportering." } ] }
            ],
            "attachments": [
                {
                    "displayName": [ { "value": "Vedtak om tvangsmulkt", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-5-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-5-attachment-1.json",  "mimeType": "application/json" }
                    ]
                }
            ]
        }
    ],
    // Aktivitetslogg vil i visningsøymed kunne og se på forsendelsene, hvor createdAt/sender/
    // content.title legges til grunn. På denne måten trenger man ikke å legge inn samme informasjon
    // to steder. Andre aktiviteter som ikke er relatert til _opprettelsen_ av en konkret inn/utsending 
    // kan populeres av tjenesteeier her. 
    
    // Varslingslogg blir ikke populert her, men er tilgjengelig for arbeidsflate/SBS gjennom
    // varslingsproduktets API-er, som vil eksponere en egen varslingslogg (hvem som er blir varslet om 
    // hva når)
    "activityLog": [
        
        // Dialog opprettet
        {
            "createdAt": "2024-01-12T15:43:21.3348051",
            "performedBy": { // Samme datastruktur som "sender" på forsendelse/transmission
                "role": "user",
                "name": "Ola Nordmann",
            },
            // Her kan vi ha flere typer, f.eks. at noe er signert eller betalt
            "type": "dialogCreated" 
        },

        // Utsending nr 1 åpnet av Kari Kollega
        {
            "createdAt": "2024-01-12T17:02:53.9857230", 
            "performedBy": {
                "role": "user",
                "name": "Kari Kollega",
            },
            "type": "transmissionOpened",
            "relatedTransmissionId": "{{transmissionId-utsending-1}}"
        },

        // Utsending nr 1 (godkjenning) åpnet av Ola Nordmann
        {
            "createdAt": "2024-01-12T19:31:12.8736971", 
            "performedBy": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "type": "transmissionOpened",
            "relatedTransmissionId": "{{transmissionId-utsending-1}}"
        },

        // Utsending nr 2 (avvisning) åpnet av Ola Nordmann
        {
            "createdAt": "2024-01-14T13:31:12.8736971", 
            "performedBy": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "type": "transmissionOpened",
            "relatedTransmissionId": "{{transmissionId-utsending-2}}"
        },

        // Utsending nr 3 (purring) åpnet av Ola Nordmann
        {
            "createdAt": "2024-01-21T16:41:24.8475550", 
            "performedBy": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "type": "transmissionOpened",
            "relatedTransmissionId": "{{transmissionId-utsending-3}}"
        },
        // Ingen innslag for åpning av utsending 4 og 5

        // Dialog lukket ifm vedtak om tvangsmulkt
        {
            "createdAt": "2024-03-08T08:01:03.6301023", 
            "performedBy": {
                "role": "serviceOwner",
            },
            "type": "dialogClosed"
        }        
    ],

    // Denne har samme logikk som i dag, og følger dialog-aggregatet (og dets updatedAt), og indikerer
    // altså hvem som har åpnet dialogen (ikke nødvendigvis forsendelsene) siden forrige oppdatering
    // "seenlog"-endepunktet inneholder alle seen-innslag for alle endringer.
    "seenSinceLastUpdate": [
        {
            "id": "01298471-a444-46e8-80b0-f8bc18f10e1c",
            "seenAt": "2024-03-08T08:42:01.3301025",
            "endUserIdHash": "0676ad6bdf",
            "endUserName": "Ola Nordmann",
            "isCurrentEndUser": true
        }
    ]
}
```

## Hvordan dialogen oppdateres gjennom livsløpet til dialogen

Her tar vi utgangspunkt i tredjepartsopplysninger fra boligsameie (slik vi forstår den).

### Hendelse 0: Dialog opprettes i det bruker starter utfylling (men før innsending foretas)

```jsonc
// POST /api/v1/serviceowner/dialogs
{
    "id": "{{dialog-guid}}", 
    "serviceResource": "urn:altinn:resource:ske_tredjepartsopplysninger_boligsameier",
    "party": "urn:altinn:organization:identifier-no:912345678",
    "dueAt": "2025-01-31T23:59:59", // Vi starter med 31, januar inntil noe faktisk er sendt inn
    "content": [
        { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Rapportering av tredjepartsopplysninger for boligsameie" } ] },
        { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Sameier med 9 eller flere boligseksjoner skal levere opplysninger om sameiers andel av felles inntekter, utgifter, formue og gjeld. " } ] },
        { "type": "AdditionalInfo", "value": [ { "cultureCode": "nb_NO", "value": "Frist for innsending er 31. januar. Du vil motta fortløpende tilbakemeldinger på oppgaver du sender inn, og har mulighet til å korrigere disse frem til 1. mars" } ] }
    ],
    "guiActions": [
        {
            "action": "write",
            "url": "https://skatteetaten.no/dyplenke-til-gui-for-innsending-av-oppgaver-for-denne-saken",
            "priority": "Primary",
            "title": [ { "value": "Gå til utfylling av oppgaver", "cultureCode": "nb-no" } ]
        }
    ],
    "apiActions": [
        {
            "action": "write",        
            "type": "urn:ske:fastsetting:innsamling:boligsameie:v2",
            "endpoints": [
                {
                    "version": "v2.1.1",
                    "url": "https://api.skatteetaten.no/endepunkt-for-mottak-eller-korrigering-av-ordinærleveranse",
                    "httpMethod": "POST",
                    "accepts": [ "application/xml" ],
                    "requestSchema": "https://www.skatteetaten.no/contentassets/bacb3ec210ce4ec19aa129f7f1ba5367/v2_1_1/boligsameie_v2_1.xsd"
                }
            ]
        }
    ],
    "activityLog": [
        {
            "sender": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "type": "information",
            "description": [ { "value": "Startet utfylling av oppgaver", "cultureCode": "nb-no" } ]
        }
    ]
}
```

### Hendelse 1: Bruker har foretatt en innsending. 


#### Alt 1. Vi bruker POST for å legge til forsendelsen under "transmissions" på dialogen 

```jsonc
// POST /api/v1/serviceowner/dialogs/{dialogId}/transmissions
{
    "id": "{{transmissionId-innsending-1}}",            
    "sender": {
        "role": "user",
        "name": "Ola Nordmann",
    },
    "content": [
        { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Innlevering av tredjepartsopplysninger" } ] },
        { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Tredjepartsopplysningene ble levert" } ] }
    ],
    "attachments": [
        {
            "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 56 - Ola Huseier", "cultureCode": "nb-no" } ],
            "urls": [
                { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-1-attachment-1.pdf", "mimeType": "application/pdf" },
                { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-1-attachment-1.json",  "mimeType": "application/json" }
            ]
        },
        {
            "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 57 - Kari Huseier", "cultureCode": "nb-no" } ],
            "urls": [
                { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-1-attachment-2.pdf", "mimeType": "application/pdf" },
                { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-1-attachment-2.json",  "mimeType": "application/json" }
            ]
        }
    ]
}
```

### Alt 2. Vi bruker PATCH til å legger til en forsendelse og oppdaterer frist på dialogen til 31. mars i en og samme operasjon

```jsonc
// PATCH /api/v1/serviceowner/dialogs/{dialogId}
[
    {
        "op": "add",
        "path": "/transmissions/-",
        "value":        
        {
            "id": "{{transmissionId-innsending-1}}",            
            "sender": {
                "role": "user",
                "name": "Ola Nordmann",
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Innlevering av tredjepartsopplysninger" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Tredjepartsopplysningene ble levert" } ] }
            ],
            "attachments": [
                {
                    "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 56 - Ola Huseier", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-1-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-1-attachment-1.json",  "mimeType": "application/json" }
                    ]
                },
                {
                    "displayName": [ { "value": "Oppgave for Gnr 12 Bnr 23 Snr 57 - Kari Huseier", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/innsending-1-attachment-2.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/innsending-1-attachment-2.json",  "mimeType": "application/json" }
                    ]
                }
            ]
        }
    },
    {
        "op": "replace",
        "path": "/dueAt",
        "value": "2025-03-01T23:59:59"
    }
]
```

### Hendelse 2: Tjenesteeier sender svar (utsending)

```jsonc
// POST /api/v1/serviceowner/dialogs/{dialogId}/transmissions
{
    "id": "{{transmissionId-utsending-1}}",
    "relatedtransmissionId": "{{transmissionId-innsending-1}}", // knyttet til innsending
    "sender": {
        "role": "serviceOwner" // implisitt er "name" tjenesteeier
    },
    "content": [
        { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Godkjenning av tredjepartsopplysninger" } ] },
        { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Innsendte opplysninger ble godkjente" } ] }
    ],
    "attachments": [
        {
            "displayName": [ { "value": "Godkjenningskvittering", "cultureCode": "nb-no" } ],
            "urls": [
                { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-1-attachment-1.pdf", "mimeType": "application/pdf" },
                { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-1-attachment-1.json",  "mimeType": "application/json" }
            ]
        }
    ]
}
```

Tjenesteeier oppretter her varslingsordrer; en som sendes umiddelbart og en som sendes om 7 dager med en condition som baserer seg på om forsendelsen er logget åpnet

_MERK! ikke reelle modeller, men viser tenkt funksjonalitet_

```jsonc
// POST varsling.altinn.no/api/v1/....
[
    // Ordinær varsling
    {
        "serviceResource": "urn:altinn:resource:ske_tredjepartsopplysninger_boligsameier", 
        "party": "urn:altinn:organization:identifier-no:912345678",
        // associatedWith brukes av arbeidsflate/SBSer for å finne varslingsordrer relatert til en 
        // dialog
        "associatedWith": [
            { "type": "urn:altinn:dialogporten:dialog-id", "id": "{{dialog-guid}}" }
            { "type": "urn:altinn:dialogporten:transmission-id", "id": "{{transmissionId-utsending-1}}" }
        ],
        "channel": "both",
        "content": [ /* .... */ ]
    },
    // Revarsling
    {
        "serviceResource": "urn:altinn:resource:ske_tredjepartsopplysninger_boligsameier",        
        "party": "urn:altinn:organization:identifier-no:912345678",
        "associatedWith": [            
            { "type": "urn:altinn:dialogporten:dialog-id", "id": "{{dialog-guid}}" }
            { "type": "urn:altinn:dialogporten:transmission-id", "id": "{{transmissionId-utsending-1}}" }
        ],
        "channel": "both",
        "content": [ /* .... */ ],
        "delayFor": "+7 days",
        // returnerer noe ala { "conditionMet": true } basert på om openedlog er tom for 
        // {{transmissionId-utsending-1}}
        "conditionUrl": "https://dialogporten.no/api/v1/serviceowner/{{dialog-guid}}/activitylog/?condition=exists&type=transmissionOpened&relatedTransmissionId={{transmissionId-utsending-1}}" 
    }
]
```

### Hendelse 3: Sluttbruker åpner forsendelse (utsending), og tjenesteeier logger dette. 
```jsonc
// POST /api/v1/serviceowner/dialogs/{dialogId}/activitylog
{
    "createdAt": "2024-01-12T17:02:53.9857230", 
    "performedBy": {
        "role": "user",
        "name": "Kari Kollega",
    },
    "type": "transmissionOpened",
    "relatedTransmissionId": "{{transmissionId-utsending-1}}"
}
```

Øvrige hendelser knyttet til inn/utsendinger foregår på samme måte:
- Det legges til en transmission på dialogen
- Det opprettes en eller flere varslingsordrer
- Det logges til aktivitetsloggen når bruker åpner forsendelser (eller foretar signering, betaling etc)

### Avslutning

Modellen øverst viser en dialog hvor brukeren ikke følger opp påleggene om å foreta korrigeringer, som ender med vedtak om tvangsmulkt. Actions bør på det tidspunktet endres til å representere klage-handlinger som er tilgjengelig for brukeren. Klageprosess (gjennomført med annen offentlig instans) bør vurderes realisert i en ny dialog, som refererer den opprinnelige dialogen gjennom [prosess-identifikator](https://github.com/digdir/dialogporten/issues/565).

Det antas at de fleste innrapporteringen ikke ender i tvangsmulkt-vedtak, men når en naturlig ende etter at brukeren har sendt inn alle sine oppgaver og mottatt kvittering på at disse er godkjent. Etter at frist for korrigering er utløpt, bør dialogen i sin helhet oppdateres for å reflektere nettopp dét, og ikke bare bli liggende i innboksen til brukeren med en frist som er utløpt (som av arbeidsflate dermed vil bli vurdert som viktig og løftet høyt, mens den egentlig er avsluttet). 

Her er et eksempel som fører dialogen til en tilstand som tilsvarer det opprinnelige eksemplet (sett fra enduser), som oppdaterer content, status, fjerner frist, legger til en forsendelse for tvangsmulkten, og legger til en activity som indikerer at dialogen er avsluttet i et og samme `PATCH`-kall (her vil man også kunne bruke `PUT`)

```jsonc
// PATCH /api/v1/serviceowner/dialogs/{dialogId}
[
    {
        "op": "replace",
        "path": "/dueAt",
        "value": null
    },
    {
        "op": "replace",
        "path": "/status",
        "value": "Completed"
    },
    {
        "op": "replace",
        "path": "/content",
        "value": [
        
            { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Rapportering av tredjepartsopplysninger for boligsameie" } ] },
            { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Korrigering ble ikke utført innenfor frist, og boligsameier et ilagt tvangsmulkt. Du kan klage for dette vedtaket." } ] },
            { "type": "AdditionalInfo", "value": [ { "cultureCode": "nb_NO", "value": "Vedtak om tvangsmulkt ble gjort 8. mars pga. manglende rapportering. Hvis du mener dette er urettmessig, kan vedtaket påklages." } ] }        
        ]
    },
    {
        "op": "replace",
        "path": "/guiActions",
        "value": [
            {
                "action": "write",
                "authorizationAttribute": "urn:altinn:resource:ske_klage_på_tvangsmulkt_for_boligsameie",
                "url": "https://skatteetaten.no/dyplenke-til-instansiering-av-klage",
                "priority": "Primary",
                "title": [ { "value": "Opprett klage", "cultureCode": "nb-no" } ]
            }
        ]
    },
    {
        "op": "add",
        "path": "/transmissions/-",
        "value": {
            "id": "{{transmissionId-utsending-5}}",
            "isAuthorized": false, 
            "relatedtransmissionId": "{{transmissionId-utsending-4}}", 
            "authorizationAttribute": "urn:altinn:resource:ske_tvangsmulkt",
            "sender": {
                "role": "serviceOwner"
            },
            "content": [
                { "type": "Title", "value": [ { "cultureCode": "nb_NO", "value": "Vedtak om tvangsmulkt" } ] },
                { "type": "Summary", "value": [ { "cultureCode": "nb_NO", "value": "Boligsameiet er ilagt tvangsmulkt pga manglende rapportering." } ] }
            ],
            "attachments": [
                {
                    "displayName": [ { "value": "Vedtak om tvangsmulkt", "cultureCode": "nb-no" } ],
                    "urls": [
                        { "consumerType": "Gui", "url": "https://someexternalsite.com/utsending-5-attachment-1.pdf", "mimeType": "application/pdf" },
                        { "consumerType": "Api", "url": "https://someexternalsite.com/utsending-5-attachment-1.json",  "mimeType": "application/json" }
                    ]
                }
            ]
        }
    },
    {
        "op": "add",
        "path": "/activityLog/-",
        "value": {
            "performedBy": {
                "role": "serviceOwner",
            },
            "type": "dialogClosed"
        }
    }
]
