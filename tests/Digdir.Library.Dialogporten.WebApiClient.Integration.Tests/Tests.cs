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

        Assert.True(createResponse.IsSuccessStatusCode);
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
        var updateResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsPatchDialog(dialogId, patchDocument, null, CancellationToken.None);
        Assert.True(updateResponse.IsSuccessStatusCode);
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
    public async Task SearchTest()
    {
        // Amund: DateTimeOffset blir sendt over i feil format. Virker som den blir sendt som string og parset fra string igjen.
        // Usikker på hvordan jeg skal sende i annet format?
        // Det er nok en implisitt toString som skjer. hvordan skal jeg da gi den format stringen?
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
         *      [ ] Lagde PR inn til refitter repo med forandringene
         */
        var dateOffset = DateTimeOffset.UtcNow;
        // Thread.CurrentThread.CurrentCulture.Calendar = new GregorianCalendar(GregorianCalendarTypes.TransliteratedEnglish);
        var createDialogCommand = CreateCommand();
        var createResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsCreateDialog(createDialogCommand);


        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.NotNull(createResponse.Content);
        Assert.True(Guid.TryParse(createResponse.Content!.Replace("\"", "").Trim(), out var dialogId));

        var param = new V1ServiceOwnerDialogsSearchSearchDialogQueryParams
        {
            CreatedAfter = dateOffset
        };
        var searchResponse = await fixture.DialogportenClient.V1ServiceOwnerDialogsSearchSearchDialog(param, CancellationToken.None);
        await fixture.DialogportenClient.V1ServiceOwnerDialogsPurgePurgeDialog(dialogId, null!, CancellationToken.None);
        Assert.True(searchResponse.IsSuccessStatusCode);
        Assert.NotNull(searchResponse.Content);
        Assert.Single(searchResponse.Content!.Items);

    }

    public static V1ServiceOwnerDialogsCommandsCreate_DialogCommand CreateCommand()
    {
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
            ]
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
