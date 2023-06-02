---
---
```jsonc
// Dette er modellen som SBS sender inn til et felles endepunkt hos Altinn for å instansiere en navngitt tjeneste.
// Altinn gjør en enkel proxy av dette kallet til instansierings-endepunktet som tjenesteeier har registrert på den 
// aktuelle tjenesten. Altinn proxyer også svaret, som skal være enten en 201 Created og en DE-response-modell, eller
// en 4xx/5xx med en RFC7807-kompatibel feilmelding.
{
    "serviceResource": "urn:altinn:resource:super-simple-service",
    "party": "12345678901",
    // Hvis det ifm instansiering kreves noen parametre kan disse oppgis her. Disse sendes verbatim til instatiationUrl 
    // (kun gjenstand for størrelsesbegrensninger). All logikk og øvrig validering foretas av tjenesteeier.
    "inputParameters": { 
        "someKey": "someValue"
    }
}
```