---
---
```jsonc
// Input modell her vil være en ServiceResource som beskrevet på 
// https://docs.altinn.studio/authorization/modules/resourceregistry/
// Modellen må utvides med en "instantiationUrl", som er lagt til under.

// POST /resourceregistry/api/v1/resource
{
    "identifier": "super-simple-service",
    "title": [
        {
            "title": "Tittel på tjenesten",
            "language": "nb-NO"
        }
    ],
    "description": [
        {
            "language": "nb-NO",
            "description": "En beskrivelse av tjenesten"
        }
    ],
    "hasCompetentAuthority": {
        "organization": "991825827",
        "orgcode": "digdir"
    },
    "contactpoint": [
        {
            "phone": "1231324",
            "email": "super-simple-service@example.com"
        }
    ],
    "isPartOf": "someportal",
    "homepage": "https://example.com/about_super-simple-service",
    "status": "Completed",
    "thematicArea": [],
    "type": [],
    "sector": [],
    "keyword": [
        {
            "keyword": "delegation"
        },
        {
            "keyword": "access management"
        }
    ],
    "instantiationUrl": "https://example.com/api/v1/super-simple-service/instantiate"
}

// Velykket innsending returnerer en 201 Created med Location-lenke til den aktuelle tjenesteressursen. Policy settes så på 
// tjenesteressursen med et kall som inneholder en XACML-policy til: 
// POST /resourceregistry/api/v1/resource/super-simple-service/policy
```