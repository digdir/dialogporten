---
---
```jsonc
// Input modell som tjenesteeiere oppgir for å opprette en dialog.
// Modellen kan også oppdateres/muteres med PATCH-kall som inneholder de feltene som en ønsker å endre. 
//Ikke-komplekse felter som ligger på rotnivå kan ikke endres (med unntak av "status").

// POST /dialogporten/api/v1/dialogs
{
    // Tjenestetilbyder kan valgfritt oppgi en egen-generert UUID her. Hvis det ikke oppgis vil Dialogporten generere
    // en unik identifikator som blir returnert ved opprettelse
    "id": "e0300961-85fb-4ef2-abff-681d77f9960e",

    // Identifikator som refererer en tjenesteressurs ("Altinn Service Resource") i Altinn Autorisasjon
    // Se https://docs.altinn.studio/technology/solutions/altinn-platform/authorization/resourceregistry/
    // Dette bestemmer også hvilken autorisasjonspolicy som legges til grunn for både ressursen og tilhørende
    "serviceResourceIdentifier": "example_dialogue_service", 

    // Organisasjonsnummer, fødselsnummer eller brukernavn (aka "avgiver" eller "aktør") - altså hvem sin dialogboks 
    // skal dialogen tilhøre. Brukernavn benyttes for selv-registrerte bruker, og er typisk en e-postadresse.
    "party": "org/991825827", 
                                  
    // Vilkårlig referanse som presenteres sluttbruker i UI. Dialogporten tilegger denne ingen semantikk (trenger f.eks. ikke
    // være unik). Merk at identifikator/primærnøkkel vil kunne være den samme gjennom at tjenestetilbyder kan oppgi "id",
    // så dette kan f.eks. brukes for et saksnummer eller en annen referanse hos party eller en tredjepart (systemleverandør). 
    "externalReference": "123456789",

    // Alle dialoger som har samme dialoggruppe-id vil kunne grupperes eller på annet vis samles i GUI    
    "dialogueGroup": {
        "id": "some-arbitrary-id",
        
        // Bestemmer rekkefølgen denne dialogen har blant alle dialoger som har samme dialogueGroup.id
        "order": 1,
        
        // Trenger bare oppgis av én dialog. Hvis oppgitt av flere, er det den med høyest "order"-verdi 
        // som skal benyttes.
        "name": [ { "code": "nb_NO", "value": "Navn på dialoggruppe." } ]
    },

    // Kjente statuser som bestemmer hvordan dialogen vises for bruker: 
    // "unspecified"    = Dialogen har ingen spesiell status. Brukes typisk for enkle meldinger som ikke krever noe 
    //                    interaksjon. Dette er default. 
    // "in-progress"    = Under arbeid. Generell status som brukes for dialogtjenester der ytterligere bruker-input er 
    //                    forventet.
    // "waiting"        = Venter på tilbakemelding fra tjenesteeier
    // "signing"        = Dialogen er i en tilstand hvor den venter på signering. Typisk siste steg etter at all  
    //                    utfylling er gjennomført og validert. 
    // "cancelled"      = Dialogen ble avbrutt. Dette gjør at dialogen typisk fjernes fra normale GUI-visninger.
    // "completed"      = Dialigen ble fullført. Dette gjør at dialogen typisk flyttes til et GUI-arkiv eller lignende.
    "status": "in-progress", 
    
    // En vilkårlig streng som er tjenestespesifikk
    "externalStatus": "SKE-ABC",
    "dates": {
        // Hvis oppgitt blir dialogen satt med en frist 
        // (i Altinn2 er denne bare retningsgivende og har ingen effekt, skal dette fortsette?)
        "dueDateTime": "2022-12-01T12:00:00.000Z",
        
        // Mulighet for å skjule/deaktivere en dialog på et eller annet tidspunkt?
        "expiryDate": "2023-12-01T12:00:00.000Z" 
    },
    "content": {
        // Alle tekster som vises verbatim må oppgis som en array av oversatte tekster. 
        // Det som benyttes er brukerens valgte språk i Dialogboksen
        "body": [ { "code": "nb_NO", 
            "value": "Innhold med <em>begrenset</em> HTML-støtte. Dette innholdet vises når dialogen ekspanderes." } ],
        "title": [ { "code": "nb_NO", "value": "En eksempel på en tittel" } ],
        "senderName": [ { "code": "nb_NO", "value": "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } ]            
    },  
    "attachments": [
        {
            "displayName": [ { "code": "nb_NO", "value": "dette er et vedlegg" } ],
            "sizeInBytes": 123456,
            "contentType": "application/pdf",            
            "url": "https://example.com/api/dialogues/123456789/attachments/1",

            // Det kan oppgis en valgfri referanse til det som mappes til en XACML-ressurs. Brukeren må ha tilgang til "open" i
            // XACML-policy for oppgitt XACML-ressurs for å få tilgang til dialogen.
            "authorizationRequirement": "attachment1"
        }
    ],
    "actions": {
        "gui": [ 
            { 
                "action": "open", // Denne kan refereres i XACML-policy                
                "type": "primary", // Dette bestemmer hvordan handlingen presenteres.
                "title": [ { "code": "nb_NO", "value": "Åpne i dialogtjeneste" } ],
                "url": "https://example.com/some/deep/link/to/dialogues/123456789"
            },
            {
                "action": "confirm",
                "authorizationRequirement": "somesubresource", // Det kan oppgis en valgfri referanse til en XACML-ressurs
                "type": "secondary",
                "title": [ { "code": "nb_NO", "value": "Bekreft mottatt" } ],

                // Dette foretar et POST bakkanal-kall til oppgitt URL, og det vises i frontend bare en spinner mens 
                // kallet går. Må returnere en oppdatert DE-modell (som da vises bruker) eller 204 (hvis dialogen 
                // oppdatert i annet bakkanal-kall), eller en RFC7807-kompatibel feilmelding.
                "isBackChannel": true, 

                "url": "https://example.com/some/deep/link/to/dialogues/123456789/confirmReceived"
            },
            { 
                "action": "delete",
                "type": "tertiary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],

                // Dette impliserer isBackChannel=true, og viser i tillegg en "Er du sikker?"-prompt. 
                // Vil ved vellykket kall skjule dialogen fra GUI, og legge dialogen i søppelkasse
                "isDeleteAction": true, 

                // Blir kalt med DELETE i bakkanal. Må returnere 204 eller en RFC7807-kompatibel feilmelding.
                "url": "https://example.com/some/deep/link/to/dialogues/123456789" 
            }
        ],
        "api": [ 
            { 
                "action": "open", // Denne referes til i XACML-policy (som action)
                "endpoints": [ 
                    // Det støttes ulike parallelle versjoner av API-endepunkter. Første i lista er å regne som 
                    // siste versjon og anbefalt brukt. GUI-actions er ikke versjonerte.
                    {
                        "version": "v2",
                        "actionUrl": "https://example.com/api/v2/dialogues/123456789",
                        "method": "GET",

                        // Indikerer hva API-konsumenter kan forvente å få slags svar
                        "responseSchema": "https://schemas.altinn.no/dialogs/v2/dialogs.json", 
                        // Lenke til dokumentasjon for denne actionen
                        "documentationUrl": "https://api-docs.example.com/v2/dialogueservice/open-action"                         
                    },
                    {
                        "version": "v1",

                        // Tjenestetilbyder kan indikerer om en versjon er utgående, og evt oppgi en dato 
                        // for når versjonen ikke lengre støttes
                        "deprecated": true, 
                        "sunsetDate": "2024-12-31T23:59:59.999Z",
                        "actionUrl": "https://example.com/api/v1/dialogues/123456789",
                        "method": "GET",

                        // Indikerer hva API-konsumenter kan forvente å få slags svar
                        "responseSchema": "https://schemas.altinn.no/dialogs/v1/dialogs.json", 
                        // Lenke til dokumentasjon for denne actionen
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
                        "actionUrl": "https://example.com/api/dialogues/123456789/confirmReceived/23456",
                        // Hvis handlingen omfatter/berører en spesifikk entitet som utgjør en logisk del av dialogen
                        // kan det oppgis en identifikator til denne her.
                        "externalId": "23456",
                        "documentationUrl": "https://api-docs.example.com/dialogueservice/confirm-action"
                        // Ingen requestmodell impliserer tom body
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
                
                        // Indikerer hva API-et forventer å få som input på dette endepunktet
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
                        // Merk dette vil kreve at org gjør bakkanal-kall for å slette dialogen
                        "actionUrl": "https://example.com/api/dialogues/123456789"
                    }
                ]
            }
        ]
    },
    // Dette er en lineær, "append-only" historikk vedlikeholdt av tjenesteeier som indikerer hva som logisk har skjedd 
    // gjennom den aktuelle dialogen. 
    //
    // En rekke ulike typer aktivitet gjenkjennes, og kan brukes for å indikere innsendinger, utsendinger (enten som 
    // tilbakemelding på en innsending, eller frittstående), feilsituasjoner og annen generell informasjon.
    //
    // Dette tilgjengeliggjøres sluttbruker gjennom GUI og API, og vil  slås sammen med 
    // aktivitet foretatt i dialogporten, som kan være:
    // - videredelegering av instansen
    // - dialogen åpnes for første gang
    //
    // Se dialogporten-get-single-response.json for flere eksempler.
    "activityHistory": [
        { 
            // Tjenestetilbyder kan selv oppgi identifikator
            "activityId": "fc6406df-6163-442a-92cd-e487423f2fd5",

            "activityDateTime": "2022-12-01T10:00:00.000Z",
            // Her kan det være ulike typer som medfører ulik visning i GUI. Følgende typer gjenkjennes:            
            // - submission:     Refererer en innsending utført av party som er mottatt hos tjenestetilbyder.
            // - feedback:       Indikerer en tilbakemelding fra tjenestetilbyder på en innsending.
            // - information:    Informasjon fra tjenestetilbyder, ikke (direkte) relatert til noen innsending.  
            // - error:          Brukes for å indikere en feilsituasjon, typisk på en innsending. Inneholder en
            //                   tjenestespesifikk activityErrorCode.
            // - closed:         Indikerer at dialogen er lukket for videre endring. Dette skjer typisk ved fullføring
            //                   av dialogen, eller sletting.
            //
            // Typer som kun kan settes av Dialogporten selv som følge av handlinger utført av bruker:
            // - seen:           Når dialogen først ble hentet og av hvem. Kan brukes for å avgjøre om purring 
            //                   skal sendes ut, eller internt i virksomheten for å tracke tilganger/bruker.
            //                   Merk at dette ikke er det samme som "lest", dette må tjenestetilbyder selv håndtere 
            //                   i egne løsninger.
            // - forwarded:      Når dialogen blir videresendt (tilgang delegert) av noen med tilgang til andre
            "activityType": "submission",

            // Indikerer hvem som står bak denne aktiviteten. Fravær av dette feltet indikerer at det er tjenesteilbyder
            // som har utført aktiviteten.
            "performedBy": "person:12018212345",
            
            // Vilkårlig streng som er ment å være maskinlesbar, og er en tjenestespesifikk kode som gir ytterligere
            // informasjon om hva slags aktivitetstype dette innslaget er
            "externalType": "SKE-1234-received-precheck-ok",

            // Hvis aktiviteten omfatter eller berør en eller annen entitet som utgjør en del av dialogen hos tjenestetilbyder,
            // kan denne oppgis her. Denne underlegges ingen validering eller logikk i Dialogporten, men vil bli lagt ved i 
            // events som genereres.
            "externalId": "b323cef4-adbd-4d2c-b33d-5c0f3b11171b",

            // Menneskelesbar beskrivelse av aktiviteten
            "activityDescription": [ { "code": "nb_NO", "value": "Innsending er mottatt og sendt til behandling" } ],

            // Ytterligere informasjon som bruker/SBS kan aksessere for å hente mer informasjon om det aktuelle innslaget.
            // Semantikk rundt disse er tjenestespesifikke, men skal henge sammen med activityType. Tilbyder kan oppgi én 
            // av dem, eller begge. GUI-lenker er ment for å sende sluttbrukere til en lenke som gir innsyn og/eller mer
            // informasjon knyttet til den aktuelle aktivitet. API-lenker er ment for å kunne gi SBS-er strukturerte 
            // data relatert til aktiviteten. 
            "activityDetailsUrls": {
                // Når activityType er "submission" refererer API-lenken her typisk til den mottatte innsendingen
                "api": [
                    {
                        "version": "v1",
                        "url": "https://example.com/api/dialogues/123456789/received_submissions/b323cef4-adbd-4d2c-b33d-5c0f3b11171b"
                    }
                ],
                // ... mens GUI-lenken typisk tar brukeren til en innsynsside hvor hen kan se hva som ble sendt inn
                "gui": "https://example.com/dialogues/123456789/view_submission/b323cef4-adbd-4d2c-b33d-5c0f3b11171b"
            }
        }
    ],
    // Dette er ulike innstillinger som kun kan oppgis og er synlig for tjenesteeier
    "configuration": {

        // Tjenestetilbyder kan oppgi et selvpålagt tokenkrav, som innebærer at dette dialogen vil kreve at det 
        // autoriseres med et Maskinporten-token som inneholder følgende scopes i tillegg til 
        "serviceProviderScopesRequired": [ "serviceprovider:myservice" ],

        // Når blir dialogen synlig hos party. Muliggjør opprettelse i forveien og samtidig tilgjengeliggjøring 
        // for mange parties.
        "visibleDateTime": "2022-12-01T12:00:00.000Z"
    }
}
```