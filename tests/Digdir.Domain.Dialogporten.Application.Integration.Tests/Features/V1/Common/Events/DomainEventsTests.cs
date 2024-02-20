using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Common.Events;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class DomainEventsTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    [Fact]
    public async Task Creates_DomainEvents_When_Dialog_Created()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateFakeDialog();

        // Act
        var _ = await Application.Send(createDialogCommand!);

        // Assert
        var outBoxMessages = Application.GetDbEntities<OutboxMessage>();
        var eventAssembly = typeof(OutboxMessage).Assembly;
        foreach (var outboxMessage in outBoxMessages)
        {
            var eventType = eventAssembly.GetType(outboxMessage.EventType);
            var domainEvent = JsonSerializer.Deserialize(outboxMessage.EventPayload, eventType);

            switch (domainEvent)
            {
                case DialogCreatedEvent dialogCreatedEvent:
                    dialogCreatedEvent.Should().NotBeNull();
                    dialogCreatedEvent.Id.Should().Be(createDialogCommand.Id);
                    dialogCreatedEvent.ServiceResource.Should().Be(createDialogCommand.ServiceResource);
                    dialogCreatedEvent.Party.Should().Be(createDialogCommand.Party);
                    dialogCreatedEvent.Status.Should().Be(createDialogCommand.Status);
                    break;
                default:
                    throw new Exception("Unknown domain event");
            }
        }
        Console.WriteLine(outBoxMessages);
    }

    // [Fact]
    // public async Task Creates_DomainEvents_When_Dialog_Updated()
    // {
    //     // Arrange
    //     var dialogId = Guid.NewGuid();
    //     var createCommand = new CreateDialogCommand
    //     {
    //         Id = dialogId,
    //         ServiceResource = "urn:altinn:resource:example_dialog_service",
    //         Party = "org:991825827",
    //         Status = DialogStatus.Values.New
    //     };
    //     var response = await Application.Send(createCommand);
    //     response.TryPickT0(out var result, out var _).Should().BeTrue();
    //
    //     // Act
    //     var updateCommand = new UpdateDialogCommand
    //     {
    //         Id = dialogId,
    //         ServiceResource = "urn:altinn:resource:example_dialog_service",
    //         Party = "org:991825827",
    //         Status = DialogStatus.Values.InProgress
    //     };
    //     var updateResponse = await Application.Send(updateCommand);
    //
    //     // Assert
    //     updateResponse.TryPickT0(out var updateResult, out var _).Should().BeTrue();
    //     //updateResult.Should().NotBeNull();
    //     //updateResult.Should().BeEquivalentTo(updateCommand);
    // }
    //
    // [Fact]
    // public async Task Creates_DomainEvents_When_Dialog_Deleted()
    // {
    //     // Arrange
    //     var dialogId = Guid.NewGuid();
    //     var createCommand = new CreateDialogCommand
    //     {
    //         Id = dialogId,
    //         ServiceResource = "urn:altinn:resource:example_dialog_service",
    //         Party = "org:991825827",
    //         Status = DialogStatus.Values.New
    //     };
    //     var response = await Application.Send(createCommand);
    //     response.TryPickT0(out var result, out var _).Should().BeTrue();
    //
    //     // Act
    //     var deleteCommand = new DeleteDialogCommand
    //     {
    //         Id = dialogId,
    //         ServiceResource = "urn:altinn:resource:example_dialog_service",
    //         Party = "org:991825827"
    //     };
    //     var deleteResponse = await Application.Send(deleteCommand);
    //
    //     // Assert
    //     deleteResponse.TryPickT0(out var deleteResult, out var _).Should().BeTrue();
    //     //deleteResult.Should().NotBeNull();
    //     //deleteResult.Should().BeEquivalentTo(deleteCommand);
    // }
}
