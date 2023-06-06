using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using FluentAssertions;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Features.V1.Dialogs.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class CreateDialogTests : ApplicationCollectionFixture
{
    public CreateDialogTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Create_CreateDialog_WhenDataIsValid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var createCommand = new CreateDialogCommand
        {
            Id = expectedId,
            ServiceResourceIdentifier = "example_dialog_service",
            Party = "org:991825827",
            StatusId = DialogStatus.Enum.InProgress,
            ExtendedStatus = "SKE-ABC",
            //DueAtUtc = new(2022, 12, 01),
            //ExpiresAtUtc = new(2023, 12, 01),
            Title = new() { new() { CultureCode = "nb_NO", Value = "Et eksempel på en tittel" } },
            SenderName = new() { new() { CultureCode = "nb_NO", Value = "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } },
            Body = new() { new() { CultureCode = "nb_NO", Value = "Innhold med <em>begrenset</em> HTML-støtte. Dette innholdet vises når dialogen ekspanderes." } },
            DialogElements = new()
            {
                new()
                {
                    DisplayName = new() { new() { CultureCode = "nb_NO", Value = "Dette er et vedlegg" } },
                    SizeInBytes = 123456,
                    ContentType = "application/pdf",
                    Url = new("https://example.com/api/dialogs/123456789/attachments/1"),
                    Resource = "attachment1"
                }
            },
            GuiActions = new()
            {
                new() {
                    Action = "open",
                    TypeId = DialogGuiActionType.Enum.Primary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Åpne i dialogtjeneste" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789")},
                new() {
                    Action = "confirm",
                    TypeId = DialogGuiActionType.Enum.Secondary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Bekreft mottatt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    Resource = "somesubresource",
                    IsBackChannel = true},
                new() {
                    Action = "delete",
                    TypeId = DialogGuiActionType.Enum.Tertiary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Avbryt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    IsDeleteAction = true}
            },
            ApiActions = new()
            {
                new() {
                    Action = "open",
                    Url = new("https://example.com/api/dialogs/123456789"),
                    HttpMethod = "GET",
                    ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json"),
                    DocumentationUrl = new("https://api-docs.example.com/dialogservice/open-action")},
                new() {
                    Action = "confirm",
                    Url = new("https://example.com/api/dialogs/123456789/confirmReceived"),
                    HttpMethod = "POST",
                    DocumentationUrl = new("https://api-docs.example.com/dialogservice/confirm-action")},
                new() {
                    Action = "submit",
                    Url = new("https://example.com/api/dialogs/123456789"),
                    HttpMethod = "POST",
                    RequestSchema = new("https://schemas.example.com/dialogservice/v1/dialogservice.json"),
                    ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json")},
                new() {
                    Action = "delete",
                    Url = new("https://example.com/api/dialogs/123456789"),
                    HttpMethod = "DELETE"},
            },
            History= new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    TypeId = DialogActivityType.Enum.Submission,
                    PerformedBy = "person:12018212345",
                    ExtendedType = "SKE-1234-received-precheck-ok",
                    Description = new() { new() { CultureCode = "nb_NO", Value = "Innsending er mottatt og sendt til behandling" } },
                    DetailsApiUrl = new("https://example.com/api/dialogs/123456789/received_submissions/fc6406df-6163-442a-92cd-e487423f2fd5"),
                    DetailsGuiUrl = new("https://example.com/dialogs/123456789/view_submission/fc6406df-6163-442a-92cd-e487423f2fd5")
                }
            }
        };

        // Act
        var result = await Application.Send(createCommand);

        // Assert
        result.AsT0.Should().Be(expectedId);
    }

    // TODO: Add tests
}
