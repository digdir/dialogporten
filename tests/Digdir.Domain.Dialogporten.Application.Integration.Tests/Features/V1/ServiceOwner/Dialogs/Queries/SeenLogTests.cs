using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;
using GetDialogQueryEndUser = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get.GetDialogQuery;
using GetDialogQueryServiceOwner = Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get.GetDialogQuery;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Queries;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class SeenLogTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    [Fact]
    public async Task Get_Dialog_Should_Not_Return_User_Ids_Unhashed()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        // Call EndUser API to trigger SeenLog
        await Application.Send(new GetDialogQueryEndUser { DialogId = createCommandResponse.AsT0.Value });

        // Act
        var response = await Application.Send(new GetDialogQueryServiceOwner { DialogId = createCommandResponse.AsT0.Value });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();

        result.SeenSinceLastUpdate
            .Single()
            .ActorId
            .Should()
            .HaveLength(PersistentRandomSaltStringHasher.StringLength);
    }

    [Fact]
    public async Task Search_Dialog_Should_Not_Return_User_Ids_Unhashed()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        // Trigger SeenLog
        await Application.Send(new GetDialogQueryEndUser { DialogId = createCommandResponse.AsT0.Value });

        // Act
        var response = await Application.Send(new SearchDialogQuery
        {
            ServiceResource = [createDialogCommand.ServiceResource]
        });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();

        result.Items
            .Single()
            .SeenSinceLastUpdate
            .Single()
            .ActorId
            .Should()
            .HaveLength(PersistentRandomSaltStringHasher.StringLength);
    }

    [Fact]
    public async Task Get_SeenLog_Should_Not_Return_User_Ids_Unhashed()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        var triggerSeenLogResponse = await Application.Send(new GetDialogQueryEndUser { DialogId = createCommandResponse.AsT0.Value });
        var seenLogId = triggerSeenLogResponse.AsT0.SeenSinceLastUpdate.Single().Id;

        // Act
        var response = await Application.Send(new GetDialogSeenLogQuery
        {
            DialogId = createCommandResponse.AsT0.Value,
            SeenLogId = seenLogId
        });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();

        result.ActorId
            .Should()
            .HaveLength(PersistentRandomSaltStringHasher.StringLength);
    }

    [Fact]
    public async Task Search_SeenLog_Should_Not_Return_User_Ids_Unhashed()
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var createCommandResponse = await Application.Send(createDialogCommand);

        // Trigger SeenLog
        await Application.Send(new GetDialogQueryEndUser { DialogId = createCommandResponse.AsT0.Value });

        // Act
        var response = await Application.Send(new SearchDialogSeenLogQuery
        {
            DialogId = createCommandResponse.AsT0.Value,
        });

        // Assert
        response.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();

        result.Single()
            .ActorId
            .Should()
            .HaveLength(PersistentRandomSaltStringHasher.StringLength);
    }
}
