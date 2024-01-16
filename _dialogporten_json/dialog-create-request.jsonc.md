---
---
```jsonc
// Input modell som tjenesteeiere oppgir for å opprette en dialog.
// Modellen kan også oppdateres/muteres med PATCH-kall (se https://jsonpatch.com/)
//Ikke-komplekse felter som ligger på rotnivå kan ikke endres (med unntak av "status").

// POST /dialogporten/api/v1/servicowner/dialogs
{
    // Tjenestetilbyder kan valgfritt oppgi en egen-generert UUID her. Hvis det ikke oppgis vil Dialogporten generere
    // en unik identifikator som blir returnert ved opprettelse
    "id": "e0300961-85fb-4ef2-abff-681d77f9960e",

    // Identifikator som refererer en tjenesteressurs ("Altinn Service Resource") i Altinn Autorisasjon
    // Se https://docs.altinn.studio/authorization/modules/resourceregistry/
    // Dette bestemmer også hvilken autorisasjonspolicy som legges til grunn.
    "serviceResource": "urn:altinn:resource:super-simple-service", 

    // Organisasjonsnummer, fødselsnummer eller brukernavn (aka "avgiver" eller "aktør") - altså hvem sin dialogboks 
    // skal dialogen tilhøre. Brukernavn benyttes for selv-registrerte bruker, og er typisk en e-postadresse.
    "party": "urn:altinn:organization:identifier-no::991825827", 
                                  
    // Vilkårlig referanse som presenteres sluttbruker i UI. Dialogporten tilegger denne ingen semantikk (trenger f.eks. ikke
    // være unik). Merk at identifikator/primærnøkkel vil kunne være den samme gjennom at tjenestetilbyder kan oppgi "id",
    // så dette kan f.eks. brukes for et saksnummer eller en annen referanse hos party eller en tredjepart (systemleverandør). 
    "externalReference": "123456789",

    // Alle dialoger som har samme prosess-id vil kunne grupperes eller på annet vis samles i GUI    
    "process": {
        "id": "some-arbitrary-id",
        
        // Bestemmer rekkefølgen denne dialogen har blant alle dialoger som har samme prosess-id
        "order": 1,
        
        // Trenger bare oppgis av én dialog. Hvis oppgitt av flere, er det den med høyest "order"-verdi 
        // som skal benyttes.
        "name": [ { "code": "nb_NO", "value": "Navn på prosess." } ]
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
    
    // Alle tekster som vises verbatim må oppgis som en array av oversatte tekster. 
    // Det som benyttes er brukerens valgte språk i Dialogboksen
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

    // Search Tags gjør det mulig å oppgi ord og fraser som det skal kunne søkes på, men som en ikke ønsker å
    // eksponere i DTO-en til sluttbrukere
    "searchTags": [
        {
            "value": "noe søkbart"
        }
    ],

    // Dialogelementer kan være tiltenkt enten GUI eller API, eller begge. GUI-dialogelementer er typiske filvedlegg i 
    // menneskelesbart format, f.eks. PDF, som tjenestetilbyder legger ved dialogen. Dette kan være utsendinger fra 
    // tjenestilbyder eller innsendinger fra parten. API-dialogelementer er strukturerte filer tiltenkt SBS-er. 
    // Dette kan være enkelteskjemaer, egne prefill-modeller, strukturerte feilmeldinger,tidligere innsendte skjema etc.
    //
    // Dialogelementer kan også indikeres at de skal embeddes, altså lastes og vises direkte i det aktuelle 
    // sluttbrukersystemet (arbeidsflate/nettleser). Denne typer dialogelementer har typisk mimeType: text/html
    // 
    // Dialogelementer kan hentes (leses) gjennom oppgitt URL. Actions kan peke på et spesifikt dialogelement for andre
    // operasjoner direkte knyttet til et dialogelement.
    "dialogElements": [
        {
            // Unik identifikator for dialogElement. Kan oppgis av tjenestetilbyder for å sikre idempotens.
            "dialogElementId": "5b5446a7-9b65-4faf-8888-5a5802ce7de7",

            // Hvis dialogelementet har en relasjon til et annet dialogelement, f.eks. en tidligere innsending, kan dette 
            // oppgis her. 
            "relatedDialogElementId": "dbce996a-cc67-4cb2-ad2f-df61cee6683a",
            
            // Vilkårlig URI som indikerer type dialogElement
            "dialogElementType": "skd:form-type-1",   

            // Valgfri: Brukes for å vise sluttbrukeren hva dette er, typisk bare brukt i GUI
            "displayName": [ { "code": "nb_NO", "value": "Innsendt skjema" } ],
            
            // Valgfri: Det kan oppgis en referanse til det som mappes til en XACML-ressurs-attributt. Hvis oppgitt, må 
            // brukeren må ha tilgang  til actionen "elementread" i XACML-policy evt. begrenset til denne ressursen. 
            // Hvis ikke oppgitt kreves bare "read".
            "authorizationAttribute": "urn:altinn:subresource:somesubresource",
            "urls": [
                {
                    // Indikerer hvilken type konsument denne URL-en er tiltenkt (menneske/GUI eller maskin/API)
                    "consumerType": "gui",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/5b5446a7.pdf",
                    // Valgfri: MIME-type. Brukes i GUI for hint om hva slags dokument dette er 
                    "mimeType": "application/pdf"
                },
                {
                    "consumerType": "api",
                    "url": "https://example.com/api/dialogs/123456789/dialogelements/5b5446a7.xml",
                    "mimeType": "application/json"
                }
            ]
        },
        {
            // Dialogelementer kan også representere innhold som skal lastes og embeddes i GUI
            "authorizationAttribute": "urn:altinn:subresource:somesubresource",
            // Valgfri: Blir rendret som en tittel for det embedda innholdet.
            "displayName": [ { "code": "nb_NO", "value": "Melding om bla bla bla" } ],
            "urls": [
                {
                    "consumerType": "gui",
                    // Kan bare brukes på "gui" actions. Sluttbrukersystemet (statisk del i nettleseren) vil laste 
                    // denne med Fetch API og oppgi dialogtoken, og vise dette direkte i detaljvisning.
                    // Det kan være flere front-channel embeds, enten innenfor et og samme dialogelement eller på
                    // tvers av flere dialogelementer. Innenfor samme dialogelement vil statisk del av arbeidsflate 
                    // typisk rendre disse etter hverandre med en eller annen visuell separator. 
                    //
                    // Hvis flere dialogelementer inneholder frontChannelEmbeds, vil GUI måtte bestemme
                    // hvem av disse som er relevant å vise. En mulig logikk her vil være å vise den som er referert 
                    // til av det nyeste aktivitetshistorikk-innslaget, og vise evt. øvrige kollapset (kan lene seg
                    // på "displayName"). Hvis ingen er referert til av aktivitetshistorikk-innslag, vis kun den siste
                    // i lista.
                    "frontChannelEmbed": true,
                    // mimeType blir i utgangspunktet ikke hensyntatt, det antas mimeType: text/html
                    // Her kan det tenkes at andre mimeTypes (f.eks. video) kan håndteres på andre måter i fremtiden
                    "url": "https://example.com/api/dialogs/123456789/user-instructions"
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
                // Det kan oppgis en valgfri referanse til en XACML-ressurs-attributt
                "authorizationAttribute": "urn:altinn:subresource:somesubresource", 
                "priority": "secondary",
                "title": [ { "code": "nb_NO", "value": "Bekreft mottatt" } ],

                // Dette indikerer til GUI at dette er en eller annen skrivehandling som ikke skal innebære
                // en omdirigering av nettleseren, men kun foreta en POST/DELETE med Fetch API-et med dialogtoken.
                "writeAction": {
                    // enum: post, delete (valgfritt, default: post)
                    // Indikerer hvilket verb som skal benyttes i requesten
                    "method": "post",
                
                    // enum: reload, archive, delete, noop (default: noop)
                    // Indikerer hva GUI-et skal gjøre ved vellykket operasjoner (2xx respons)
                    // - reload: Indikerer at GUI-et laster dialogen på nytt og viser denne. Legger opp til at tjenesteeier har 
                    //           oppdatert dialogen synkront i bakkanal. 
                    // - archive: Indikerer at GUI-et skal utføre noe tilsvarende en brukerstyrt arkiveringshandling (via labelling)
                    // - delete: Indikerer at GUI-et skal utføre noe tislvarende en brukerstyrt slettehandling (via labelling)                    
                    // - noop: Indikerer at GUI-et ikke utfører noe med dialogvisningen
                    // - hideAction: Indikerer at GUI-et i DOM skal skjule knappen/lenken som ble brukt for å trigge handlingen
                    // - disableAction: Indikerer at GUI-et i DOM skal deaktivere knappen/lenken som ble brukt for å trigge handlingen
                    "onSuccess": "noop", 

                    // Vises etter vellykket utført handling. Valgfri, vil benytte en standardtekst hvis ikke oppgitt.
                    "successMessage": [ { "code": "nb_NO", "value": "Handlingen ble utført" } ],

                    // Feilmelding som vises hvis handlingen feiler. Kan overstyres av responsen (hvis RFC7807). Valgfri, vil 
                    // benytte en stadardtekst hvis ikke oppgitt.
                    "errorMessage": [ { "code": "nb_NO", "value": "Handlingen ble ikke utført" } ],

                    // Hvis oppgitt, vil vise en continue/cancel prompt til sluttbruker som må bekreftes før handlinge blir 
                    // forsøkt utført
                    "prompt": [ { "code": "nb_NO", "value": "Vil du sende lesebekreftelse?" } ]

                },

                "url": "https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"
            },
            { 
                "action": "delete",
                "priority": "tertiary",
                "title": [ { "code": "nb_NO", "value": "Avbryt" } ],

                // Shorthand for en writeAction tilsvarende:
                //"writeAction": {
                //    "method": "delete",
                //    "onSuccess": "delete",
                //    "successMessage": [ { "code": "nb_NO", "value": "Sletting utført" } ],
                //    "prompt": [ { "code": "nb_NO", "value": "Er du sikker på du vil slette?" } ]
                //}                 
                "deleteAction": true,
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
                "dialogElementId": "4558064e-4049-4075-a58f-d67bda83f88c",
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
            "performedBy": [ { "code": "nb_NO", "value": "Anne Olsen" } ],
            
            // Vilkårlig streng som er ment å være maskinlesbar, og er en tjenestespesifikk kode som gir ytterligere
            // informasjon om hva slags aktivitetstype dette innslaget er
            "extendedActivityType": "SKE-1234-received-precheck-ok",

            // Hvis denne aktiviteten har direkte avstedkommet et dialogelement, kan dette oppgis her. Det valideres at 
            // oppgitt dialogElementId også oppgis i "dialogElements" i samme request. Denne identifikatoren blir lagt ved 
            // events som genereres.
            "dialogElementId": "b323cef4-adbd-4d2c-b33d-5c0f3b11171b",

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