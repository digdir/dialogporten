using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Delete;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class DeleteDialogTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    [Fact]
    public async Task Deleting_Dialog_Should_Set_DeletedAt()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createDialogResponse = await Application.Send(createDialogCommand);

        // Act
        var dialogId = createDialogResponse.AsT0.Value;
        var deleteDialogCommand = new DeleteDialogCommand { Id = dialogId };
        await Application.Send(deleteDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = dialogId };
        var getDialogResponse = await Application.Send(getDialogQuery);

        // Assert
        getDialogResponse.TryPickT0(out var dialog, out _).Should().BeTrue();
        dialog.Should().NotBeNull();
        dialog.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Updating_Deleted_Dialog_Should_Return_BadRequest()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createDialogResponse = await Application.Send(createDialogCommand);

        var dialogId = createDialogResponse.AsT0.Value;
        var deleteDialogCommand = new DeleteDialogCommand { Id = dialogId };
        await Application.Send(deleteDialogCommand);

        var getDialogQuery = new GetDialogQuery { DialogId = dialogId };
        var getDialogResponse = await Application.Send(getDialogQuery);
        getDialogResponse.TryPickT0(out var dialog, out _).Should().BeTrue();

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(dialog);

        // Act
        var updateDialogCommand = new UpdateDialogCommand { Id = dialogId, Dto = updateDialogDto };
        var updateDialogResponse = await Application.Send(updateDialogCommand);

        // Assert
        updateDialogResponse.TryPickT2(out var validationError, out _).Should().BeTrue();
        validationError.Should().NotBeNull();
        validationError.Reasons.Should().Contain(e => e.Contains("cannot be updated"));
        validationError.Reasons.Should().Contain(e => e.Contains(dialogId.ToString()));
    }
}
