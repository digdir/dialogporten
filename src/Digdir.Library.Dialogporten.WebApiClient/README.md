# SO SDK

Simple overview

## Installation

Install nuget

## Usage

Setup

```C#
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddDialogportenClient();
services.AddDialogTokenVerifer();
var dialogportenClient = serviceProvider.GetRequiredService<IServiceownerApi>();
```

AppSettings

```JSON
{
    "DialogportenSettings": {
        "Environment": "test",
        "Maskinporten": {
            "ClientId": "ce3b732a-d4f2-4997-8545-adf8df70fe6c",
            "Scope": "digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search",
            "EncodedJwk": "eyJwIjoieTRBZEhlVVBxdFEtSFlOWkR5ci0zS09RT3NQajA5TFh2a2hIUFlTdGFYNThkMndIWUJiVXlDTWdMYWtGTHo4UExKNWtscURsanRoczFtM1dFVGJhSWVuY25TalpjZTh4S1Q2SHh3bTNyaDlydWZ1TWVOZDRqaFptTm9WZmJrcGNXcVh0UDFvb1NPTE5zYUNVUWFUUEVKTXlFd3VhdWxMSzgxRG1SSTlMSmVNIiwia3R5IjoiUlNBIiwicSI6InFmOEQ2Uy1Kd19BdVQ0Q2hjQTlDek9WNk1uTW9mc1VCdTkteHJBcVFDRjh4WWZZOTRxQ1ZjQ3llajlkTlN3eXZUZXg1dThIMzNSaU1LMEFWM2tTQlpJLVZqcXJHLUx6YzNfTUlTTVpSVDJfbzNVQlRWVHpqTkUtSkpMX1hKaXJ6ZVhhQjM1UmFZMjFnWVhKQWg3X2tuR3dpRzF3MGxiT2ozQ0FzdnVwaU1BMCIsImQiOiJLVkF1b1Zhd2paTTgwenRYcUxSZUJGZkJ3M3pxVjdkUGFpaWFONWU0RFp6bW5MYTFMNEZJMTgtanVraHN4UVdqR1NFQnBIdTFrOHRPUWMyWjBsSDVaaTBydERqM0JKeEhxeDNsUGdYMWdTNXNiX1EyeXNfb2FKcklSX012MHBDQUFHX3hpa2lUY2kzTHMyeV9femV4QTdLbG0yalNmYW9Udzdhbml1R3RId1d5dHhCSnJnZ0J2c3loaHZIaUVQcnZaMHZBZldYYWI3QUtkUjc1cEtEaVVHOGdGNTdJN0hrWnpJSk9QYXp3MTU1Skx4TG9HcDVzeTFCVVpBNHRiQmlseWVsdG9ONGZINWd1aUktOXJjTE5zUmVYazJ1c3NFbE9EbVZ2Qmx2ZVVfb1ZRMVYtVDRJRnUzZk1BYVJGUFA2Wlo1akJJX2hkOFJOTTJ3eUp5UHVRWVEiLCJlIjoiQVFBQiIsInVzZSI6InNpZyIsImtpZCI6ImRpYWxvZ3BvcnRlbi1zcC1zZGstdGVzdC0yMDI0MTAxMCIsInFpIjoiQm9VS0RlczQ0UTNpXzNyT3Q4aHRrS2NxWkFNem00Njl2cTZuQnJVcHBTU1Ric3YwalZwN1daRGRRR0Q0bU8yMVJVOEFUbmN3NjFPOUt3YXktOGloX082VWFWbGxZN3NHYlVrQ2NVaG43ZDkzSElLZnhybnhWVE9nNUNMWTBka2Zwa3A1V2pyU1VvMTVKQURsY3BRM0ItRlU0Nm9PTG9ydjJ0SVFQekE4OF93IiwiZHAiOiJ1emVaRWZpN2Fqa3JFREhYekZtTThXWFUtZ3RmM1ctN0pnY082MnpWc1JrNTN4QlcxTE1NZlRlN2tlWk9xOEhDN3hTbGktSm9idnR6WGU3Y295ZW9sTXkzTnlydXFhQVp4VTBPMHpHQWQ4UFdjdHNXeDlITHlrU1hNby1QVlVNNkpmZERCaWFtcXk5bGQ0WTRfdzlscEdVWEMyaUFwLXdsWktaSHdrbG1KR3MiLCJhbGciOiJSUzI1NiIsImRxIjoiVENBcV9DMlJuX0RhakRlcUU2aUIzWWVWNVNtMHBMQk1Tbm10OHNENEp3ZVo4YWgzcGhrTFVxUm9qVGw1SDNhYXVtWl9UUmxiaWVNSVFnWDh4UUFnZ1l2YkNYeG9oZEx0aGt3ckZZdlp0WjBEeHJDYm9Md1hjc0Y3Ukwyejl4LWMwSFBGVFAzLVREQWF6UWlBNVVtRmNwYnAzeDYzWGFLSWFuYnVFc0NiSDdFIiwibiI6Imh5Sks4WnE2Wk8tRjFSSklVWVNCdUpfeG9RWkNNV1EyTVhrSFQ1bVROVVJJZmVWWWpCNWMwMzI0Uk5nc3ZPMEtXX0hUejRRSnptLV9rU1VaZ0h1Z2JoR0F3a1Vqc1lwTlJJRTBvLVNtdEExMlMxZHVCZWx6ajg2LVFrZkFzeFlwblNnSzl5OXpTS1B0YVlzMS1EcEVIb0hVdk9BSDJlNktFTXRaYUZPM0J0Yk9WUURXMENMYi1FY0UyaDBQRlFMMUp3NU8zeDhHcXBZeUFhamNoWnptcWlFbjBaSEd1QTNZZ1NyNGxQV1lkTzBNWHZmRFdyaFBTcnVTS3FodzBHMTlBRUpHOFhoek9xTWxLTUFIbW5ybk9XOHM2cWR2Sy1UQ1BiVGJJOU5XUWdFd2JpUFBBdlU0MUFITzZmTEYxUHZzQ3FhNjZTSGdYMkJzS3pvNVhORjhodyJ9"
        }
    },
    // Ed25519 keys for validating dialog tokens
    "Ed25519Keys": {
        "Primary": {
            "Kid": "",
            "PublicComponent": ""
        },
        "Secondary": {
            "Kid": "",
            "PublicComponent": ""
        }
    }
} 
```

Basic usage

```C#
// Basic Usage example

// Create a dialog
var createDialogCommand = new V1ServiceOwnerDialogsCommandsCreate_DialogCommand
{
    // createDialogCommand.Id = Guid.Parse("01927a6d-40d8-728b-b3da-845b680840d9");
    ServiceResource = "urn:altinn:resource:super-simple-service",
    Party = "urn:altinn:person:identifier-no:14886498226",
    SystemLabel = DialogEndUserContextsEntities_SystemLabel.Default,
    Status = DialogsEntities_DialogStatus.New,
    Progress = 2,
    Content = new V1ServiceOwnerDialogsCommandsCreate_Content
    {
        Title = new V1CommonContent_ContentValue
        {
            Value =
            [
                new V1CommonLocalizations_Localization
                {
                    LanguageCode = "nb",
                    Value = "Hoved"
                },
                new V1CommonLocalizations_Localization
                {
                    LanguageCode = "en",
                    Value = "Main"
                }
            ],
            MediaType = "text/plain"
        },
        Summary = new V1CommonContent_ContentValue
        {
            Value =
            [
                new V1CommonLocalizations_Localization
                {
                    LanguageCode = "nb",
                    Value = "Hoved Summary"
                },
                new V1CommonLocalizations_Localization
                {
                    LanguageCode = "en",
                    Value = "Main Summary"
                }
            ],
            MediaType = "text/plain"
        }


    },
    Transmissions =
    [
        new V1ServiceOwnerDialogsCommandsCreate_Transmission
        {
            Attachments =
            [
                new V1ServiceOwnerDialogsCommandsCreate_TransmissionAttachment
                {
                    DisplayName =
                    [
                        new V1CommonLocalizations_Localization
                        {
                            LanguageCode = "nb",
                            Value = "Hoved misson"
                        }
                    ],
                    Urls =
                    [
                        new V1ServiceOwnerDialogsCommandsCreate_TransmissionAttachmentUrl
                        {
                            ConsumerType = Attachments_AttachmentUrlConsumerType.Gui,
                            Url = new Uri("https://digdir.apps.tt02.altinn.no/some-other-url")
                        }
                    ]

                }
            ],
            Content = new V1ServiceOwnerDialogsCommandsCreate_TransmissionContent
            {
                Summary = new V1CommonContent_ContentValue
                {
                    MediaType = "text/plain",
                    Value =
                    [
                        new V1CommonLocalizations_Localization
                        {
                            LanguageCode = "nb",
                            Value = "Tranmission summary"
                        }
                    ]
                },
                Title = new V1CommonContent_ContentValue
                {
                    MediaType = "text/plain",
                    Value =
                    [
                        new V1CommonLocalizations_Localization
                        {
                            LanguageCode = "nb",
                            Value = "Tranmission Title"
                        }
                    ]
                }
            },
            Sender = new V1ServiceOwnerCommonActors_Actor
            {
                ActorType = Actors_ActorType.ServiceOwner
            },
            Type = DialogsEntitiesTransmissions_DialogTransmissionType.Information
        }
    ]
};
var response = await dialogportenClient.DialogsPost(createDialogCommand);
if (response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
    string guid = response.Content; 
}
```

```C#
// Patch Dialog
List<JsonPatchOperations_Operation> patchDocument =
[
    new()
    {
        Op = "replace",
        OperationType = JsonPatchOperations_OperationType.Replace,
        Path = "/progress",
        Value = 50
    }
];
var patchResponse = await dialogportenClient.V1ServiceOwnerDialogsPatchDialog(guid, patchDocument, null);
Console.WriteLine(patchResponse.IsSuccessStatusCode);
Console.WriteLine(patchResponse.StatusCode);
```

API REFERENCE/STRUCTURE


Links to changelog etc.
