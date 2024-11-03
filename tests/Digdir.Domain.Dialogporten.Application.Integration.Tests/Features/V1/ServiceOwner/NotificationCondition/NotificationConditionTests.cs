using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.NotificationCondition;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.NotificationCondition;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class NotificationConditionTests : ApplicationCollectionFixture
{
    public NotificationConditionTests(DialogApplication application) : base(application)
    {
    }

    public static IEnumerable<object[]> NotificationConditionTestData()
    {
        foreach (var activityType in Enum.GetValues(typeof(DialogActivityType.Values)))
        {
            foreach (var conditionType in Enum.GetValues(typeof(NotificationConditionType)))
            {
                yield return [activityType, conditionType];
            }
        }
    }

    [Theory]
    [MemberData(nameof(NotificationConditionTestData))]
    public async Task NotificationCondition_Should_Return_True_For_SendNotification_When_Condition_Is_Met(
        DialogActivityType.Values activityType,
        NotificationConditionType conditionType)
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        if (conditionType == NotificationConditionType.Exists)
        {
            AddActivityExistsRequirements(createDialogCommand, activityType);
        }

        // Act
        var response = await Application.Send(createDialogCommand);
        response.TryPickT0(out var dialogId, out _);

        var notificationConditionQuery = new NotificationConditionQuery
        {
            DialogId = dialogId.Value,
            ActivityType = activityType,
            ConditionType = conditionType,
        };

        if (activityType is DialogActivityType.Values.TransmissionOpened)
        {
            var transmissionId = createDialogCommand.Transmissions.FirstOrDefault()?.Id ?? Guid.NewGuid();
            notificationConditionQuery.TransmissionId = transmissionId;
        }

        var queryResult = await Application.Send(notificationConditionQuery);

        // Assert
        queryResult.TryPickT0(out var notificationConditionResult, out _);
        queryResult.IsT0.Should().BeTrue();
        notificationConditionResult.SendNotification.Should().BeTrue();
    }

    private static void AddActivityExistsRequirements(
        CreateDialogCommand createDialogCommand,
        DialogActivityType.Values activityType)
    {
        createDialogCommand.Activities.Add(new ActivityDto
        {
            Type = activityType,
            PerformedBy = new ActivityPerformedByActorDto
            {
                ActorType = ActorType.Values.ServiceOwner
            }
        });

        if (activityType is DialogActivityType.Values.TransmissionOpened)
        {
            var transmission =
                DialogGenerator.GenerateFakeDialogTransmissions(type: DialogTransmissionType.Values.Information)[0];
            createDialogCommand.Transmissions.Add(transmission);
            createDialogCommand.Activities[0].TransmissionId = createDialogCommand.Transmissions[0].Id;
        }
        else if (activityType is DialogActivityType.Values.Information)
        {
            createDialogCommand.Activities[0].Description =
                [new LocalizationDto { LanguageCode = "nb", Value = "Info" }];
        }
    }
}
