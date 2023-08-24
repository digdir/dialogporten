---
---
```jsonc
// Logg-modell som brukes for å hente liste over sendte varslinger

// GET /dialogporten/api/v1/enduser/dialogs/{dialogId}/notifications
{
    "sms": [
        { 
            "sentDateTime": "2022-12-01T10:00:00.000Z",
            "status": "pending",
            "notification": {
                "text": "dette kommer på sms",
                "from": "Etaten" 
            }
        }
    ],
    "email": [
        {
            "sentDateTime": "2022-12-01T10:00:00.000Z",
            "status": "ok",
            "notification": {
                "subject": "emnefeltet på e-post" ,
                "template": "some-email-template",
                "tokens": [
                    { "name": "header", "value": "dette er en header" },
                    { "name": "body", "value": "Hei {{party.fullname}}!" } 
                ]
            }
        }
    ],
    "push": [
        {
            "sentDateTime": "2022-12-01T10:00:00.000Z",
            "status": "ok",
            "notification": {            
                "title": "Notification title",
                "body": "Simple piece of body text.\nSecond line of body text :)",
                "icon": "https://example.com/some-icon-atleast-192x192.png",
                "notificationClickUrl": "https://example.com/some/deep/link/to/dialogues/123456789"
            }
        }    
    ]
}
```