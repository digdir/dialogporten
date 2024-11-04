using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.NotificationCondition;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.NotificationCondition;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class NotificationConditionTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    private static readonly bool[] ExpectedSendNotificationsValues = [true, false];

    public static IEnumerable<object[]> NotificationConditionTestData() =>
        from bool expectedSendNotificationValue in ExpectedSendNotificationsValues
        from DialogActivityType.Values activityType in Enum.GetValues(typeof(DialogActivityType.Values))
        from NotificationConditionType conditionType in Enum.GetValues(typeof(NotificationConditionType))
        select new object[] { activityType, conditionType, expectedSendNotificationValue };


    [Theory, MemberData(nameof(NotificationConditionTestData))]
    public async Task SendNotification_Should_Be_True_When_Conditions_Are_Met(
        DialogActivityType.Values activityType,
        NotificationConditionType conditionType,
        bool expectedSendNotificationValue)
    {
        // Arrange
        var createDialogCommand = DialogGenerator.GenerateSimpleFakeDialog();
        switch (conditionType)
        {
            case NotificationConditionType.Exists when expectedSendNotificationValue:
            case NotificationConditionType.NotExists when !expectedSendNotificationValue:
                AddActivityRequirements(createDialogCommand, activityType);
                break;
        }

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

        // Act
        var queryResult = await Application.Send(notificationConditionQuery);

        // Assert
        queryResult.TryPickT0(out var notificationConditionResult, out _);
        queryResult.IsT0.Should().BeTrue();
        notificationConditionResult.SendNotification.Should().Be(expectedSendNotificationValue);
    }

    private static void AddActivityRequirements(
        CreateDialogCommand createDialogCommand,
        DialogActivityType.Values activityType)
    {
        var activity = DialogGenerator.GenerateFakeDialogActivity(type: activityType);
        createDialogCommand.Activities.Add(activity);

        if (activityType is not DialogActivityType.Values.TransmissionOpened) return;

        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(type: DialogTransmissionType.Values.Information)[0];
        createDialogCommand.Transmissions.Add(transmission);
        createDialogCommand.Activities[0].TransmissionId = createDialogCommand.Transmissions[0].Id;
    }

    [Fact]
    public async Task NotFound_Should_Be_Returned_When_Dialog_Does_Not_Exist()
    {
        // Arrange
        var notificationConditionQuery = new NotificationConditionQuery
        {
            DialogId = Guid.NewGuid(),
            ActivityType = DialogActivityType.Values.Information,
            ConditionType = NotificationConditionType.Exists,
        };

        // Act
        var queryResult = await Application.Send(notificationConditionQuery);

        // Assert
        queryResult.TryPickT2(out var notFound, out _);
        queryResult.IsT2.Should().BeTrue();
        notFound.Should().NotBeNull();
    }
}
