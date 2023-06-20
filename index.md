---
menutitle: Dialogporten
title: Dialogporten - konsept for felles nasjonal arbeidsflate for både API- og GUI-konsumenter av offentlige tjenester
toc: true
layout: page
topmenu: false
---

Versjon 0.9 - Bjørn Dybvik Langfors, sist endret: {{ page.last_modified_at  | date: '%d. %b %Y, kl. %H:%M:%S' }} [(se git-historikk)](https://github.com/digdir/dialogporten/commits/docs/index.md)

# Introduksjon

{% include note.html type="warning" content="Dette er under arbeid, og alt som står i dette dokumentet må anses som foreløpig og gjenstand for endring. Formålet med dokumentet er primært å skissere et konsept, ikke nødvendigvis detaljere en løsning (selv om teksten til tider er forholdsvis detaljert)." %}

I dette dokumentet beskrives et konsept for hvordan tjenester, som i denne konteksten er begrenset til dialog- og meldingstjenester, kan nyttiggjøre seg av fellesfunksjonalitet i økosystemet av Altinn-produkter, herunder dagens "innboks", sluttbrukers arkiv, autorisasjon, varsling og hendelser uten at det innebærer behov for å benytte seg av Altinns utviklingsmiljøer eller applikasjonskjøretidsmiljø. Alle interaksjoner mellom tjenestetilbydere og denne løsningen foregår via API-er, og det legges opp til stor fleksibilitet i hvorvidt løsningen involveres og det legges ingen begrensninger på hvordan forretningslogikken eller ulike brukerflater hos tjenestetilbyder realiseres.

# Scenarioer som påvirker Dialogporten

Det er typisk tre scenarioer som innebærer behov for interaksjon med Dialogporten og dialoger.

1. **Sluttbruker-initiert dialog**, hvor sluttbruker (på vegne av seg selv eller annen part) finner og starter tjeneste ved hjelp av  
    * Offentlig tjenestekatalog
    * Søkemotor
    * Etatenes nettsider
    * SBS (enten manuelt eller fordi SBS agerer på en hendelse  
    Dette fører til at tjenestetilbyder oppretter en dialog i Dialogporten som tilhører den aktuelle mottakeren.  

2. **Tjenestetilbyder-initiert dialog**, hvor tjenestetilbyder oppretter dialogen selv i Dialogporten som tilhører den aktuelle mottakeren. Dette er typisk:
    * «Prefill»-scenarioer, hvor tjenestetilbyderen trenger å innhente opplysninger og gir aktøren et delvis forhåndsutfylt skjema å begynne med
    * Proaktive/sammenhengende tjenester, hvor en tjenestetilbyder igangsetter en dialog som følge av en hendelse (som kan ha oppstått i en annen tjeneste)  

3. **Sending av digital post**
    * DPV, DPI
    * Typisk én-veis (foruten «rekommandert»-funksjonalitet hvor tjenestetilbyder trenger bekreftelse på at melding er mottatt og lest)
    * Ikke-muterbar - meldingen forandrer seg ikke etter at den er mottatt (annet enn «lest»-status og arkiv/slettet-tilstand)
    * Kan være utgangspunkt for sluttbruker-initiert dialog, med lenker til «neste trinn» 
    * Teknisk/funksjonelt subset av tjenestetilbyder-initiert dialog, men kan også være del av en sluttbruker-initiert dialog (f.eks. et vedtaksbrev)

Det finnes andre scenarioer rundt oppslag/innsynstjenester og filoverføringer som trolig ikke vil behøve en representasjon i en Dialogporten, og er følgelig out-of-scope for dette arbeidet.

# Viktige begreper

## GUI 

_"Graphical User Interface"_, som på norsk kan oversettes til grafisk brukergrensesnitt. Typisk en webløsning som konsumeres gjennom en nettleser, men kan også være f.eks. et fagsystem i form av en desktop-applikasjon eller en mobil app.

## API 

_"Application Programming Interface"_, maskinelt grensesnitt som gjør det mulig for ulike systemer å kommunisere på hverandre.

## Dialogporten

_Dialogporten_ benyttes for å betegne det overordnede konseptet som beskrives i dette dokumentet, prosjektet/tiltaket som utarbeidet konseptet, samt teamet som jobber med å realisere det.

Dialogporten benyttes også som navn på løsningskomponenten (produktet) som tilbyr API for enhetlig tilgang og håndtering av digitale dialoger til Felles Arbeidsflate(r) og Sluttbrukersystemer, inkludert lagring av metadata om dialogene, og som dekker Altinn Platform-funksjonalitet for tilgangsstyring og -kontroll, hendelser og varsling.

## Felles Arbeidsflate 

_Felles Arbeidsflate_ refererer til en tenkt implementasjon av et GUI som benytter seg av Dialogporten, og som fungerer som en felles grafisk arbeidsflate for alle som ikke benytter et sluttbrukersystem eller et skreddersydd GUI implementert på f.eks. en etatsportal.

## Sluttbrukersystem (SBS) og fagsystem

_Sluttbrukersystemer_ og _fagsystemer_ er begge applikasjoner som benytter seg API for å tilby et skreddersydd GUI for en eller flere grupper brukere i ulike kontekster. 

## Systembruker

Tilsvarede "systembruker" i Altinn 2, er en systembruker  en ikke-menneskelig identitet knyttet til en virksomhet (i praksis et organisasjonsnummer) som virksomheten kan fritt opprette for å aksessere tjenester og utføre handlinger på vegne av virksomheten selv eller en eller flere av virksomhetens kunder uten menneskelig interaksjon, gjennom at den gis rettigheter i Altinn på samme måte som vanlige ansatte/personer. Kan sammenlignes med en "service principal" i Azure Active Directory. 

## Tjenestetilbyder

En _tjenestetilbyder_ er en etat eller offentlig virksomhet som har behov for å føre dialog med publikum i forbindelse med forvaltning av et eller flere lovverk. I noen tilfeller er det private aktører som tilbyr tjenesten på vegne av en offentlig virksomhet.

## Part

En _part_ er en person, enten fysisk eller juridisk, som tjenestetilbyderen har dialog med.

## Dialogtjeneste

En _dialogtjeneste_ er en digital tjeneste, typisk på web, hvor tjenestetilbyderen kommuniserer med en part i forbindelse med forvaltning av et eller flere lovverk. Tradisjonelt innebærer en dialogtjeneste bruk av ulike skjema, hvor parten - gjennom et GUI levert av tjenestetilbyderen og/eller et API levert av tjenestetilbyderen og tatt i bruk av et sluttbrukersystem eller fagsystem. En dialogtjeneste har gjerne definert flere ulike trinn som typisk gjøres sekvensielt (men som i noen tilfeller kan ha parallelle "spor" som involverer flere parter).

Som det pekes på i avsnittet [Scenarioer som påvirker Dialogporten](#scenarioer-som-påvirker-dialogporten) vil en dialogtjeneste i denne konteksten også kunne dekke behovet tjenestetilbydere har for å kunne dele informasjon, altså det som typisk kalles "digital post".

## Dialog

Dialogen er en abstrakt og felles modell for alle pågående eller avsluttede [spesialiserte dialoger](#spesialisert-dialog) hos en tjenestetilbyder, og inneholder beskrivende metadata, f.eks. hvem som er mottakende part, adresse (URL), overskrift, dato, status samt en liste over aktuelle _handlinger_ som kan utføres av brukeren. Dialogporten knytter semantikk kun til slette-handlinger, hvis dette gjøres tilgjengelig av tjenestetilbyder. Andre handlinger kan vilkårlig defineres av tjenestetilbyder, og all interaksjon med den dialogen foregår i tjenestetilbyders brukerflater (unntaket er GUI-handlinger som går gjennom bakkanal, se mer om dette i avsnittet [Hendelser](#hendelser)).

En viktig forskjell mot dagens «correspondence» i Altinn, er at dialogene i Dialogporten er _mutérbare_. Tjenestetilbyder kan når som helst oppdatere metadata og tilgjengelige handlinger på dialogen. Enhver endring fører til at det genereres _hendelser_, som autoriserte parter kan agere på, f.eks. at det sendes et varsel eller at et SBS foretar seg noe.

Dialoger har en UUID som identifikator. Tjenesteeier kan selv oppgi ønsket UUID ved opprettelse for å gjøre det mulig å bruke samme identifikator på tvers av systemer.

## Spesialisert dialog

Med _spesialisert dialog_, refereres det til en konkret dialog (f.eks. innsending av Skattemelding) mellom en tjenestetilbyder og en eller flere parter, og som typisk refererer en saksgang eller prosess hos tjenestetilbyderen, og/eller realiserer et behov parten har for innsyn i opplysninger hos tjenestetilbyder. En spesialisert dialog kan ses på som en tilstandsfull "instans" av en dialogtjeneste. All håndtering og forretningslogikk/semantikk knyttet til en spesialisert dialog håndteres av tjenestetilbyderen.

## Dialogelement

Dialogelementer utgjør distinkte bestanddeler av en dialog og kan brukes i komplekse dialoger hvor det kan være hensiktsmessig for sluttbrukere og systemer å forholde seg til enkelte deler av dialogen i tillegg til dialogen som helhet. Dette kan være meldinger og pre-utfylte skjemaer fra tjenestetilbyder, innsendte skjemaer fra parten, kvitteringer, strukturerte feilmeldinger, rapporter, ustrukturerte vedlegg til meldinger etc. som utgjør en del av den totale dialogen. Dialogelementer blir typisk referert av innslag i activityHistory. Handlinger kan også referere et enkelt dialogelement. 

## Dialoggruppe (DG)

Enkelte typer saksganger består av flere distinkte delprosesser/dialoger som ikke enkelt eller hensiktsmessig kan knyttes til en og samme dialog, f.eks. når det er ulike dialoger som må gjennomføres med ulike parter og som ikke nødvendigvis skal foregå sekvensielt.

Alle dialoger kan referere en dialoggruppe (DG), som knytter disse sammen. En dialoggruppe er ikke en egen entitet, men er en rik attributt på dialogen som lar GUI-implementasjoner gruppere/sammenknytte dialoger som logisk hører sammen.

## Hendelser

_Hendelser_ refererer til tekniske applikasjonshendelser som genereres av Dialogporten (eller tjenestetilbyder) og publiseres gjennom [Event-komponenten i Altinn](https://docs.altinn.studio/technology/solutions/altinn-platform/events/). Hendelser kan konsumeres av autoriserte parter og tjenestetilbyder, i tråd med de autorisasjonsregler tjenestetilbyder har definert.

Se også [Integrasjon med event-komponenten](#integrasjon-med-event-komponent) for tekniske detaljer.

## Handlinger

En _handling_ (som i «action») beskriver en interaksjon som brukere kan gjøre med eller relatert til en dialog. Eksempler på handlinger er «Åpne», «Arkiver», «Slett», «Start signering», «Betal», «Bekreft», «Les mer» etc. Listen over aktuelle handlinger er en del av den strukturerte beskrivelsen av en dialogen, og kan når som helst endres av tjenestetilbyder gjennom API.

En handling er enten en _«GUI»-handling_ eller en _«API»-handling_. Alle handlinger - både GUI og API - har en identifikator som mappes til en _action_ (og valgfritt et _autorisasjonsattributt_, indikert av feltet `authorizationAttribute`) i _autorisasjonspolicyen_ (XACML) som er knyttet til en _tjenesteressurs._ 

### GUI-handlinger

GUI-handlinger gjøres synlige for brukeren i form av knapper, lenker eller lignende. Tjenestetilbyderen oppgir selv om en gitt handling er å regne som en primær-, sekundær eller tertiær-handling, noe som påvirker hvordan dette presenteres til brukeren. En primærhandling vil typisk presenteres som en fremhevet knapp («call to action»), og benyttes for det som er det logiske neste steget. En sekundærhandling (f.eks. «Avbryt») kan være en mer nedtonet knapp, eller tekstlenker, mens en tertiærhandling (f.eks. «Les mer om denne tjenesten») kan gjemmes bak en nedtrekksmeny eller lignende. Alt dette vil være opp til det aktuelle GUI-et som benyttes å vurdere, og ulike vurderinger vil kunne gjøres avhengig av "view" - altså kontekst, tenkt brukergruppe m.m.

Alle GUI-handlinger har en URL. Disse URLene blir kalt i det brukeren aktiverer den aktuelle handlingen. Typisk innebærer dette at brukeren blir omdirigert til etatens egen brukerflate hvor den aktuelle handlingen da utføres, enten automatisk eller gjennom videre brukerinteraksjon. Andre handlinger kan markeres at de skal utføres gjennom bakkanal-kall. Brukeren blir da ikke omdirigert, men Dialogporten vil da foreta en forespørsel på vegne av brukeren til den oppgitte URL-en. Tjenestetilbyderen returnerer da den oppdaterte dialogen, som umiddelbart blir vist brukeren igjen. Ved feil (enten av tekniske eller forretningslogiske årsaker) kan en feilmelding vises.

Det er kun én GUI-handling som Dialogporten knytter semantikk til; slett. Denne fungerer som andre handlinger, men vil alltid innebære et bakkanal-kall, som hvis vellykket, fører til at dialogen blir markert som slettet i Dialogporten, og da typisk flyttes til en "papirkurv" eller lignende. Felles Arbeidsflate vil også kunne knytte ekstra UI-logikk til disse handlingene (f.eks. vise en «Er du sikker?» dialog i forkant).

### API-handling

En API-handling er tiltenkt SBS-er og portaler som benytter Dialogporten gjennom en egen integrasjon. API-handlinger er versjonerte, og inneholder en liste med endepunkter som kan benyttes, evt. med informasjon om et endepunkt er under utfasing og når dette vil skje. Hver handling inneholder også en identifikator som indikerer hva slags type handling det er snakk om, og hvert endepunkt indikerer hvilken URL som må kalles for å utføre handlingen. Endepunktet inneholder også informasjon om hvilken HTTP-operasjon som skal benyttes (typisk GET eller POST), og valgfritt en lenke til en strukturert beskrivelse (JSON Schema) av datamodellen som enten returneres eller forventes som input, som kan brukes for dokumentasjonsformål.

{% include note.html type="info" content="Dialogporten foretar ikke validering av noe data, og ser ikke hvilke data som flyter mellom SBS-et og tjenestetilbyderens API." %}

## Tjenesteressurs

Alle dialoger må referere en _tjenesteressurs_. En tjenesteressurs utgjør autorisasjonsbæreren, og kan sammenlignes med moderne bruk av lenketjenester i Altinn 2. Dette er en beskrivelse av en tjeneste som ligger i [Altinn Resource Registry](https://docs.altinn.studio/authorization/modules/resourceregistry/), en ny komponent i Altinn Autorisasjon. Hver tjenesteressurs har en autorisasjonspolicy uttrykt i [XACML](https://docs.altinn.studio/authorization/guide/xacml/), som beskriver hvilke tilgangsregler som gjelder for alle dialoger som refererer den. XACML gir stor fleksibilitet i hvor grov- eller finkornet tilgangskontrollen skal være, og Dialogporten vil legge denne policyen til grunn for å bestemme hvem som kan se en gitt dialog, og hvilke handlinger som skal være tilgjengelige. 

Eksempelvis vil GUI-handlingen «Signer» referere en _action_ kalt «sign» i XACML-policyen, som krever tilganger den innloggende brukeren ikke besitter. Knappen vil derfor kunne være grået ut og deaktivert. Tjenesteressursen er det tilgangsstyrere i virksomhetene forholder seg til, mht hvem som skal ha tilgang til å gjøre hva på vegne av en virksomhet (tilsvarende dagens tjenestedelegering).

## Autorisasjonsattributt

Handlinger og andre deler (f.eks. referanser til dialogelementer) av dialogen kan også oppgi et ekstra _autorisasjonsattributt_ gjennom feltet `authorizationAttribute`.

Dette muliggjør at man kan ha ulike autorisasjonskrav for samme type handling som er tilgjengelige ved ulike tilstander dialogen har. F.eks. vil det kunne brukes for å la en signeringshandling kun være tilgjengelig for en ekstern revisor/regnskapsfører, mens en annen signeringshandling er tilgjengelig for daglig leder.

## Aktivitet

En _aktivitet_ beskriver en eller annen utført handling eller hendelse som har skjedd i tilknytning til dialogen. Hver aktivitet inngår i aktivitetshistorikken, som er en kronologisk liste over aktiviteter. Det er tjenestetilbyder som populerer aktivitetshistorikken etter hvert som ulike tilstandsendringer inntreffer. Dialogporten kan også foreta innslag i aktivitetshistorikken, f.eks. om en dialog blir åpnet for første gang, eller tilgang til dialogen delegeres videre.

# Begrepsmodell

Under er en forenklet modell som viser relasjonene mellom de ulike begrepene. For en mer overordnet modell av konseptet, se [Overordnet modell over konsept](#overordnet-diagram-over-konsept).

![](https://lucid.app/publicSegments/view/763e5fd0-eb36-4d9a-becb-efe9b76ab3dc/image.png)

# Overordnet modell over konsept

Under er en overordnet modell som viser relasjonen mellom Dialogporten, tjenesteplattformene som benytter den og sluttbrukere/systemer som konsumerer dialogene som er realisert.

![](https://lucid.app/publicSegments/view/c3ce275d-9170-45d0-8c09-e50de2ffc690/image.png)

# Teknisk ER-diagram

Under er et ER-diagram som viser de konkrete relasjonene mellom de ulike entitetene som er definert. Merk at disse kan ha avvik i navngivning i forhold til modellene og beskrivelsene i dette dokumentet

<img src="{{ "/assets/dialogporten-er.svg" | relative_url }}">

# Autorisasjon

## Relasjon til XACML

Hver dialog refererer en [_tjenesteressurs_](https://docs.altinn.studio/authorization/modules/resourceregistry/) i [Altinn Autorisasjon](https://docs.altinn.studio/authorization/), som består av en XACML-policy samt metadata som beskriver tjenesten. 

Hver action som defineres på en dialog mappes til en tilsvarende "action-id" i XACML. Eksempelvis, gitt følgende action på en dialog som refererer tjenesteressursen `min_fine_tjeneste`:

```jsonc
{
    // ...
    "serviceResource": "min_fine_tjeneste", 
    // ...
    "actions": {
        "gui": [ 
            { 
                "action": "open", // Denne refereres i "Action"-delen  i XACML-policy                
                "title": [ { "code": "nb_NO", "value": "Åpne" } ],
                "url": "https://example.com/some/deep/link/to/dialogs/123456789"
            }
        ]
    }
}
```

Hvis man ønsker å lage en regel i en policy som gir innehavere av Enhetsregister-rollen `DAGL` (daglig leder) tilgang til en action `open` på dialoger som refererer tjenesteressursen med identifikator `min_fine_tjeneste`, kan dette uttrykkes i XACML som:

```xml
<xacml:Rule RuleId="urn:altinn:example:ruleid:1" Effect="Permit">
    <xacml:Target>
        <!-- Subjekt-del -->
        <xacml:AnyOf>
            <xacml:AllOf>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:3.0:function:string-equal-ignore-case">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">DAGL</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:altinn:rolecode" Category="urn:oasis:names:tc:xacml:1.0:subject-category:access-subject" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
            </xacml:AllOf>
        </xacml:AnyOf>
        <!-- Ressurs-del -->
        <xacml:AnyOf>
            <xacml:AllOf>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">min_fine_tjeneste</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:altinn:resource" Category="urn:oasis:names:tc:xacml:3.0:attribute-category:resource" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
            </xacml:AllOf>
        </xacml:AnyOf>
        <!-- Action-del -->
        <xacml:AnyOf>
            <xacml:AllOf>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">open</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:oasis:names:tc:xacml:1.0:action:action-id" Category="urn:oasis:names:tc:xacml:3.0:attribute-category:action" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
            </xacml:AllOf>
        </xacml:AnyOf>
    </xacml:Target>
</xacml:Rule>
```
Dette kan uttrykkes i [forenklet JSON](https://github.com/Altinn/altinn-studio/issues/5016) som:

```jsonc
{
    "Rules": [
        {
            "Subjects": [
                "urn:altinn:rolecode:DAGL",
            ],
            "Resources": [
                [
                    "urn:altinn:resource:min_fine_tjeneste"
                ]
            ],
            "Actions": [
                "open"
            ]
        }
}
```

### Autorisasjonsattributter
Autorisasjonsattributter er ytterligere ressurs-attributter som det vil matches mot i en regel i en policy, som kan refereres til i actions og dialogelementer gjennom feltet `authorizationAttribute`. En slik ressurs er en URN som i eksemplene i dette dokumentet tilhører navnerommet `urn:altinn:subresource`, men andre navnerom kan også benyttes; f.eks. bruker Altinn Studio vanligvis navnerommet `urn:altinn:task`. 

En action som refererer en subsressurs kan se slik ut:


```jsonc
{
    // ...
    "serviceResource": "min_fine_tjeneste", 
    // ...
    "actions": {
        "gui": [ 
            { 
                "action": "sign", // Denne refereres i "Action"-delen  i XACML-policy                
                "authorizationAttribute": "urn:altinn:subresource:autorisasjonsattributt1", // Denne refereres i "Ressurs"-delen  i XACML-policy                
                "title": [ { "code": "nb_NO", "value": "Åpne" } ],
                "url": "https://example.com/some/deep/link/to/dialogs/123456789"
            }
        ]
    }
}
```

Tilsvarende, for å lage en regel som gir `DAGL` lov til å utføre handlingen `sign` på autorisasjonsattributtet `urn:altinn:subresource:autorisasjonsattributt1` på dialoger som refererer `min_fine_tjeneste`:

```xml
<xacml:Rule RuleId="urn:altinn:example:ruleid:1" Effect="Permit">
    <xacml:Target>
        <!-- Subjekt-del -->
        <xacml:AnyOf>
            <xacml:AllOf>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:3.0:function:string-equal-ignore-case">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">DAGL</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:altinn:rolecode" Category="urn:oasis:names:tc:xacml:1.0:subject-category:access-subject" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
            </xacml:AllOf>
        </xacml:AnyOf>
        <!-- Ressurs-del -->
        <xacml:AnyOf>
            <xacml:AllOf>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">min_fine_tjeneste</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:altinn:resource" Category="urn:oasis:names:tc:xacml:3.0:attribute-category:resource" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">autorisasjonsattributt1</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:altinn:subresource" Category="urn:oasis:names:tc:xacml:3.0:attribute-category:resource" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
            </xacml:AllOf>
        </xacml:AnyOf>
        <!-- Action-del -->
        <xacml:AnyOf>
            <xacml:AllOf>
                <xacml:Match MatchId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">sign</xacml:AttributeValue>
                    <xacml:AttributeDesignator AttributeId="urn:oasis:names:tc:xacml:1.0:action:action-id" Category="urn:oasis:names:tc:xacml:3.0:attribute-category:action" DataType="http://www.w3.org/2001/XMLSchema#string" MustBePresent="false"/>
                </xacml:Match>
            </xacml:AllOf>
        </xacml:AnyOf>
    </xacml:Target>
</xacml:Rule>
```
Dette kan uttrykkes i [forenklet JSON](https://github.com/Altinn/altinn-studio/issues/5016) som:

```jsonc
{
    "Rules": [
        {
            "Subjects": [
                "urn:altinn:rolecode:DAGL",
            ],
            "Resources": [
                [
                    "urn:altinn:resource:min_fine_tjeneste",
                    "urn:altinn:subresource:autorisasjonsattributt1",
                ]
            ],
            "Actions": [
                "sign"
            ]
        }
}
```

{% include note.html type="info" content="Merk at XACML gjør matching på attributter. Så en regel som gir f.eks. `sign` uten å oppgi et ekstra autorisasjonsattributt, vil da gi tilgang til alle `sign`-actions uavhengig av hvilket autorisasjonsattributt som er definert på action-en." %}

## Bruk av Dialogportens API-er

Tjenesteressursen bestemmer hvilke autorisasjonskontroller som skal legges til grunn for å få tilgang. Typisk innebærer dette at konsumenten innehar en spesiell rolle eller tilhører en spesifikk forhåndsdefinert tilgangsgruppe, eller er blitt delegert tilgang til tjenesteressursen dialogen refererer av en tilgangsstyrer/hovedadministrator hos den aktuelle parten.

For API-konsumenter krever Dialogporten at klienten oppgir et token med et spesifikt scope; `digdir:dialogporten`. Dette scopet kan for SBS-er/GUI-implementasjoner være tildelt klienten gjennom brukerstyrt datadeling i ID-porten (OAuth2/OIDC) eller ved hjelp av Maskinporten (ren OAuth2). Ved bruk av Maskinporten-token, vil det typisk kreves at det i tillegg autentiseres en systembruker, som innbærer utstedelse av et beriket Maskinporten-token som benyttes mot Dialogporten og/eller tjenestetilbyders API-er. Tjenestetilbydere blir tildelt et eget scope; `digdir:dialogporten.serviceprovider` som kun er tilgjengelig for Maskinporten-integrasjoner. Dette gir skrivetilgang til Dialogporten og mulighet til å hente ut (og oppdatere) alle dialoger som er opprettet av tjenestetilbyderen. Liste/søke-API-et krever et ytterligere scope; `digdir:dialogporten.serviceprovider.search`. Tjenestetilbyder kan også konfigurere [ytterligere scopekrav for dialoger](#selvpålagt-scopekrav-på-dialog) som egne integrasjoner må inneha for å kunne aksessere dialoger.

Felles arbeidsflate vil av hensyn til brukskvalitet ikke kreve eksplisitt autorisasjon fra sluttbrukeren for tilgang til Dialogporten-API-ene; dette skjer implisitt gjennom innlogging i ID-porten, og Felles arbeidsflate vil bruke et internt scope for å autorisere seg mot Dialogportens API-er.

## Bruk av samme token mot både Dialogporten og tjenestetilbyders API

Tjenestetilbyders API-er vil typisk kreve egne scopes som grovkornet autorisasjon. I disse tilfellene vil det kunne utstedes tokens som benyttes mot både Dialogportens API såvel som tjenestetilbyderen, altså scopes for begge API-ene. Dialogporten vil ikke kreve eller verifisere "aud"-claims i tokens, men disse kan benyttes hvis tokenet også skal benyttes mot tjenestetilbyders endepunkter. Klienter må imidlertid vurdere sikkerhetsrisikoen knyttet til at tjenestetilbyder da vil kunne gjenbruke tokenet mot Dialogporten (replay attack). For høyest sikkerhet må klienten hente ut to tokens for bruk mot hhv. Dialogporten API og tjenestetilbyder.

## Selvpålagt scopekrav på dialog

{% include note.html type="info" content="Denne funksjonaliteten er tenkt som en mekanisme som benyttes i tillegg til ekstra regler som ligger på tjenesteressursen, og/eller dens policy. Dette er foreløpig ikke utredet." %}

Tjenestetilbydere med et stort antall tjenester implementert over ulike systemer kan av sikkerhetshensyn ønske å begrense hver enkelt integrasjons tilgang til Dialogportens privilegerte API-er for å isolere dialoger tilhørende ulike tjenester (eller sikkerhetsdomener) fra hverandre. Ved opprettelse/endring av dialogen, kan et felt i `configuration`-feltet settes som inneholder en liste over ytterligere Maskinporten-scopes som tjenestetilbyder må ha i oppgitt token for å kunne aksessere den aktuelle dialogen. Dette scopet må også være oppgitt i token ved opprettelse, og ved endring må alle scopes som både i eksisterende og ny liste være oppgitt. På denne måten kan tjenestetilbyder provisjonere ulike Maskinporten-klienter som kan ha tilgang til ulike subsets av dialoger eid av tjenestetilbyderen.

## Håndhevelse av autorisasjon hos tjenestetilbyder

Tokens utstedt av Maskinporten vil inneholde en systembruker-id, mens tokens utstedt av ID-porten vil inneholde et fnr/dnr. Det pålegges tjenestetilbyder å håndheve autorisasjon mot egne endepunkter gjennom at det foretas oppslag mot Altinn Autorisasjon hvor man verifiserer tilgangen en gitt systembruker-id eller fnr/dnr har for endepunktet/operasjonen som er forsøkt aksessert/utført. For GUI-endepunkter som aksesserer av sluttbruker gjennom felles arbeidsflate, vil single-sign-on (SSO) i ID-porten sørge for at tjenestetilbyder får autentisert fnr/dnr som står bak requesten og kan foreta autoriasjonsoppslag/håndhevelse som oppleves sømløst for sluttbruker. 

Tokens mottatt fra Maskinporten eller ID-porten vil på sikt kunne berikes med finkornede autorisasjonsopplysninger basert på Rich Authorization Requests (RAR) fra SBS-et som Maskinporten/ID-porten verifiserer opp mot Altinn Autorisasjon som foretar en autorisasjonsbeslutning. Hvis tilgang er gitt, populeres tokenet med opplysninger som vil kunne brukes av både Dialogporten og tjenestetilbyder for å håndheve tilgangskontroll uten videre oppslag mot Altinn Autorisasjon (innenfor levetiden av tokenet).

## Integrasjonsmønster for SBS-er

{% include note.html type="info" content="Dette er avsnittet som tidligere var kalt &quot;Varianter for autorisasjon&quot;" %}

{% include note.html type="info" content="Under beskrives autentiserings- og autorisasjonsmønstre som involverer Maskinporten/ID-porten og Altinn Autorisasjon. Avsnittet beskriver hvordan dette vil kunne implementeres med Dialogporten, men ingenting representerer Dialogporten-spesifikk funksjonalitet" %}

Under beskrives hvordan SBS-er foretar autentisering for å kunne autoriseres i både Dialogporten og hos tjenestetilbyders API-er.

### Ting som legges til grunn / avgrensninger
* Det finnes en systembruker-mekanisme som en beskrankningsmekanisme for virksomheter
* Hvem som "eier" denne, eller hvordan den provisjoneres er out-of-scope.
* Det opereres med tre nivåer av autorisasjon
  * Scope-nivå
    * I Dialogporten er denne grovkornet (f.eks. `digdir:dialogporten` eller `altinn:instances.read`), og autoriserer kun for å kunne kalle API-et. Gir i seg selv ikke tilgang til noen tjenester. 
    * Scopes tolkes typisk mer finkornet hos tjenestetilbyder, som gjerne har scopes per tjeneste (f.eks. `skatteetaten:summertskattegrunnlag`).
  * Tjenestenivå
    * Har tilgang til en eller flere actions på en tjeneste og/eller definert autorisasjonsattributt (egen ressurs-attributt i XACML, tradisjonelt "prosessteg" i Altinn2) av tjeneste
  * Dialognivå
    * Tilgang til konkret instans, aka "instansdelegering". 
    * Tilgang på tjenestenivå gir tilgang til alle dialoger, men noen kan ha tilgang til en eller flere actions til enkelte dialoger og/eller tilhørende definerte autorisasjonsattributter.
 * Det tas utgangspunkt i ikke-interaktive, virksomhetsautentiserte flyter med Maskinporten som IDP. Det er derfor fem prinsipielle aktører; sluttbrukersystemet, Dialogporten, Maskinporten, Altinn Autorisasjon og Tjenestetilbyders API for tjenesten, samt Altinn Token Exchange + Altinn Registry for håndtering av systembrukere. 
 * Varianter med ID-porten vil kunne fungere annerledes (f.eks. faller Token Exchange ut, siden man umiddelbart har en "bruker"), avhengig av grad av interaktivitet. Disse er ikke tegnet inn i denne omgang.
 * Bruk av flere tokens eller `aud`-claim forutsettes for å unngå problematikk rundt replay-angrep.

### Maskinporten-token med systembruker-ID

{% include note.html type="info" content="Dette er tidligere omtalt som &quot;Variant D&quot;, og er det mønsteret Dialogporten vil implementere først" %}

Maskinporten foretar autentisering av systembruker/passord og utsteder et beriket token med identifikator for systembrukeren. Både tjenestetilbyder og Dialogporten må foreta oppslag mot Altinn Autorisasjon for å autorisere den oppgitte systembrukeren på tjenestenivå.

```mermaid!
sequenceDiagram
autonumber
    participant SBS as Sluttbrukersystem
    participant MP as Maskinporten
    participant DP as Dialogporten
    participant AA as Altinn Autorisasjon    
    participant AR as Altinn Registry
    participant TT as Tjenestetilbyder
SBS->>MP: Forespør MP-token (med RAR?) for Dialogporten inkl. systembruker + MP-token m/scope for tjeneste
opt Hvis leverandør
    MP->>AA: Sjekke delegeringer på scopes
    AA->>MP: Returner autorisasjonsbeslutning
end
MP->>AR: Autentiserer systembruker
AR->>MP: Returner autentiseringsbeslutning
MP->>SBS: Utsteder beriket MP-token for Dialogporten/systembruker + MP-token m/scope for tjeneste
opt Hvis ikke SBS kjenner til dialog-ID / actions
    SBS->>DP: Henter dialog med beriket MP-token
    DP->>AA: Forespør autorisasjon for dialog for systembruker
    AA->>DP: Returnerer autorisasjonsbeslutning for dialog
    DP->>SBS: Returnerer dialog
end 
SBS->>TT: Foretar handling for dialog med beriket MP-token imkl systembruker + MP-token m/scope for tjeneste
TT->>TT: Validerer beriket MP-token og MP-token m/scope for tjeneste
TT->>AA: Forespør autorisasjon for dialog for systembruker
AA->>TT: Returnerer autorisasjonsbeslutning for dialog
TT->>SBS: Returner respons på handling
```

### Maskinporten-token med innbakt autorisasjon 

{% include note.html type="info" content="Dette er tidligere omtalt som &quot;Variant C&quot;, og representerer et mulig fremtidig målbilde hvor Maskinporten har en større rolle knyttet til å foreta finkornet autorisasjon. Ved første iterasjon av Dialogporten vil ikke dette mønsteret være realisert." %}

I dette mønsteret oppgir SBS systembruker + passord, samt tjenesteressurs i forespørselen til Maskinporten, som da foretar både grov- og finkornet autorisasjon. Dette krever innføring av RAR (Rich Authorization Requests) for Maskinporten, og en tettere kobling mellom Maskinporten og Altinn Autorisasjon. Samme token-type kan benyttes mot både Tjenestetilbyder og Dialogporten, men `aud`-claim må settes i token og valideres for å unngå å åpne for replay-attacks.

```mermaid!
sequenceDiagram
autonumber
    participant SBS as Sluttbrukersystem
    participant MP as Maskinporten
    participant DP as Dialogporten
    participant AA as Altinn Autorisasjon    
    participant AR as Altinn Registry
    participant TT as Tjenestetilbyder
SBS->>MP: Forespør MP-token med RAR for Dialogporten inkl. systembruker og tjenesteressurs + MP-token m/scope for tjeneste
MP->>AA: Forespør delegering på scope + autorisasjon for dialog for systembruker
AA->>AR: Autentiserer systembruker
AR->>AA: Returner autentiseringsbeslutning (systembruker-ID)
AA->>MP: Returner autorisasjonsbeslutning
MP->>SBS: Utsteder beriket MP-token for Dialogporten/systembruker/tjeneste + MP-token m/scope for tjeneste
opt Hvis ikke SBS kjenner til dialog-ID / actions
    SBS->>DP: Henter dialog med beriket MP-token
    DP->>DP: Validerer beriket MP-token
    DP->>SBS: Returner dialog
end 
SBS->>TT: Foretar handling for dialog med beriket MP-token + MP-token m/scope for tjeneste
TT->>TT: Validerer beriket MP-token og MP-token m/scope for tjeneste
TT->>SBS: Returner respons på handling
```
# Integrasjon med event-komponent

Dialogporten vil generere events automatisk basert på endringer som gjøres på dialog-entiteten. Det vil også gjøres genereres events for hvert separate innslag i aktivitetshistorikken. Se 

Se [case-01]({% link _dialogporten_case/example-case-01.md %}) for eksempler på hvordan events genereres.

## Knyttet til dialog-entiteten

Opprettelse/endring/sletting av dialog-entiteten genererer meldinger med følgende typer:

| Type                             | Når                            |
| -------------------------------- | ------------------------------ |
| `dialogporten.dialog.created.v1` | Dialogen opprettes             |
| `dialogporten.dialog.updated.v1` | Tilstand på dialogen er endret |
| `dialogporten.dialog.deleted.v1` | Dialogen slettes               |

I tillegg genereres det en event med type `dialogporten.dialog.read.v1` når dialogen lastes for første gang etter at den er oppdatert (altså readDateTime < max(updatedDateTime, createdDateTime).

### Eksempel

```jsonc
{
    "specversion": "1.0",

    // Unik event-id
    "id": "91f2388f-bd8c-4647-8684-fd9f68af5b15",
    
    // Se "Type" i tabell over for liste over mulige hendelser
    "type": "dialogporten.dialog.created.v1",
    
    // Timestamp for når hendelsen inntraff i Dialogporten
    "time": "2023-02-20T08:00:06.4014168Z",
    
    // urn:altinn:resource:{serviceResource}
    "resource": "urn:altinn:resource:super-simple-service", 
    
    // Dialog-ID
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    
    // Party
    "subject": "org/91234578", 
    
    // URL til dialog i Dialogporten. Merk denne vil gi 410 Gone hvis type er `dialogporten.dialog.deleted.v1`
    "source": "https://dialogporten.no/api/v1/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20" 
}
```

## Knyttet til aktivitetshistorikken 

Det vil alltid bli generert egne events for hvert nye innslag i aktivitetshistorikken. `source` vil referere selve innslaget (ikke dialogen). Det vil også inkluderes en `data` som inneholder ytterligere informasjon som i mange tilfeller lar systemer kunne agere direkte mot tjenesteeiers API-er uten å måtte hente dialog/aktivitet først.

Eventer har type som er prefikset/navnerommet `dialogporten.dialog.activity`. Suffikset er gjenspeiler mulige verdier av `activityType`.

| Type                                          | Når                                                                   |
| --------------------------------------------- | --------------------------------------------------------------------- |
| `dialogporten.dialog.activity.submission.v1`  | Tjenestetilbyder har lagt til en aktivitet av type `submission`       |
| `dialogporten.dialog.activity.feedback.v1`    | Tjenestetilbyder har lagt til en aktivitet av type `feedback`         |
| `dialogporten.dialog.activity.information.v1` | Tjenestetilbyder har lagt til en aktivitet av type `information`      |
| `dialogporten.dialog.activity.error.v1`       | Tjenestetilbyder har lagt til en aktivitet av type `error`            |
| `dialogporten.dialog.activity.closed.v1`      | Tjenestetilbyder har lagt til en aktivitet av type `closed`           |
| `dialogporten.dialog.activity.seen.v1`        | Dialogen åpnes/lastes for første gang (etter en `feedback`-aktivitet) |
| `dialogporten.dialog.activity.forwarded.v1`   | Dialogen er videresendt/delegert til noen andre.                      |

### Eksempel

```jsonc
{
    "specversion": "1.0",

    // Unik event-id
    "id": "91f2388f-bd8c-4647-8684-fd9f68af5b15",
    
    // Se "Type" i tabell over for liste over mulige hendelser
    "type": "dialogporten.dialog.activity.submission.v1",
    
    // Timestamp for når hendelsen inntraff i Dialogporten
    "time": "2023-02-20T08:00:06.4014168Z",
    
    // urn:altinn:resource:{serviceResource}
    "resource": "urn:altinn:resource:super-simple-service", 
    
    // Dialog-ID
    "resourceinstance": "f4e6df3c-7434-44c3-875e-8dca1cdf0b20",
    
    // Party
    "subject": "org/91234578",

    // URL til aktivitetshistorikk-innslag i Dialogporten
    "source": "https://dialogporten.no/api/v1/dialogs/f4e6df3c-7434-44c3-875e-8dca1cdf0b20/activityhistory/21241c7e-819f-462b-b8a4-d5d32352311a",
    
    // Disse hentes verbatim fra aktivitetshistorikk-innslaget. Kun "activityId" er alltid oppgitt, alle andre felter
    // vil kunne være utelatt siden de er valgfrie i activityHistory
    "data": { 
        "activityId": "21241c7e-819f-462b-b8a4-d5d32352311a",
        "relatedActivityId": "cfdd19a4-8cf9-4138-9930-36478fdce398",
        "extendedActivityType": "additional-info-received",
        "dialogElementId": "54ae8387-7320-4693-8423-0ceb0efaf5fa"
    }
} 
```

## Autorisasjon knyttet til produsering/konsumering av events

Tilgang til events reguleres også av den samme autoriasjonspolicyen for tjenestressursen som dialogene benytter, gitt gjennom `resource`. Dialogporten har imidlertid en privilgert tilgang til eventkomponenten som gjør at den kan generere events for alle tjenesteressurser, som da innebærer at det ikke behøves å opprettes egne regler i policyen for Dialogporten.

For alle andre konsumenter reguleres konsum (herunder opprettelse av abonnement) av actionen `subscribe`. Hvis tjenesteeier (eller andre parter) skal kunne produsere vilkårlige events, må det opprettes regler med action `publish`. [Les mer om dette i dokumentasjonen for event-komponenten](https://docs.altinn.studio/events/publish-events/get-started/resource-registry/).

Et eksempel:

```jsonc
{
    "Rules": [
        // Gir ID-porten-autentiserte personer med DAGL hos party og Maskinporten-autentiserte 912345678 tilgang 
        // til å konsumere events av type "dialogporten.dialog.activity.submission.v1" for ressursen
        // "urn:altinn:resource:super-simple-service"
        {
            "Subjects": [
                "urn:altinn:rolecode:DAGL",
                "urn:altinn:organizationnumber:912345678",
            ],
            "Resources": [
                [
                    "urn:altinn:resource:super-simple-service",

                    // Regler kan også begrenses på event-type
                    "urn:altinn:eventtype:dialogporten.dialog.activity.submission.v1",
                ]
            ],
            "Actions": [
                "subscribe"
            ]
        },
        // Gir Maskinporten-autentiserte 912345678 tilgang til å produsere hvilken som helst event-type for ressursen
        // "urn:altinn:resource:super-simple-service"
        {
            "Subjects": [
                "urn:altinn:organizationnumber:912345678",
            ],
            "Resources": [
                [
                    "urn:altinn:resource:super-simple-service",
                ]
            ],
            "Actions": [
                "publish"
            ]
        }
}
```

Les mer om [Event-komponenten på Altinn Docs](https://docs.altinn.studio/events/).



# Sekvensbeskrivelser

Under følger punktvis beskrivelser av fire ulike sekvenser. Det er to måter en dialog kan initieres på; enten av sluttbruker eller tjenestetilbyder. Hver av disse kan gjennomføres ved hjelp av GUI eller API.

## Tjenestetilbyder-initiert dialog

### Opprettelse av dialog 

#### Sekvensdiagram

```mermaid!
sequenceDiagram
    participant TE as Tjenestetilbyders integrasjon mot Dialogporten
    participant API as Dialogporten API
    participant ARR as Altinn Resource Registry
    participant EVT as Altinn Event Component
    participant NOT as Altinn Notification Component
    participant EVTRCPT as Hendelesesabonnent
    actor NOTRCPT as Varselsmmottaker

note over TE,NOTRCPT: Opprettelse av tjenesteressurs (gjøres én gang per tjeneste)
TE->>ARR: Oppretter tjenesteressurs
ARR->>TE: Returnerer tjenesteressurs-identifikator
note over TE,NOTRCPT: Opprettelse av dialog
TE->>API: Opprette dialog (eventuelt med egen-generert identifikator)
API->>TE: Returnere OK
par
    API->>EVT: Generer hendelse for opprettet dialog
    and
    API->>NOT: Sende evt. varsler indikert av tjenestetilbyder ved opprettelse, samt brukerstyrte varsler
end
par
    EVT->>EVTRCPT: Push maskinlesbar hendelse
    and
    NOT->>NOTRCPT: Send varsel på e-post/SMS el.l.
end

note over TE,NOTRCPT: Oppdatering av dialog (som følge av at brukeren foretar seg noe, eller andre hendeleser inntreffer)

TE->>API: Endrer dialog med oppgitt identifikator
API->>TE: Returnerer kvittering på at endring er gjennomført
par
    API->>EVT: Generer hendelse for opprettet dialog
    and
    API->>NOT: Sende evt. varsler indikert av tjenestetilbyder ved endring, samt brukerstyrte varsler
end
par
    EVT->>EVTRCPT: Push maskinlesbar hendelse
    and
    NOT->>NOTRCPT: Send varsel på e-post/SMS el.l.
end
```

#### Tekstlig beskrivelse av trinn

1.  Tjenestetilbyder oppretter tjenesteressurs i Altinn Resource Registry som refererer den aktuelle tjenesten
2.  Tjenestetilbyder oppretter dialog som    
    * Referer tjenesteressurs
    * Inneholder tekstlig metadata (tittel, ingress etc) i flere språk
    * Inneholder andre (valgfrie) metadata som f.eks.
    * Frist
    * "Fra"-felt
    * Dato for når dialogen skal aktiveres
    * Eventuell tilhørlighet til dialog-gruppe
    * Inneholder (valgfri) URI for å hente oppdatering strukturert versjon av dialogen, hvis hensiktsmessig for tjenesten. Brukes typisk i det brukeren ekspanderer/åpner en grafisk fremvisning av dialogen for å vise
        * Hvilke handlinger som kan utføres på dialogen (se under)
        * Hvis ikke oppgitt, vises de metadata/handlinger jf. siste oppdatering som ble gjort av tjenestetilbyder (hvis noen, etter opprettelse)
    * Beskriver tilgjengelige handlinger (for både API og GUI)
        * Strukturert liste over handlinger som kan gjøres på dialogen, typisk "Åpne", "Arkiver", "Slett", "Bekreft", "Signer", "Betal", "Les mer", etc. 
        * GUI-handlinger kan flagges som primær, sekundær og tertiær-handlinger som påvirker hvordan dette vises til bruker (f.eks. kan primær og sekundærhandlinger vises som ulike typer knapper, mens tertiærhandlinger vises i en nedtrekksliste/tekstlenker).
        * Er gjenstand for autorisasjon definert av referert tjenesteressurs og eventuelt autorisasjonsattributt. F.eks. vil f.eks. "Signer" kunne kreve andre rettigheter avhengig av policy knyttet til tjenesteressursen.
        * Hver GUI-handling inneholder
            * En identifikator for handlingen. "Standard"-handlinger vil kunne oppgis som allerede finnes oversatt i Dialogporten.
            * Hvis ikke standard-handling, tekst som beskriver handlingen i flere språk
            * En valgfri hjelpetekst som kan fremvises brukeren som gir mer informasjon om hva handlingen innebærer
            * Flagg som indikerer om handlingen skal utføres i bakkanal
            * En URI som enten (1) brukeren vil omdirigert til når hen aktiverer det aktuelle GUI-dialogen (f.eks. en knapp) eller (2) Felles Arbeidsflate vil kalle hvis på vegne av brukeren hvis flagget som bakkanal-kall
            * Hvis bakkanal-kall, skal URI-en skal returnerer en standardmodell som indikerer om kallet var vellykket, eller om det skal vises en feilmelding.
            * Flagg som indikerer om dialogen skal slettes/arkiveres i Dialogporten hvis kall til URI lykkes (vil brukes til f.eks. "Er du sikker"-prompts)
        * Hver API-handling inneholder
            * En identifikator for handlingen
            * Hvilken http-metode som skal benyttes
            * En URI som handlingen skal utføres mot
            * JSON Schema el.l. som beskriver datamodellen som skal pushes/pulles
    * Inneholder valgfri liste over dialogelementer, som består av
        * En identifikator som kan være oppgitt av tjenesteeier
        * Indikator på hva slags tjenestespesifikk type dialogelement er
        * Indikator på om dialogelementet er tiltenkt API eller GUI-kanaler
        * URL til dialogelementet hvor det kan hentes (GET) evt omdirigeres til
    * Kan muteres etter opprettelse
        * Tilgjengelige handlinger kan oppdateres når som helst av tjenestetilbyder 
        * Tittel og annen tekstlig metadata
        * Status ("under utfylling", "klar til signering", "venter på andre", "venter på svar fra tjenestetilbyder" etc) 
    * Etter vellykket opprettelse, returners en unik identifikator for dialogen (identifikator kan også opprettes av tjenestetilbyder).  
3.  Når dialogen er opprettet/endret vil det
    * Genereres hendelser som vil kunne konsumeres av parten
    * Disse kan igjen være koblet til brukerstyrt varsling på e-post/SMS andre push-kanaler 

### Konsum gjennom GUI (portal)

I beskrivelsene under brukes Felles Arbeidsflate som eksempel på en implementasjon av Dialogporten-API, men tilsvarende sekvenser kan også gjelde for andre GUI-klienter.

#### Sekvensdiagram

```mermaid!
sequenceDiagram
    actor SB as Sluttbruker
    participant GUI as Felles Arbeidsflate
    participant API as Dialogporten API
    participant AA as Altinn Autorisasjon
    participant TEGUI as Tjenestetilbyders GUI
note over SB,GUI: Bruker autentiseres via ID-porten og blir logget<br>inn i Felles Brukerflate, hvor hen autoriseres for<br>en liste med dialoger
SB->>GUI: Bruker klikker på en bestemt dialog
GUI->>SB: Viser innhold i dialog med aktuelle handlinger
SB->>TEGUI: Sluttbruker følger lenke for ønsket operasjon til tjenestetilbyders portal
note over TEGUI: Tjenestetilbyder får fnr/dnr<br>fra ID-porten gjennom SSO
TEGUI->>AA: Sjekke om fnr/dnr er autorisert for handling/ressurs
AA->>TEGUI: Returner autorisasjonsbeslutning
TEGUI->>SB: Vis arbeidsflate for tjeneste til sluttbruker
SB->>TEGUI: Foreta endringer
par
    TEGUI->>API: Oppdater dialog for å reflektere ny tilstand
    API->>TEGUI: Oppdatering OK
and
    TEGUI->>SB: Vis arbeidsflate med oppdatert tilstand
end
```

#### Tekstlig beskrivelse av trinn

1.  Bruker mottar varsling på en eller annen kanal, og logger inn i Felles Arbeidsflate
2.  Dialogen har en grafisk fremstilling som viser overskrift, status og andre metadata
3.  Bruker klikker på dialogen for å ekspandere den. Ekspandert dialog viser rikt innhold som tjenestetilbyder har definert, sammen med tilgjengelige handlinger. Hvis oppdatering feilet, vises enten feilmelding som tjenestetilbyder oppga, eller en standardfeilmelding.
4.  Bruker klikker på den definerte primærhandlingen.
    * Felles Arbeidsflate vil da redirecte brukeren (nettleseren) til oppgitt URI. 
    * Hvis ikke sesjon foreligger hos tjenestetilbyder, redirectes bruker innom ID-porten som pga. SSO umiddelbart vil redirecte bruker tilbake. Tjenestetilbyder vil da ha autentisert fnr/dnr
5. Tjenestetilbyder identifiserer {handling, part, tjenesteressurs} eller {handling, dialog-id} utfra URL som er forsøkt aksessert, og foretar en autorisasjonsforespørsel til Altinn Autorisasjon.
6.  Ved positiv autorisasjonbeslutning blir brukeren logget inn hos tjenestetilbyder og tatt inn til dialogen hos tjenestetilbyder, hvor brukeren interagerer med tjenesten. Etter hvert som dialogen skrider frem, kan tjenestetilbyder gjøre asynkrone bakkanal-kall til Dialogporten for å oppdatere dialogen slik den fremstår for brukeren.
7.  Hvis brukeren fullfører dialogen, kan tjenestetilby,der gjøre et bakkanal-kall for å indikere til Dialogporten at dialogen skal markeres som fullført. Dialoget blir da merket som fullført, og vil typisk flyttes til "arkiv"-visning i Felles arbeidsflate. Merk at det fremdeles kun ligger metadata på dialogen i Dialogporten.
8.  Når brukeren senere ekspanderer dialogen i en arkiv-visning i Felles Arbeidsflate, vises da de data som siste ble lagt inn på dialogen av tjenestetilbyder. Typisk vises da bare en kort tekst og et vedlegg til en PDF-versjon av en kvittering/gjenpart el.l. som ligger hos tjenestetilbyder.

### Konsum gjennom API

#### Sekvensdiagram

```mermaid!
sequenceDiagram
    participant SBS as Sluttbrukersystem
    participant EID as Maskinporten/ID-porten
    participant API as Dialogporten API
    participant AA as Altinn Autorisasjon
    participant TEAPI as Tjenestetilbyders API
note over SBS,TEAPI: SBS abonnerer på hendelser for opprettelse av dialoger<br>og mottar URI til ny dialog.
SBS->>EID: Autentisering/autorisering
EID->>SBS: access_token med systembruker-id eller fnr/dnr
SBS->>API: Hent dialog
API->>AA: Autoriser forespørsel
AA->>API: Returner autorisasjonssvar
API->>SBS: Returnere dialog med liste over aktuelle handlinger og URI-er
SBS->>TEAPI: Foreta endringer
TEAPI->>AA: Autoriser forespørsel
AA->>TEAPI: Returner autorisasjonssvar
par
    opt Hvis endringer som skal reflekteres i Dialogporten
        TEAPI->>API: Oppdater dialog for å reflektere ny tilstand
        API->>TEAPI: Oppdatering OK
    end
and
    TEAPI->>SBS: Oppdatering OK
end
```

#### Tekstlig beskrivelse av trinn

1.  SBS abonnerer på hendelser knyttet til opprettelse av dialoger av en eller flere typer for en gitt part, og mottar en notifikasjon om at en ny dialog er opprettet. Notifikasjonen inneholder en URI til dialogen i Dialogportens API. Alternativt kan liste med dialoger hentes/søkes opp gjennom standard Dialogporten-API-er.
2.  Avhengig av autorisasjonspolicy tilhørende tjenesteressursen, autoriserer SBS-et seg. Les mer i avsnittet [Autorisasjon](#autorisasjon).
3.  Ved uthenting av dialogen som ble referert av hendelsen, returneres en strukturert modell som langt på vei speiler modellen som tjenestetilbyder opprinnelig sendte inn (typisk har samme identifikator). Listen over handlinger definerer da hva SBS-et kan foreta seg, og hvilke endepunkter som må aksesseres for å utføre handlingene.  Enkelte handlinger kan være synlige/gyldige kun for portal, eller kun for API.  Handlinger kun synlige for API kan også referere JSON schemas el.l. som beskriver datamodellen som forventes på det aktuelle endepunktet. 
4.  SBS-et interagerer med tjenestetilbyders API gjennom endepunktene som dialogen beskriver, og/eller i tråd med swagger/annen dokumentasjon som tjenestetilbyder har tilgjengeliggjort f.eks. via API-katalogen. Tjenestetilbyder foretar autorisasjonsoppslag mot Altinn Autorisasjon. Etter hvert som dialogen skrider frem, kan tjenestetilbyder gjøre asynkrone bakkanal-kall til Dialogporten for å oppdatere dialogen slik den fremstår for brukeren både i portal og API.

## Sluttbruker-initiert dialog

### Gjennom GUI (portal) 

#### Sekvensdiagram

```mermaid!
sequenceDiagram
    actor SB as Sluttbruker
    participant API as Dialogporten API 
    participant AUTH as Altinn Autorisasjon API 
    participant TEGUI as tjenestetilbyders GUI
note over SB,TEGUI: Bruker oppdager tjeneste gjennom tjenestekatalog, søkemotor, etatens nettside el.l.
SB->>TEGUI: Bruker starter tjeneste
note over SB,TEGUI: Her kan potensielt Ansattporten benyttes i stedet for at tjenestetilbyder implementerer egen aktørvelger
TEGUI->>AUTH: Hente brukers aktørliste filtrert på tjenesteressurs
AUTH->>TEGUI: Returnere aktørliste
TEGUI->>SB: Presenter aktørliste
SB->>TEGUI: Aktør velges
TEGUI->>TEGUI: Opprette dialog
par
    note over API,TEGUI: Opprettelse av dialogen gjøres når tjenestetilbyder finner dette hensiktsmessig
    opt
        TEGUI->>API: Bakkanal kall for å opprette dialog med egengenerert identifikatpr
        API->>TEGUI: Returnere dialog
    end
and
    TEGUI->>SB: Presenter tjeneste for bruker
end

SB->>TEGUI: Foreta endringer
par
    opt
        TEGUI->>API: Oppdater dialog for å reflektere ny tilstand
        API->>TEGUI: Oppdatering OK
    end
and 
    TEGUI->>SB: Presenter oppdatert tilstand
end
```

#### Tekstlig beskrivelse av trinn

1.  Bruker oppdager tjeneste gjennom tjenestekatalog, søkemotor, etatenes nettsider el.l.
2.  Bruker starter tjenesten og blir umiddelbart tatt inn i brukerflaten hos tjenestetilbyderen, og velger aktør avhengig av tjenestens natur (via autorisasjonskall mot Altinn Autorisasjon for å bygge aktørliste)
3.  Tjenestetilbyder gjør bakkanal-kall for å opprette dialog i Dialogporten
4.  Sluttbruker interagerer med tjenesten, og tjenestetilbyder gjør kall til Dialogporten for å oppdatere dialogen.
5.  Hvis bruker avslutter økten før dialogen er ferdigstilt, kan han/hun (eller andre autoriserte) fortsette å jobbe med dialogen gjennom å aksessere dialogen i Felles Arbeidsflate. Prosessen blir da som "tjenestetilbyder-initiert dialog / Konsum gjennom GUI (portal)" steg 2. 
  
### Gjennom API

*  Her er det to nærliggende alternativer - skal tjenester kunne "instansieres"
    a) gjennom et felles "instansierings"-API i Dialogporten som gjør bakkanal-kall til tjenestetilbyder og returnerer en dialog med liste over handlinger/endepunkter?
    b)  direkte mot tjenestetilbyders API-er som da kan gjøre bakkanal-kall til Dialogporten for å opprette dialogen?

*  Begge deler bør kanskje kunne støttes? Førstnevnte gir en mer homogen løsning sett fra SBS-ene sin side; selv om all kommunikasjon går direkte mot tjenestetilbyder etter instansiering, er dialogen i Dialogporten den som vil kunne reflektere gjeldende tilstand/status og aktuelle handlinger. Det andre løsningen gir en løsere kobling til Dialogporten, men gjør at SBS-et i mindre grad kan forholde seg til en felles brukerflate.
*  I alle tilfeller skal Dialogporten støtte at tjenesteeier oppgir identifikator for dialogen.

#### Variant med instansierings-API 

```mermaid!
sequenceDiagram
    participant SBS as Sluttbrukersystem
    participant EID as Maskinporten/ID-porten/Altinn Token Exchange
    participant API as Dialogporten API
    participant AA as Altinn Autorisasjon
    participant TEAPI as Tjenestetilbyders API
SBS->>EID: Autentisering/autorisering
EID->>SBS: access_token
SBS->>API: Opprett dialog
activate SBS
activate API
API->>AA: Autoriser forespørsel
AA->>API: Returner autorisasjonssvar
API->>TEAPI: Bakkanal kall til tjenestetilbyder (URI fra tjenesteressurs) for å opprette dialog
activate TEAPI
TEAPI->>TEAPI: Opprette dialog
note over API,TEAPI: Tjenesteeier kan oppgi identifikator (UUIDv4 eller tilsvarende)<br>for å ha samme identifikator på tvers av systemene
TEAPI->>API: Opprette dialog med selvgenerert dialog-ID
activate TEAPI
activate API
API->>TEAPI: Opprettelse OK, returner dialog-ID
deactivate TEAPI
deactivate API
TEAPI->>API: Returner dialog-ID
deactivate TEAPI
API->>SBS: Returnere dialog med liste over aktuelle handlinger med URI-er
deactivate API
deactivate SBS
SBS->>TEAPI: Foreta oppslag/endringer
TEAPI->>AA: Autoriser forespørsel
AA->>TEAPI: Returner autorisasjonssvar
opt
    TEAPI->>API: Oppdater dialog for å reflektere ny tilstand
    API->>TEAPI: Oppdatering OK
end
TEAPI->>SBS: Return av oppslag/oppdatering OK

```

1.  ("Design-time") SBS oppdager API gjennom API-katalog eller annen dokumentasjon, og foretar merkantil og teknisk onboarding (setter opp Maskinporten-klient med rette scopes, oppretter systembruker etc)
2.  SBS autentiserer seg (tilsvarende "tjenestetilbyder-initiert dialog / Konsum gjennom API", trinn 2.
3.  SBS gjør et kall til et standard API i Dialogporten ("createinstance" el.l) som oppgir aktør og tjenesteressurs
4.  Dialogporten foretar autorisasjon basert på policy knyttet til tjenesteressurs, og hvis godkjent gjør et bakkanal kall til et instansierings-endepunkt som er definert på tjenesteressurs. Kallet inneholder en standard modell som beskriver autentisert part, valgt aktør, og tjenesteressurs.
5.  Tjenestetilbyder oppretter instans (el.l) i egne systemer, og gjør kall tilbake til Dialogporten i ny kanal for å opprette dialog som beskrevet i "tjenestetilbyder-initert dialog / opprettelse av dialog", trinn 2.
6.  Tjenestetilbyder mottar fra Dialogporten bekreftelse på at dialogen er opprettet
7.  Tjenestetilbyder returnerer identifikator til dialog til Dialogporten i tråd som startet i trinn 3
8.  Dialogporten laster den nyopprettede dialogen, og returner dette til SBS
9.  SBS interagerer med tjenestetilbyders API-er som beskrevet i "tjenestetilbyder-initiert dialog / Konsum gjennom API", trinn 4

#### Variant uten instansierings-API 

```mermaid!
sequenceDiagram
    participant SBS as Sluttbrukersystem
    participant EID as Maskinporten/ID-porten/Altinn Token Exchange
    participant API as Dialogporten API
    participant AA as Altinn Autorisasjon
    participant TEAPI as Tjenestetilbyders API
SBS->>EID: Autentisering/autorisering
EID->>SBS: access_token
note over SBS,TEAPI: Mange tjenester vil ikke ha behov for noe eget initierings-trinn, men kan fullføre en innsending i ett kall.<br>Andre tjenester vil ha behov for dette, kanskje fordi en innsending skal prefilles med data av tilbyder.<br>I dette eksemplet er det et eget instansieringstrinn, og dialogen opprettes ikke før etter at SBS-et har foretatt en innsending.
SBS->>TEAPI: Initier dialogtjeneste
TEAPI->>AA: Autoriser forespørsel
AA->>TEAPI: Returner autorisasjonsbeslutning
TEAPI->>TEAPI: Opprette dialog
TEAPI->>SBS: Returner OK + referanse til dialog / prefill-data
opt
    SBS->>TEAPI: Foreta oppslag for å hente forhåndsutfylt data
    TEAPI->>SBS: Returner forhåndsutfylt data
end
SBS->>TEAPI: Foreta innsending
TEAPI->>TEAPI: Validere innsending
par
    TEAPI->>SBS: Returner OK, kvittering etc
and
    note over API,TEAPI: Opprett dialog i Dialpgportgen med selvgenerert identifikator, som typisk er den samme som egen dialog
    TEAPI->>API: Opprette dialog med selvgenerert dialog-ID
    par
        API->>TEAPI: Opprettelse OK, returner dialog-ID
    and
        API->>SBS: Send notifikasjon om hendelse som genereres ved opprettelse av dialog    
    end
end

note over SBS,TEAPI: Eventuelt videre arbeid/oppfølging kan (men må ikke) gjøres gjennom DP

opt
    SBS->>API: Hent liste over aktuelle handlinger
    API->>SBS: Returner liste over aktuelle handlinger

    SBS->>TEAPI: Foreta oppslag/endringer
    TEAPI->>AA: Autoriser forespørsel
    AA->>TEAPI: Returner autorisasjonsbeslutning
    TEAPI->>API: Oppdater dialog for å reflektere ny tilstand
    API->>TEAPI: Oppdatering OK

    TEAPI->>SBS: Return av oppslag/oppdatering OK
end

```

1.  SBS autentiserer/autoriserer seg mot Maskinporten/ID-porten og får ut access tokens med nødvendige scopes
2.  SBS gjør et eller annet kall for å initiere (og potensielt samtidig fullføre) en dialogtjeneste hos tjenestetilbyder
3.  Tjenestetilbyder foretar et oppslag mot Altinn Autorisasjon for å sjekke om forespørselen (fnr/systembruker) er autorisert for den aktuelle tjenesteressursen
4.  Tjenestetilbyder oppretter dialogen i egne systemer, og returnerer en referanse til SBS-et
5.  SBS-et kan ved behov gjøre et oppslag for å hente prefill-data, hvis ikke dette er inkludert i responsen på forrige trinn
6.  SBS-et foretar en innsending basert på hva sluttbrukeren oppgir 
7.  Tjenestetilbyderen validerer innsendingen
8.  Tjenestetilbyder returner OK til SBS-et. Parallelt (asynkront) opprettes dialog i Dialogporten, og tjenesteeier oppgir å bruke samme identifikator for dialogen.
9. På et senere tidspunkt kan SBS-er jobbe videre med dialogen gjennom dialogen i Dialogporten, eller fortsette å benytte API-ene til tjenestetilbuder direkte.

# Implementasjon / Proof-of-concept

En foreløpig implementasjon er publisert, se [https://dppoc-webapi.azurewebsites.net/swagger/](https://dppoc-webapi.azurewebsites.net/swagger/). Git-repository er åpent tilgjengelig, og ligger på [Github](https://github.com/digdir/dialogporten). 

# OpenAPI

En OpenAPI 3.0.1 specification (OAS) basert på modellene beskrevet under kan sees på [https://dppoc-webapi.azurewebsites.net/swagger/V0.1/swagger.json](https://dppoc-webapi.azurewebsites.net/swagger/V0.1/swagger.json). 

{% include note.html type="warning" content="OpenAPI-spesifikasjonen har visse tekniske (spesielt mht navngivning) forskjeller med modellene som vises på denne siden, og kan være ute av synk" %}

# Eksempel-modeller

Under er utkast til JSON-modeller slik de kan fremstå i API-ene som må implementeres gitt beskrivelsene over, med kommentarer.

{% for dpj in site.dialogporten_json %}
  {% assign jsonPageName = dpj.url | split: "/" | last | remove: ".html" | remove: ".jsonc" %}
  {% assign jsonPageId = jsonPageName | remove: "." %}
  <h2 id="{{ jsonPageId }}">{{ jsonPageName }}</h2>
  {{ dpj.content | markdownify }}
{% endfor %}

# Eksempel-case

{% for dpc in site.dialogporten_case %}
  {% assign casePageId = dpc.url | split: "/" | last | remove: ".html" %}
  <h2 id="{{ casePageId }}">{{ dpc.menutitle }}</h2>
  <p>{{ dpc.title | escape }}</p>
  <a class="page-link" href="{{ dpc.url | relative_url }}">Gå til case</a>
{% endfor %}

