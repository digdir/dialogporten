using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Purge;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using AutoMapper;
using FluentAssertions;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Delete;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.Common.Events;

[Collection(nameof(DialogCqrsCollectionFixture))]
public class DomainEventsTests(DialogApplication application) : ApplicationCollectionFixture(application)
{
    private static readonly IMapper Mapper;

    static DomainEventsTests()
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(ApplicationExtensions).Assembly);
        });

        Mapper = mapperConfiguration.CreateMapper();
    }

    [Fact]
    public async Task Creates_CloudEvents_When_Dialog_Created()
    {
        // Arrange
        var allActivityTypes = Enum.GetValues<DialogActivityType.Values>().ToList();
        var activities = allActivityTypes.Select(activityType =>
            DialogGenerator.GenerateFakeDialogActivity(activityType)).ToList();

        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            activities: activities,
            elements: DialogGenerator.GenerateFakeDialogElements(3));

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
                + 1); // +1 for the dialog created event
    }

    [Fact]
    public async Task Creates_CloudEvent_When_Dialog_Updates()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            activities: [],
            progress: 0,
            elements: []);

        _ = await Application.Send(createDialogCommand);

        var getDialogResult = await Application.Send(new GetDialogQuery { DialogId = dialogId });
        getDialogResult.TryPickT0(out var getDialogDto, out _);

        var updateDialogDto = Mapper.Map<UpdateDialogDto>(getDialogDto);

        // Act
        updateDialogDto.Progress = 1;

        var updateDialogCommand = new UpdateDialogCommand
        {
            Id = dialogId,
            Dto = updateDialogDto
        };

        _ = await Application.Send(updateDialogCommand);

        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogUpdatedDomainEvent)));

        cloudEvents.Should().NotContain(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogElementUpdatedDomainEvent)));
    }

    [Fact]
    public async Task Creates_CloudEvent_When_DialogElement_Updates()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            activities: [],
            elements: [DialogGenerator.GenerateFakeDialogElement()]);

        _ = await Application.Send(createDialogCommand);

        var getDialogResult = await Application.Send(new GetDialogQuery { DialogId = dialogId });
        getDialogResult.TryPickT0(out var getDialogDto, out _);

        var updateDialogDto = Mapper.Map<UpdateDialogDto>(getDialogDto);

        // Act
        updateDialogDto.Elements[0].ExternalReference = "newExternalReference";

        var updateDialogCommand = new UpdateDialogCommand
        {
            Id = dialogId,
            Dto = updateDialogDto
        };

        _ = await Application.Send(updateDialogCommand);

        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().NotContain(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogUpdatedDomainEvent)));

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogElementUpdatedDomainEvent)));
    }

    // Throws NRE on parent Dialog
    [Fact]
    public async Task Creates_CloudEvents_When_Deleting_DialogElement()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            activities: [],
            elements: [DialogGenerator.GenerateFakeDialogElement()]);

        _ = await Application.Send(createDialogCommand);

        var getDialogResult = await Application.Send(new GetDialogQuery { DialogId = dialogId });
        getDialogResult.TryPickT0(out var getDialogDto, out _);

        var updateDialogDto = Mapper.Map<UpdateDialogDto>(getDialogDto);

        // Act
        updateDialogDto.Elements = [];

        var updateDialogCommand = new UpdateDialogCommand
        {
            Id = dialogId,
            Dto = updateDialogDto
        };

        _ = await Application.Send(updateDialogCommand);

        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogElementDeletedDomainEvent)));

        cloudEvents.Should().NotContain(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogElementUpdatedDomainEvent)));
    }

    [Fact]
    public async Task Creates_CloudEvents_When_Dialog_Deleted()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, elements: [], activities: []);

        _ = await Application.Send(createDialogCommand);

        // Act
        var deleteDialogCommand = new DeleteDialogCommand
        {
            Id = dialogId
        };
        _ = await Application.Send(deleteDialogCommand);

        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogDeletedDomainEvent)));
    }

    [Fact]
    public async Task Creates_DialogDeletedEvent_When_Dialog_Purged()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, elements: [], activities: []);

        _ = await Application.Send(createDialogCommand);

        // Act
        var purgeCommand = new PurgeDialogCommand
        {
            DialogId = dialogId
        };

        await Application.Send(purgeCommand);
        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogDeletedDomainEvent)));
    }

    [Fact]
    public async Task Creates_DialogElementDeleted_CloudEvent_When_Purging_Dialog()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            activities: [],
            elements: [DialogGenerator.GenerateFakeDialogElement()]);

        await Application.Send(createDialogCommand);

        // Act
        var purgeCommand = new PurgeDialogCommand
        {
            DialogId = dialogId
        };

        await Application.Send(purgeCommand);

        await Application.PublishOutBoxMessages();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogElementDeletedDomainEvent)));
    }
}
