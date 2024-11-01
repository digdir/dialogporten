using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;
using static Digdir.Domain.Dialogporten.Application.Integration.Tests.UuiDv7Utils;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Queries;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class GetDialogTests : ApplicationCollectionFixture
{
    public GetDialogTests(DialogApplication application) : base(application) { }

    [Fact]
    public async Task Get_ReturnsSimpleDialog_WhenDialogExists()
    {
        // Arrange
        var expectedDialogId = GenerateBigEndianUuidV7();
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog(id: expectedDialogId);

        var createCommandResponse = await Application.Send(createDialogCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createDialogCommand, options => options
            .ExcludingMissingMembers()
            .Excluding(x => x.UpdatedAt)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.SystemLabel));
    }

    [Fact]
    public async Task Get_ReturnsDialog_WhenDialogExists()
    {
        // Arrange
        var expectedDialogId = GenerateBigEndianUuidV7();
        var createCommand = DialogGenerator.GenerateFakeDialog(id: expectedDialogId);
        var createCommandResponse = await Application.Send(createCommand);

        // Act
        var response = await Application.Send(new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createCommand, options => options
            .ExcludingMissingMembers()
            .Excluding(x => x.UpdatedAt)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.SystemLabel));

        /*result.Activities.Should().BeEquivalentTo(createCommand.Activities,
            options => options.ExcludingMissingMembers().Excluding(x => x.PerformedBy.Id));

        result.Transmissions.Should().BeEquivalentTo(createCommand.Transmissions,
            options => options.ExcludingMissingMembers().Excluding(x => x.Sender.Id));*/
    }
    // TODO: Add tests
}
