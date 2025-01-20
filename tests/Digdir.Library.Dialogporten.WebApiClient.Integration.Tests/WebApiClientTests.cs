using System.Net;
using Digdir.Library.Dialogporten.WebApiClient.Config;
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

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<WebApiClientTests>()
            .AddEnvironmentVariables()
            .Build();
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

public class WebApiClientTests(WebApiClientFixture fixture) : IClassFixture<WebApiClientFixture>, IDisposable
{
    private readonly List<Guid> _dialogIds = [];
    [Fact]
    public async Task Create_Invalid_Dialog_Returns_400()
    {
        var createDialogCommand = CreateCommand();
        createDialogCommand.Progress = 200;
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }


    [Fact]
    public async Task Purge_Dialog_Returns_204()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
        var purgeResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, null);
        Assert.Equal(HttpStatusCode.NoContent, purgeResponse.StatusCode);

        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!, CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    private async Task<Guid> CreateDialog(V1ServiceOwnerDialogsCommandsCreate_DialogCommand createDialogCommand)
    {

        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));
        return dialogId;
    }

    [Fact]
    public async Task Patch_Invalid_Dialog_Returns_400()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
        _dialogIds.Add(dialogId);

        List<JsonPatchOperations_Operation> patchDocument =
        [
            new()
            {
                Op = "replace",
                OperationType = JsonPatchOperations_OperationType.Replace,
                Path = "/progress",
                Value = 500
            }
        ];
        var patchResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsPatchDialog(dialogId, patchDocument, null, CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);
    }
    [Fact]
    public async Task Patch_Dialog_Returns_204()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
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
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(50, getResponse.Content!.Progress);
    }

    [Fact]
    public async Task Get_Dialog_Returns_200()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);

        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        _dialogIds.Add(dialogId);
    }
    [Fact]
    public async Task Update_Invalid_Dialog_Returns_400()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
        _dialogIds.Add(dialogId);

        var updateDialogCommand = UpdateCommand();
        updateDialogCommand.Progress = 200;
        var updateResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsUpdateDialog(dialogId, updateDialogCommand, null!, CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }
    [Fact]
    public async Task Update_Dialog_Returns_204()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
        _dialogIds.Add(dialogId);

        var updateDialogCommand = UpdateCommand();
        var updateResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsUpdateDialog(dialogId, updateDialogCommand, null!, CancellationToken.None);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(updateDialogCommand.Progress, getResponse.Content!.Progress);
    }
    [Fact]
    public async Task Search_Dialog_Returns_200()
    {

        var dateOffset = DateTimeOffset.UtcNow;
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
        _dialogIds.Add(dialogId);
        var param = new V1ServiceOwnerDialogsSearchSearchDialogQueryParams
        {
            CreatedAfter = dateOffset
        };
        var searchResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsSearchSearchDialog(param, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        Assert.NotNull(searchResponse.Content);
        Assert.Single(searchResponse.Content!.Items);

    }

    [Fact]
    public async Task Search_Multiple_Dialogs()
    {
        var dateOffset = DateTime.UtcNow;
        var dialogsCreated = 5;
        for (var i = 0; i < dialogsCreated; i++)
        {
            var createDialogCommand = CreateCommand();
            var dialogId = await CreateDialog(createDialogCommand);
            _dialogIds.Add(dialogId);
        }
        var param = new V1ServiceOwnerDialogsSearchSearchDialogQueryParams
        {
            CreatedAfter = dateOffset
        };
        var searchResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsSearchSearchDialog(param, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        Assert.NotNull(searchResponse.Content);
        Assert.Equal(dialogsCreated, searchResponse.Content!.Items.Count);
    }

    [Fact]
    public async Task Delete_Dialog_Returns_204()
    {
        var createDialogCommand = CreateCommand();
        var dialogId = await CreateDialog(createDialogCommand);
        _dialogIds.Add(dialogId);

        var deleteResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsDeleteDialog(dialogId, null);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsGetGetDialog(dialogId, null!);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getResponse.Content!.DeletedAt);

    }

    [Fact]
    public async Task Create_Dialog_Transmission_returns_201()
    {

        var createDialogCommand = CreateCommand();
        createDialogCommand.Transmissions = [];
        var dialogId = await CreateDialog(createDialogCommand);
        _dialogIds.Add(dialogId);
        var createTransmission = new V1ServiceOwnerDialogTransmissionsCreate_TransmissionRequest
        {
            Attachments = [],
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
                            Value = "Sammendrag"
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
                            Value = "Dette er en tittel"
                        }
                    ]
                }
            },
            Sender = new V1ServiceOwnerCommonActors_Actor
            {
                ActorType = Actors_ActorType.ServiceOwner
            },
            Type = DialogsEntitiesTransmissions_DialogTransmissionType.Information
        };
        var transmissionResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogTransmissionsCreateDialogTransmission(dialogId, createTransmission, null!);
        Assert.Equal(HttpStatusCode.Created, transmissionResponse.StatusCode);

    }
    private static V1ServiceOwnerDialogsCommandsUpdate_Dialog UpdateCommand()
    {
        var createDialogCommand = new V1ServiceOwnerDialogsCommandsUpdate_Dialog
        {
            Activities = [],
            Status = DialogsEntities_DialogStatus.New,
            Progress = 60,
            Attachments = [],
            GuiActions = [],
            ApiActions = [],
            SearchTags = [],
            Content = new V1ServiceOwnerDialogsCommandsUpdate_Content
            {
                Title = new V1CommonContent_ContentValue
                {
                    MediaType = "text/plain",
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
            VisibleFrom = null,
        };
        return createDialogCommand;
    }
    private static V1ServiceOwnerDialogsCommandsCreate_DialogCommand CreateCommand()
    {
        var now = DateTimeOffset.UtcNow;
        var createDialogCommand = new V1ServiceOwnerDialogsCommandsCreate_DialogCommand
        {
            GuiActions = [],
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
            Attachments = [],
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
