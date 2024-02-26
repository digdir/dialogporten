using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Http;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class CreateDialogTests : ApplicationCollectionFixture
{
    public CreateDialogTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Create_CreatesDialog_WhenDialogIsSimple()
    {
        // Arrange
        var expectedDialogId = Guid.NewGuid();
        var createCommand = DialogGenerator.GenerateSimpleFakeDialog(id: expectedDialogId);

        // Act
        var response = await Application.Send(createCommand);

        // Assert
        response.TryPickT0(out var success, out var _).Should().BeTrue();
        success.Value.Should().Be(expectedDialogId);
    }

    [Fact]
    public async Task Create_CreateDialog_WhenDialogIsComplex()
    {
        // Arrange
        var expectedDialogId = Guid.NewGuid();
        // var elements = DialogGenerator.GenerateFakeDialogElement());
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: expectedDialogId);

        // Act
        var result = await Application.Send(createDialogCommand);

        // Assert
        result.TryPickT0(out var success, out var _).Should().BeTrue();
        success.Value.Should().Be(expectedDialogId);
    }

    // TODO: Add tests
}
