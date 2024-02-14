﻿using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Http;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs.Queries;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class GetDialogTests : ApplicationCollectionFixture
{
    public GetDialogTests(DialogApplication application) : base(application)
    {
    }

    [Fact(Skip = "Must fix for validation rules.")]
    public async Task Get_ReturnsSimpleDialog_WhenDialogExists()
    {
        // Arrange
        var createDialogCommand = new CreateDialogCommand
        {
            Id = Guid.NewGuid(),
            ServiceResource = new("urn:altinn:resource:example_dialog_service"),
            Party = "org:991825827",
            Status = DialogStatus.Values.InProgress
        };

        var createCommandResponse = await Application.Send(createDialogCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out var _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createDialogCommand);
    }

    [Fact(Skip = "Must fix for validation rules.")]
    public async Task Get_ReturnsDialog_WhenDialogExists()
    {
        // Arrange
        var dialogElementGuid = Guid.NewGuid();
        var createCommand = new CreateDialogCommand
        {
            Id = Guid.NewGuid(),
            ServiceResource = new("urn:altinn:resource:example_dialog_service"),
            Party = "org:991825827",
            Status = DialogStatus.Values.InProgress,
            ExtendedStatus = "SKE-ABC",
            //DueAt = new(new DateTime(2022, 12, 01), TimeSpan.Zero),
            //ExpiresAt = new(new DateTime(2022, 12, 01), TimeSpan.Zero),
            // SearchTags = new() { new() { CultureCode = "nb_NO", Value = "Et eksempel på en tittel" } },
            Content =
            {
                new() { Type = DialogContentType.Values.Title, Value = { new() { CultureCode = "nb_NO", Value = "Et eksempel på en tittel" } } },
                new() { Type = DialogContentType.Values.SenderName, Value = { new() { CultureCode = "nb_NO", Value = "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } } },
                new() { Type = DialogContentType.Values.AdditionalInfo, Value = { new() { CultureCode = "nb_NO", Value = "Innhold med <em>begrenset</em> HTML-støtte. Dette innholdet vises når dialogen ekspanderes." } } }
            },
            Elements =
            [
                new()
                {
                    Id = dialogElementGuid,
                    DisplayName = [new() {CultureCode = "nb_NO", Value = "Dette er et vedlegg"}],
                    Type = new Uri("some:type"),
                    AuthorizationAttribute = "attachment1",
                    Urls =
                    [
                        new()
                        {
                            ConsumerType = DialogElementUrlConsumerType.Values.Gui,
                            MimeType = "application/pdf",
                            Url = new Uri("http://example.com/some/deep/link/to/attachment1.pdf")
                        }
                    ]
                }

            ],
            GuiActions =
            [
                new()
                {
                    Action = "open",
                    Priority = DialogGuiActionPriority.Values.Primary,
                    Title = [new() {CultureCode = "nb_NO", Value = "Åpne i dialogtjeneste"}],
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789")
                },

                new()
                {
                    Action = "confirm",
                    Priority = DialogGuiActionPriority.Values.Secondary,
                    Title = [new() {CultureCode = "nb_NO", Value = "Bekreft mottatt"}],
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    AuthorizationAttribute = "somesubresource",
                    IsBackChannel = true
                },

                new()
                {
                    Action = "delete",
                    Priority = DialogGuiActionPriority.Values.Tertiary,
                    Title = [new() {CultureCode = "nb_NO", Value = "Avbryt"}],
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    IsDeleteAction = true
                }
            ],
            ApiActions =
            [
                new()
                {
                    Action = "open",
                    Endpoints =
                    [
                        new()
                        {
                            Url = new("https://example.com/api/dialogs/123456789"),
                            HttpMethod = HttpVerb.Values.GET,
                            ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json"),
                            DocumentationUrl = new("https://api-docs.example.com/dialogservice/open-action")
                        }

                    ]
                },

                new()
                {
                    Action = "confirm",
                    Endpoints =
                    [
                        new()
                        {
                            Url = new("https://example.com/api/dialogs/123456789/confirmReceived"),
                            HttpMethod = HttpVerb.Values.POST,
                            DocumentationUrl = new("https://api-docs.example.com/dialogservice/confirm-action")
                        }

                    ]
                },

                new()
                {
                    Action = "submit",
                    Endpoints =
                    [
                        new()
                        {
                            Url = new("https://example.com/api/dialogs/123456789"),
                            HttpMethod = HttpVerb.Values.POST,
                            RequestSchema = new("https://schemas.example.com/dialogservice/v1/dialogservice.json"),
                            ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json")
                        }

                    ]
                },

                new()
                {
                    Action = "delete",
                    Endpoints =
                    [
                        new()
                        {
                            Url = new("https://example.com/api/dialogs/123456789"),
                            HttpMethod = HttpVerb.Values.DELETE
                        }

                    ]
                }

            ],
            Activities =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    //CreatedAt = DateTimeOffset.UtcNow,
                    Type = DialogActivityType.Values.Submission,
                    PerformedBy = [new() {CultureCode = "nb_NO", Value = "Et navn"}],
                    ExtendedType = new Uri("SKE:1234-received-precheck-ok"),
                    Description =
                        [new() {CultureCode = "nb_NO", Value = "Innsending er mottatt og sendt til behandling"}],
                    DialogElementId = dialogElementGuid
                }
            ]
        };
        var createCommandResponse = await Application.Send(createCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out var _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createCommand);
    }
    // TODO: Add tests
}
