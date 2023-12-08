using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Http;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class CreateDialogTests : ApplicationCollectionFixture
{
    public CreateDialogTests(DialogApplication application) : base(application) { }

    //[Fact]
    //public async Task Create_CreatesDialog_WhenDialogIsSimple()
    //{
    //    // Arrange
    //    var expectedDialogId = Guid.NewGuid();
    //    var createCommand = new CreateDialogCommand
    //    {
    //        Id = expectedDialogId,
    //        ServiceResource = new("urn:altinn:resource:example_dialog_service"),
    //        Party = "org:991825827",
    //        StatusId = DialogStatus.Enum.InProgress
    //    };

    //    // Act
    //    var response = await Application.Send(createCommand);

    //    // Assert
    //    response.TryPickT0(out var result, out var _).Should().BeTrue();
    //    //result.Should().NotBeNull();
    //    //result.Should().BeEquivalentTo(createCommand);
    //}

    [Fact(Skip = "Must fix for validation rules.")]
    public async Task Create_CreateDialog_WhenDataIsValid()
    {
        // Arrange
        var expectedDialogId = Guid.NewGuid();
        var expectedDialogElementId = Guid.NewGuid();
        var createCommand = new CreateDialogCommand
        {
            Id = expectedDialogId,
            ServiceResource = new("urn:altinn:resource:example_dialog_service"),
            Party = "org:991825827",
            Status = DialogStatus.Values.InProgress,
            ExtendedStatus = "SKE-ABC",
            //DueAt = new(2022, 12, 01),
            //ExpiresAt = new(2023, 12, 01),
            Title = new() { new() { CultureCode = "nb_NO", Value = "Et eksempel på en tittel" } },
            SenderName = new() { new() { CultureCode = "nb_NO", Value = "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } },
            Body = new() { new() { CultureCode = "nb_NO", Value = "Innhold med <em>begrenset</em> HTML-støtte. Dette innholdet vises når dialogen ekspanderes." } },
            Elements = new()
            {
                new()
                {
                    Id = expectedDialogElementId,
                    DisplayName = new() { new() { CultureCode = "nb_NO", Value = "Dette er et vedlegg" } },
                    Type = new Uri("some:type"),
                    AuthorizationAttribute = "attachment1",
                    Urls = new List<CreateDialogDialogElementUrlDto>
                    {
                        new ()
                        {
                            ConsumerType = DialogElementUrlConsumerType.Values.Gui,
                            MimeType = "application/pdf",
                            Url = new Uri("http://example.com/some/deep/link/to/attachment1.pdf")
                        }
                    }
                },
                new()
                {
                    RelatedDialogElementId = expectedDialogElementId,
                    DisplayName = new() { new() { CultureCode = "nb_NO", Value = "Dette er et relatert element" } },
                    Type = new Uri("some:type"),
                    AuthorizationAttribute = "attachment1",
                    Urls = new List<CreateDialogDialogElementUrlDto>
                    {
                        new ()
                        {
                            ConsumerType = DialogElementUrlConsumerType.Values.Api,
                            MimeType = "application/xml",
                            Url = new Uri("http://example.com/some/deep/link/to/attachment1.xml")
                        }
                    }
                }
            },
            GuiActions = new()
            {
                new() {
                    Action = "open",
                    Priority = DialogGuiActionPriority.Values.Primary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Åpne i dialogtjeneste" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789")},
                new() {
                    Action = "confirm",
                    Priority = DialogGuiActionPriority.Values.Secondary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Bekreft mottatt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    AuthorizationAttribute = "somesubresource",
                    IsBackChannel = true},
                new() {
                    Action = "delete",
                    Priority = DialogGuiActionPriority.Values.Tertiary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Avbryt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    IsDeleteAction = true}
            },
            ApiActions = new()
            {
                new() {
                    Action = "open",
                    Endpoints = new() {
                        new() {
                            Url = new("https://example.com/api/dialogs/123456789"),
                            HttpMethod = HttpVerb.Values.GET,
                            ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json"),
                            DocumentationUrl = new("https://api-docs.example.com/dialogservice/open-action")
                        },
                    }
                },
                new() {
                    Action = "confirm",
                    Endpoints = new() {
                        new() {
                            Url = new("https://example.com/api/dialogs/123456789/confirmReceived"),
                            HttpMethod = HttpVerb.Values.POST,
                            DocumentationUrl = new("https://api-docs.example.com/dialogservice/confirm-action")
                        },
                    }
                },
                new() {
                    Action = "submit",
                    Endpoints = new() {
                        new() {
                            Url = new("https://example.com/api/dialogs/123456789"),
                            HttpMethod = HttpVerb.Values.POST,
                            RequestSchema = new("https://schemas.example.com/dialogservice/v1/dialogservice.json"),
                            ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json")
                        },
                    }
                },
                new() {
                    Action = "delete",
                    Endpoints = new() {
                        new() {
                            Url = new("https://example.com/api/dialogs/123456789"),
                            HttpMethod = HttpVerb.Values.DELETE
                        },
                    }
                },
            },
            Activities = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    //CreatedAt = DateTimeOffset.UtcNow,
                    Type = DialogActivityType.Values.Submission,
                    PerformedBy = new() { new() { CultureCode = "nb_NO", Value = "person:12018212345" } },
                    ExtendedType = new Uri("SKE:1234-received-precheck-ok"),
                    Description = new() { new() { CultureCode = "nb_NO", Value = "Innsending er mottatt og sendt til behandling" } },
                    DialogElementId = expectedDialogElementId
                }
            }
        };

        // Act
        var result = await Application.Send(createCommand);

        // Assert
        result.AsT0.Should().Be(expectedDialogId);
    }

    // TODO: Add tests
}
