using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using FluentAssertions;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Features.V1.Dialogs.Queries;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class GetDialogTests : ApplicationCollectionFixture
{
    public GetDialogTests(DialogApplication application) : base(application)
    {
    }

    [Fact(Skip = "While preparing github actions")]
    public async Task Get_ReturnsDialog_WhenDialogExists()
    {
        // Arrange
        var dialogElementGuid = Guid.NewGuid();
        var createCommand = new CreateDialogCommand
        {
            Id = Guid.NewGuid(),
            ServiceResource = "example_dialog_service",
            Party = "org:991825827",
            StatusId = DialogStatus.Enum.InProgress,
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
                    Id = dialogElementGuid,
                    DisplayName = new() { new() { CultureCode = "nb_NO", Value = "Dette er et vedlegg" } },
                    Type = new Uri("some:type"),
                    AuthorizationAttribute = "attachment1",
                    Urls = new List<CreateDialogDialogElementUrlDto>
                    {
                        new ()
                        {
                            ConsumerTypeId = DialogElementUrlConsumerType.Enum.Gui,
                            ContentTypeHint = "application/pdf",
                            Url = new Uri("http://example.com/some/deep/link/to/attachment1.pdf")
                        }
                    }
                },
            },
            GuiActions = new()
            {
                new() {
                    Action = "open",
                    TypeId = DialogGuiActionPriority.Enum.Primary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Åpne i dialogtjeneste" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789")},
                new() {
                    Action = "confirm",
                    TypeId = DialogGuiActionPriority.Enum.Secondary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Bekreft mottatt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogs/123456789/confirmReceived"),
                    AuthorizationAttribute = "somesubresource",
                    IsBackChannel = true},
                new() {
                    Action = "delete",
                    TypeId = DialogGuiActionPriority.Enum.Tertiary,
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
            Activity = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    TypeId = DialogActivityType.Enum.Submission,
                    PerformedBy = "Some Name",
                    ExtendedType = new Uri("SKE:1234-received-precheck-ok"),
                    Description = new() { new() { CultureCode = "nb_NO", Value = "Innsending er mottatt og sendt til behandling" } },
                    DialogElementId = dialogElementGuid
                }
            }
        };
        var createCommandResponse = await Application.Send(createCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { Id = createCommandResponse.AsT0 });

        // Assert
        response.TryPickT0(out var result, out var _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createCommand);
    }
    // TODO: Add tests
}
