
import { uuidv7 } from "../../common/uuid.js";

export default function (relatedTransmissionId) {
    let transmission = {
        "id": uuidv7(),
        "createdAt": new Date().toISOString(),
        "authorizationAttribute": "element1",
        "extendedType": "string",
        "type": "Information",
        "sender": {
            "actorType": "serviceOwner"
        },
        "content": {
            "title": {
                "value": [
                    {
                        "value": "Forsendelsestittel",
                        "languageCode": "nb"
                    },
                    {
                        "languageCode": "en",
                        "value": "Transmission title"
                    }
                ],
            },
            "summary": {
                "value": [
                    {
                        "languageCode": "nb",
                        "value": "Forsendelse oppsummering"
                    },
                    {
                        "languageCode": "en",
                        "value": "Transmission summary"
                    }
                ],
            }
        },
        "attachments": [
            {
                "displayName": [
                    {
                        "languageCode": "nb",
                        "value": "Forsendelse visningsnavn"
                    },
                    {
                        "languageCode": "en",
                        "value": "Transmission attachment display name"
                    }
                ],
                "urls": [
                    {
                        "url": "https://digdir.apps.tt02.altinn.no/some-other-url",
                        "consumerType": "Gui"
                    }
                ]
            }
        ]
    }
    if (relatedTransmissionId != 0) {
        transmission.relatedTransmissionId = relatedTransmissionId;
    }
    return transmission;
}