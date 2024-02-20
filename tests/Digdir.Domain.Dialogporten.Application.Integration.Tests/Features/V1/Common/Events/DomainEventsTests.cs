using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Dialogs;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Common.Events;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class DomainEventsTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    [Fact]
    public async Task Creates_CloudEvents_When_Dialog_Created()
    {
        // Arrange
        var allActivityTypes = Enum.GetValues<DialogActivityType.Values>().ToList();
        var activities = allActivityTypes.Select(activityType =>
            DialogGenerator.GenerateFakeDialogActivity(activityType)).ToList();

        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, activities: activities);

        // Act
        _ = await Application.Send(createDialogCommand);
        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogCreatedDomainEvent)));

        cloudEvents.Count(cloudEvent =>
                cloudEvent.Type == CloudEventTypes.Get(nameof(DialogElementCreatedDomainEvent)))
            .Should()
            .Be(createDialogCommand.Elements.Count);

        allActivityTypes.ForEach(activityType =>
            cloudEvents.Should().ContainSingle(cloudEvent =>
                cloudEvent.Type == CloudEventTypes.Get(activityType.ToString())));

        cloudEvents.Count
            .Should()
            .Be(
                createDialogCommand.Elements.Count
                + createDialogCommand.Activities.Count
                + 1); // + 1 for the dialog created event
    }
}
