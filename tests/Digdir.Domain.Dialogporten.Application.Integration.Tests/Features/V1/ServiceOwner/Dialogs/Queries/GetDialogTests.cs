using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Queries;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class GetDialogTests : ApplicationCollectionFixture
{
    public GetDialogTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Get_ReturnsSimpleDialog_WhenDialogExists()
    {
        // Arrange
        var expectedDialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog(id: expectedDialogId);

        var createCommandResponse = await Application.Send(createDialogCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createDialogCommand, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_ReturnsDialog_WhenDialogExists()
    {
        // Arrange
        var expectedDialogId = Guid.NewGuid();
        var createCommand = DialogGenerator.GenerateFakeDialog(id: expectedDialogId);
        var createCommandResponse = await Application.Send(createCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createCommand, options => options.ExcludingMissingMembers());
    }
    // TODO: Add tests
}
