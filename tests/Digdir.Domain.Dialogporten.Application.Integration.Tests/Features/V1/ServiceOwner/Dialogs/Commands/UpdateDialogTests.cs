using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class UpdateDialogTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    [Fact]
    public async Task New_Activity_Should_Be_Able_To_Refer_To_Old_Activity()
    {
        // Arrange
        var (_, createCommandResponse) = await GenerateDialogWithActivity();
        var getDialogQuery = new GetDialogQuery { DialogId = createCommandResponse.AsT0.Value };
        var getDialogDto = await Application.Send(getDialogQuery);

        var mapper = Application.GetMapper();
        var updateDialogDto = mapper.Map<UpdateDialogDto>(getDialogDto.AsT0);

        // New activity refering to old activity
        updateDialogDto.Activities.Add(new UpdateDialogDialogActivityDto
        {
            Type = DialogActivityType.Values.DialogClosed,
            RelatedActivityId = getDialogDto.AsT0.Activities.First().Id,
            PerformedBy = new UpdateDialogDialogActivityActorDto
            {
                ActorType = DialogActorType.Values.ServiceOwner
            }
        });

        // Act
        var updateResponse = await Application.Send(new UpdateDialogCommand { Id = createCommandResponse.AsT0.Value, Dto = updateDialogDto });

        // Assert
        updateResponse.TryPickT0(out var result, out _).Should().BeTrue();
        result.Should().NotBeNull();
    }

    private async Task<(CreateDialogCommand, CreateDialogResult)> GenerateDialogWithActivity()
    {
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        var activity = DialogGenerator.GenerateFakeDialogActivity(type: DialogActivityType.Values.Information);
        activity.PerformedBy.ActorId = DialogGenerator.GenerateRandomParty(forcePerson: true);
        activity.PerformedBy.ActorName = null;
        createDialogCommand.Activities.Add(activity);
        var createCommandResponse = await Application.Send(createDialogCommand);
        return (createDialogCommand, createCommandResponse);
    }
}
