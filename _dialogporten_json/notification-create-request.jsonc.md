---
---
```jsonc
// Input modell som tjenesteeiere oppgir for å opprette en ny varsling knytte til en dialog.
// KRR / KoFuVI / varslingsadresser oppgitt i Altinn legges til grunn.
// Man oppgir minst én av sms, email og push. Ytterligere varslinger kan sendes når som helst.
// Kun konsumenter som er autorisert for den aktuelle dialogen vil motta varsel.

// POST /dialogporten/api/v1/dialogs/{dialogId}/notifications
{
    // Valgfri identifikator som tjenestetilbyder kan oppgi. Kan brukes for å unngå duplikatutsending.
    "notificationId": "79921fae-631a-4f8b-8db5-e359f2336658",

    // XACML-ressurs i poliy knyttet til dialog som det tilgang til (vilkårlig action) for å kunne motta varsel 
    "subresource": "urn:altinn:subresource:some-xaml-resouce",
    "sms": {
            "text": [ { "code": "nb_NO", "value": "dette kommer på sms" } ],
            
            // Hvis avsender-felt skal være noe annet enn navn på tjenesteeier kan dette oppgis her. 
            // Valideres opp mot whitelist knyttet til autentisert org.
            "from": [ { "code": "nb_NO", "value": "Etaten" } ]
    },
    "email": {
            "subject": [ { "code": "nb_NO", "value": "emnefeltet på e-post" } ],
            //
            "template": "some-email-template",
            "tokens": [
                { "name": "header", "value": [ { "code": "nb_NO", "value": "dette er en header" } ] },
                { "name": "body", "value": [ { "code": "nb_NO", "value": "Hei {{recipient.fullname}}!" } ] } 
            ]
    },
    "push": {
        // Basert på https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration/showNotification
        "title": [ { "code": "nb_NO", "value": "Tittel på notifikasjon" } ],
        "body": [ { "code": "nb_NO", 
            "value": "Dette er første linje\nDette er andre linje, sendt til {{recipient.fullname}}" } ],
        "icon": "https://example.com/some-icon-atleast-192x192.png",
        // Valgfri URL som bruker blir sendt til hvis notifikasjonen klikkes på. 
        "notificationClickUrl": "https://example.com/some/deep/link/to/dialogues/123456789"
    }
}
```