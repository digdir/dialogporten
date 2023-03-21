using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using FluentAssertions;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Features.V1.Dialogues.Queries;

[Collection(nameof(DialogueCqrsCollectionFixture))]
public class GetDialogueTests : ApplicationCollectionFixture
{
    public GetDialogueTests(DialogueApplication application) : base(application)
    {
    }

    [Fact(Skip = "While preparing github actions")]
    public async Task Get_ReturnsDialogue_WhenDialogueExists()
    {
        // Arrange
        var createCommand = new CreateDialogueCommand
        {
            Id = Guid.NewGuid(),
            ServiceResourceIdentifier = "example_dialogue_service",
            Party = "org:991825827",
            StatusId = DialogueStatus.Enum.InProgress,
            ExtendedStatus = "SKE-ABC",
            //DueAtUtc = new(2022, 12, 01),
            //ExpiresAtUtc = new(2023, 12, 01),
            Title = new() { new() { CultureCode = "nb_NO", Value = "Et eksempel på en tittel" } },
            SenderName = new() { new() { CultureCode = "nb_NO", Value = "Overstyrt avsendernavn (bruker default tjenesteeiers navn)" } },
            Body = new() { new() { CultureCode = "nb_NO", Value = "Innhold med <em>begrenset</em> HTML-støtte. Dette innholdet vises når dialogen ekspanderes." } },
            TokenScopes = new() { new() { Value = "serviceprovider:myservice" } },
            Attachments = new()
            {
                new()
                {
                    DisplayName = new() { new() { CultureCode = "nb_NO", Value = "Dette er et vedlegg" } },
                    SizeInBytes = 123456,
                    ContentType = "application/pdf",
                    Url = new("https://example.com/api/dialogues/123456789/attachments/1"),
                    Resource = "attachment1"
                }
            },
            GuiActions = new()
            {
                new() {
                    Action = "open",
                    TypeId = DialogueGuiActionType.Enum.Primary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Åpne i dialogtjeneste" } },
                    Url = new("https://example.com/some/deep/link/to/dialogues/123456789")},
                new() {
                    Action = "confirm",
                    TypeId = DialogueGuiActionType.Enum.Secondary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Bekreft mottatt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogues/123456789/confirmReceived"),
                    Resource = "somesubresource",
                    IsBackChannel = true},
                new() {
                    Action = "delete",
                    TypeId = DialogueGuiActionType.Enum.Tertiary,
                    Title = new() { new() { CultureCode = "nb_NO", Value = "Avbryt" } },
                    Url = new("https://example.com/some/deep/link/to/dialogues/123456789/confirmReceived"),
                    IsDeleteAction = true}
            },
            ApiActions = new()
            {
                new() {
                    Action = "open",
                    Url = new("https://example.com/api/dialogues/123456789"),
                    HttpMethod = "GET",
                    ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json"),
                    DocumentationUrl = new("https://api-docs.example.com/dialogueservice/open-action")},
                new() {
                    Action = "confirm",
                    Url = new("https://example.com/api/dialogues/123456789/confirmReceived"),
                    HttpMethod = "POST",
                    DocumentationUrl = new("https://api-docs.example.com/dialogueservice/confirm-action")},
                new() {
                    Action = "submit",
                    Url = new("https://example.com/api/dialogues/123456789"),
                    HttpMethod = "POST",
                    RequestSchema = new("https://schemas.example.com/dialogueservice/v1/dialogueservice.json"),
                    ResponseSchema = new("https://schemas.altinn.no/dialogs/v1/dialogs.json")},
                new() {
                    Action = "delete",
                    Url = new("https://example.com/api/dialogues/123456789"),
                    HttpMethod = "DELETE"},
            },
            History = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAtUtc = DateTime.UtcNow,
                    TypeId = DialogueActivityType.Enum.Submission,
                    PerformedBy = "person:12018212345",
                    ExtendedType = "SKE-1234-received-precheck-ok",
                    Description = new() { new() { CultureCode = "nb_NO", Value = "Innsending er mottatt og sendt til behandling" } },
                    DetailsApiUrl = new("https://example.com/api/dialogues/123456789/received_submissions/fc6406df-6163-442a-92cd-e487423f2fd5"),
                    DetailsGuiUrl = new("https://example.com/dialogues/123456789/view_submission/fc6406df-6163-442a-92cd-e487423f2fd5")
                }
            }
        };
        var createCommandResponse = await Application.Send(createCommand);

        // Act
        var response = await Application.Send(new GetDialogueQuery { Id = createCommandResponse.AsT0 });

        // Assert
        response.TryPickT0(out var result, out var _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createCommand);
    }
    // TODO: Add tests
}