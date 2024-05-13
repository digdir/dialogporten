using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

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
        response.TryPickT0(out var success, out _).Should().BeTrue();
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
        result.TryPickT0(out var success, out _).Should().BeTrue();
        success.Value.Should().Be(expectedDialogId);
    }

    // TODO: Add tests
}
