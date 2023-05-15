---
---
```jsonc
// Input modell som tjenesteeiere oppgir for å opprette en dialog.
// Modellen kan også oppdateres/muteres med PATCH-kall (se https://jsonpatch.com/)
//Ikke-komplekse felter som ligger på rotnivå kan ikke endres (med unntak av "status").

// POST /dialogporten/api/v1/dialogs
{
    // Tjenestetilbyder kan valgfritt oppgi en egen-generert UUID her. Hvis det ikke oppgis vil Dialogporten generere
    // en unik identifikator som blir returnert ved opprettelse
    "id": "e0300961-85fb-4ef2-abff-681d77f9960e",

    // Identifikator som refererer en tjenesteressurs ("Altinn Service Resource") i Altinn Autorisasjon
    // Se https://docs.altinn.studio/technology/solutions/altinn-platform/authorization/resourceregistry/
    // Dette bestemmer også hvilken autorisasjonspolicy som legges til grunn.
    "serviceResourceIdentifier": "example_dialog_service", 

    // Organisasjonsnummer, fødselsnummer eller brukernavn (aka "avgiver" eller "aktør") - altså hvem sin dialogboks 
    // skal dialogen tilhøre. Brukernavn benyttes for selv-registrerte bruker, og er typisk en e-postadresse.
    "party": "org/991825827", 
                                  
    // Vilkårlig referanse som presenteres sluttbruker i UI. Dialogporten tilegger denne ingen semantikk (trenger f.eks. ikke
    // være unik). Merk at identifikator/primærnøkkel vil kunne være den samme gjennom at tjenestetilbyder kan oppgi "id",
    // så dette kan f.eks. brukes for et saksnummer eller en annen referanse hos party eller en tredjepart (systemleverandør). 
    "externalReference": "123456789",

    // Alle dialoger som har samme dialoggruppe-id vil kunne grupperes eller på annet vis samles i GUI    
    "dialogGroup": {
        "id": "some-arbitrary-id",
        
        // Bestemmer rekkefølgen denne dialogen har blant alle dialoger som har samme dialogGroup.id
        "order": 1,
        
        // Trenger bare oppgis av én dialog. Hvis oppgitt av flere, er det den med høyest "order"-verdi 
        // som skal benyttes.
        "name": [ { "code": "nb_NO", "value": "Navn på dialoggruppe." } ]
    },

    // Kjente aggregerte statuser som bestemmer hvordan dialogen vises for bruker: 
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
    "extendedStatus": "SKE-ABC",
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
    // Det skilles mellom dialogelementer som er ment for GUI og for API. Førstnevnte er typiske filvedlegg i menneskelesbart format,
    // f.eks. PDF, som tjenestetilbyder legger ved dialogen. Sistnevnte er typisk 
    //
    // Dialogelementer kan hentes (leses) gjennom oppgitt URL. Actions kan peke på et spesifikt dialogelement for andre operasjoner direkte
    // knyttet til et dialogelement.
    "dialogElements": {
        "gui": [
            {
                // Unik identifikator for dialogElement. Kan oppgis av tjenestetilbyder for å sikre idempotens.
                "dialogElementId": "5b5446a7-9b65-4faf-8888-5a5802ce7de7",
                "url": "https://example.com/api/dialogs/123456789/dialogelements/1.pdf",

                // Brukes for å vise sluttbrukeren hva dette er 
                "displayName": [ { "code": "nb_NO", "value": "Innsendt skjema" } ],
                
                // Valgfri: MIME-type. Brukes i GUI for hint om hva slags dokument dette er 
                "contentType": "application/pdf",                        

                // Valgfri: Det kan oppgis en referanse til det som mappes til en XACML-ressurs. Hvis oppgitt, må brukeren må ha tilgang 
                // til actionen "elementread" i XACML-policy evt. begrenset til denne ressursen. Hvis ikke oppgitt kreves bare "read".
                "authorizationResource": "urn:altinn:subresource:somesubresource"
            }
        ],
        "api": [
            {
                "dialogElementId": "8bf7c93c-ab32-49a6-a890-d2b450b3e7ad",
                "url": "https://example.com/api/dialogs/123456789/dialogelements/1.xml",
                
                // Valgfri: Tjenestetilbyder-oppgitt type-betegnelse som er maskinlesbar.  
                "dialogElementType": "somethingservicespecific",
                
                // Valgfri: JSON-schema som indikerer schemaet til dialogelementet
                "dialogElementSchema": "https://schemas.example.com/dialogservice/v1/somethingservicespecific.json",                        

                // Valgfri: Det kan oppgis en referanse til det som mappes til en XACML-ressurs. Hvis oppgitt, må brukeren må ha tilgang 
                // til actionen "elementread" i XACML-policy evt. begrenset til denne ressursen. Hvis ikke oppgitt kreves bare "read".
                "authorizationResource": "urn:altinn:subresource:someothersubresource"
            }
        ]        
    },
    "actions": {
        "gui": [ 
            { 
                "action": "open", // Denne kan refereres i XACML-policy                
                "importance": "primary", // Dette bestemmer hvordan handlingen presenteres.
                "title": [ { "code": "nb_NO", "value": "Åpne i dialogtjeneste" } ],
                "url": "https://example.com/some/deep/link/to/dialogs/123456789"
            },
            {
                "action": "confirm",
                "authorizationResource": "urn:altinn:subresource:somesubresource", // Det kan oppgis en valgfri referanse til en XACML-ressurs
                "importance": "secondary",
                "title": [ { "code": "nb_NO", "value": "Bekreft mottatt" } ],

                // Dette foretar et POST bakkanal-kall til oppgitt URL, og det vises i frontend bare en spinner mens 
                // kallet går. Må returnere en oppdatert DE-modell (som da vises bruker) eller 204 (hvis dialogen 
                // oppdatert i annet bakkanal-kall), eller en RFC7807-kompatibel feilmelding.
                "isBackChannel": true, 

                "url": "https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"
            },
            { 
                "action": "delete",
                "importance": "tertiary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],

                // Dette impliserer isBackChannel=true, og viser i tillegg en "Er du sikker?"-prompt. 
                // Vil ved vellykket kall skjule dialogen fra GUI, og legge dialogen i søppelkasse
                "isDeleteAction": true, 

                // Blir kalt med DELETE i bakkanal. Må returnere 204 eller en RFC7807-kompatibel feilmelding.
                "url": "https://example.com/some/deep/link/to/dialogs/123456789" 
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
                        "actionUrl": "https://example.com/api/v2/dialogs/123456789",
                        "method": "GET",

                        // Indikerer hva API-konsumenter kan forvente å få slags svar
                        "responseSchema": "https://schemas.example.com/dialogservice/v2/dialogservice-prelim-receipt.json" , 
                        // Lenke til dokumentasjon for denne actionen
                        "documentationUrl": "https://api-docs.example.com/v2/dialogservice/open-action"                         
                    },
                    {
                        "version": "v1",

                        // Tjenestetilbyder kan indikerer om en versjon er utgående, og evt oppgi en dato 
                        // for når versjonen ikke lengre støttes
                        "deprecated": true, 
                        "sunsetDate": "2024-12-31T23:59:59.999Z",
                        "actionUrl": "https://example.com/api/v1/dialogs/123456789",
                        "method": "GET",

                        // Indikerer hva API-konsumenter kan forvente å få slags svar
                        "responseSchema": "https://schemas.example.com/dialogservice/v1/dialogservice-prelim-receipt.json" , 
                        // Lenke til dokumentasjon for denne actionen
                        "documentationUrl": "https://api-docs.example.com/v1/dialogservice/open-action",
                    },
                ]
            },
            { 
                "action": "confirm",
                // Hvis handlingen omfatter/berører et spesifikt dialogelement
                // kan det oppgis en identifikator til dette her.
                "relatedDialogElementId": "4558064e-4049-4075-a58f-d67bda83f88c",
                "endpoints": [
                    {
                        "version": "v1",
                        "method": "POST",
                        "actionUrl": "https://example.com/api/dialogs/123456789/confirmReceived/23456",
                        "documentationUrl": "https://api-docs.example.com/dialogservice/confirm-action"
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
                        "actionUrl": "https://example.com/api/dialogs/123456789",
                        "method": "POST",
                
                        // Indikerer hva API-et forventer å få som input på dette endepunktet
                        "requestSchema": "https://schemas.example.com/dialogservice/v1/dialogservice.json", 
                        "responseSchema": "https://schemas.example.com/dialogservice/v1/dialogservice-prelim-receipt.json" 
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
                        "actionUrl": "https://example.com/api/dialogs/123456789"
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
            "extendedActivityType": "SKE-1234-received-precheck-ok",

            // Hvis denne aktiviteten har direkte avstedkommet et dialogelement, kan dette oppgis her. Det valideres at 
            // oppgitt dialogElementId også oppgis i "dialogElements" i samme request. Denne identifikatoren blir lagt ved 
            // events som genereres.
            "dialogElementId": "b323cef4-adbd-4d2c-b33d-5c0f3b11171b",

            // Hvis aktiviteten har en relasjon til et annet dialogelement, f.eks. en tidligere innsending, kan dette 
            // oppgis her. Det valideres at oppgitt dialogElementId finnes i "dialogElements"; enten som oppgitt i samme 
            // request eller at den finnes fra før. Denne identifikatoren blir lagt ved events som genereres.
            "relatedDialogElementId": "dbce996a-cc67-4cb2-ad2f-df61cee6683a",

            // Menneskelesbar beskrivelse av aktiviteten
            "activityDescription": [ { "code": "nb_NO", "value": "Innsending er mottatt og sendt til behandling" } ]
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