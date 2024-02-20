using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
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
        var createDialogCommand = DialogGenerator.GenerateFakeDialog() as CreateDialogCommand;

        // Act
        var response = await Application.Send(createDialogCommand!);

        // Assert
        response.TryPickT0(out _, out _).Should().BeTrue();
        //result.Should().NotBeNull();
        //result.Should().BeEquivalentTo(createCommand);
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
