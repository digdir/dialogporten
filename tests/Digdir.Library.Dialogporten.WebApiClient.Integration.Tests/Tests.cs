using System.Net;
using Digdir.Library.Dialogporten.WebApiClient.Extensions;
using Digdir.Library.Dialogporten.WebApiClient.Features.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digdir.Library.Dialogporten.WebApiClient.Integration.Tests;

public class WebApiClientFixture : IDisposable
{
    public IServiceownerApi DialogportenClient { get; }
    public WebApiClientFixture()
    {

        var configuration = new ConfigurationBuilder().AddUserSecrets<Tests>().Build();
        // .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true)
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDialogportenClient();
        services.AddDialogTokenVerifer();
        DialogportenClient = services.BuildServiceProvider().GetRequiredService<IServiceownerApi>();

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class Tests(WebApiClientFixture fixture) : IClassFixture<WebApiClientFixture>, IDisposable
{
    private readonly List<Guid> _dialogIds = [];
    [Fact]
    public async Task PurgeTest()
    {

        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        var purgeResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, null);
        Assert.True(purgeResponse.IsSuccessStatusCode);

    }

    [Fact]
    public async Task PatchTest()
    {
        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.True(createResponse.IsSuccessStatusCode, createResponse.Error?.Content);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        _dialogIds.Add(dialogId);

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
        var patchResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsPatchDialog(dialogId, patchDocument, null, CancellationToken.None);
        Assert.True(patchResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GetTest()
    {
        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(getResponse.Content!.Progress!, createDialogCommand.Progress);
        await fixture.DialogportenClient.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, null!);

    }

    [Fact]
    public async Task UpdateTest()
    {

        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        _dialogIds.Add(dialogId);

        var updateDialogCommand = UpdateCommand();
        var updateResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsUpdateDialog(dialogId, updateDialogCommand, null!, CancellationToken.None);
        Assert.True(updateResponse.IsSuccessStatusCode, updateResponse.Error?.Content);
        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!);
        Assert.True(getResponse.IsSuccessStatusCode);
        Assert.Equal(updateDialogCommand.Progress, getResponse.Content!.Progress);
    }
    [Fact]
    public async Task SearchTest()
    {
        /*
         * Amund Q:.
         *  [x] Refit støtter date format.
         *  [x] Refitter trenger at swagger sier "date" på format.
         *  [x] Swagger sier nå "date-time"
         *  [x] Refit støtter custom date format
         *  [x] Refitter søtter ikke custom date format
         *  [x] Få Swagger gen til å generete "date" istedet for "date-time"?
         *      [x] lag til date-time støtte i refitter
         *  [x] Legge til støtte for custom date format i Refitter
         *      [x] Virker doable, Relativt lett leslig kilde kode.
         *  [x] Lagde PR med forandringene, blitgt Merget inn
         *  [ ] Vente på preview av refitter blir lansert
         */

        /* Amund: .
         *  500 Error om jeg sender en null istedet for et tomt array.
         *  Funker også i postman med å skrive null
         */
        var dateOffset = DateTimeOffset.UtcNow;
        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);


        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        _dialogIds.Add(dialogId);
        var param = new V1ServiceOwnerDialogsSearchSearchDialogQueryParams
        {
            CreatedAfter = dateOffset
        };
        var searchResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsSearchSearchDialog(param, CancellationToken.None);
        Assert.True(searchResponse.IsSuccessStatusCode);
        Assert.NotNull(searchResponse.Content);
        Assert.Single(searchResponse.Content!.Items);

    }

    [Fact]
    public async Task DeleteTest()
    {
        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        _dialogIds.Add(dialogId);

        var deleteResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsDeleteDialog(dialogId, null);

        Assert.True(deleteResponse.IsSuccessStatusCode);

        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!);
        Assert.True(getResponse.IsSuccessStatusCode);
        Assert.NotNull(getResponse.Content!.DeletedAt);

    }

    public static V1ServiceOwnerDialogsCommandsUpdate_Dialog UpdateCommand()
    {
        var createDialogCommand = new V1ServiceOwnerDialogsCommandsUpdate_Dialog
        {
            Status = DialogsEntities_DialogStatus.New,
            Progress = 60,
            SearchTags = [],
            Attachments = [],
            GuiActions = [],
            ApiActions = [],
            Content = new V1ServiceOwnerDialogsCommandsUpdate_Content
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
                new V1ServiceOwnerDialogsCommandsUpdate_Transmission
                {
                    Attachments =
                    [
                        new V1ServiceOwnerDialogsCommandsUpdate_TransmissionAttachment
                        {
                            DisplayName =
                            [
                                new V1CommonLocalizations_Localization
                                {
                                    LanguageCode = "nb",
                                    Value = "Hoved mission"
                                }
                            ],
                            Urls =
                            [
                                new V1ServiceOwnerDialogsCommandsUpdate_TransmissionAttachmentUrl
                                {
                                    ConsumerType = Attachments_AttachmentUrlConsumerType.Gui,
                                    Url = new Uri("https://digdir.apps.tt02.altinn.no/some-other-url")
                                }
                            ]

                        }
                    ],
                    Content = new V1ServiceOwnerDialogsCommandsUpdate_TransmissionContent
                    {
                        Summary = new V1CommonContent_ContentValue
                        {
                            MediaType = "text/plain",
                            Value =
                            [
                                new V1CommonLocalizations_Localization
                                {
                                    LanguageCode = "nb",
                                    Value = "Transmission summary"
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
                                    Value = "Transmission Title"
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
            ],
            VisibleFrom = null
        };
        return createDialogCommand;
    }
    public static V1ServiceOwnerDialogsCommandsCreate_DialogCommand CreateCommand()
    {
        var now = DateTimeOffset.UtcNow;
        var createDialogCommand = new V1ServiceOwnerDialogsCommandsCreate_DialogCommand
        {
            Activities =
            [
                new V1ServiceOwnerDialogsCommandsCreate_Activity
                {
                    CreatedAt = now,
                    Description =
                    [
                        new V1CommonLocalizations_Localization
                        {
                            LanguageCode = "nb",
                            Value = "Dette er en beskrivelse"
                        }
                    ],
                    PerformedBy = new V1ServiceOwnerCommonActors_Actor
                    {
                        ActorType = Actors_ActorType.ServiceOwner
                    },
                    Type = DialogsEntitiesActivities_DialogActivityType.Information
                }
            ],
            ApiActions =
            [
                new V1ServiceOwnerDialogsCommandsCreate_ApiAction
                {
                    Action = "submit",
                    Endpoints =
                    [
                        new V1ServiceOwnerDialogsCommandsCreate_ApiActionEndpoint
                        {
                            HttpMethod = Http_HttpVerb.POST,
                            RequestSchema = new Uri("https://digdir.apps.tt02.altinn.no/digdir/super-simple-service/api/jsonschema/mainform-20231015"),
                            ResponseSchema = new Uri("https://docs.altinn.studio/swagger/altinn-app-v1.json#/components/schemas/DataElement"),
                            Url = new Uri("https://digdir.apps.tt02.altinn.no/digdir/super-simple-service/#/instance/50756302/58d88b01-8840-8771-a6dd-e51e9809df2c/data?dataType=mainform-20231015"),
                            Version = "20231015"
                        }
                    ]
                }
            ],
            // createDialogCommand.Id = Guid.Parse("01927a6d-40d8-728b-b3da-845b680840d9");
            ServiceResource = "urn:altinn:resource:super-simple-service",
            Party = "urn:altinn:person:identifier-no:14886498226",
            SystemLabel = DialogEndUserContextsEntities_SystemLabel.Default,
            Status = DialogsEntities_DialogStatus.New,
            Progress = 2,
            SearchTags =
            [
                new V1ServiceOwnerDialogsCommandsCreate_SearchTag
                {
                    Value = "Search tag"
                }
            ],
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
                                    Value = "Hoved mission"
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
                                    Value = "Transmission summary"
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
                                    Value = "Transmission Title"
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
            ],
            UpdatedAt = default,
            VisibleFrom = null
        };
        return createDialogCommand;
    }

    public async void Dispose()
    {
        foreach (var dialogId in _dialogIds)
        {
            var purgeResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, null!);
            Assert.True(purgeResponse.IsSuccessStatusCode);
        }
        GC.SuppressFinalize(this);
    }
}
