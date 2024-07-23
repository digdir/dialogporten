using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Purge;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Tool.Dialogporten.GenerateFakeData;
using AutoMapper;
using FluentAssertions;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Delete;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;

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
            attachments: DialogGenerator.GenerateFakeDialogAttachments(3));

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

        allActivityTypes.ForEach(activityType =>
            cloudEvents.Should().ContainSingle(cloudEvent =>
                cloudEvent.Type == CloudEventTypes.Get(activityType.ToString())));

        cloudEvents.Count
            .Should()
            // +1 for the dialog created event
            .Be(createDialogCommand.Activities.Count + 1);
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
            attachments: []);

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
    }

    [Fact]
    public async Task Creates_CloudEvent_When_Attachments_Updates()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            attachments: []);

        _ = await Application.Send(createDialogCommand);

        var getDialogResult = await Application.Send(new GetDialogQuery { DialogId = dialogId });
        getDialogResult.TryPickT0(out var getDialogDto, out _);

        var updateDialogDto = Mapper.Map<UpdateDialogDto>(getDialogDto);

        // Act
        updateDialogDto.Attachments = [new UpdateDialogDialogAttachmentDto
        {
            DisplayName = DialogGenerator.GenerateFakeLocalizations(3),
            Urls = [new()
            {
                ConsumerType = DialogAttachmentUrlConsumerType.Values.Gui,
                Url = new Uri("https://example.com")
            }]
        }];

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
    }
    [Fact]
    public async Task Creates_CloudEvents_When_Dialog_Deleted()
    {
        // Arrange
        var dialogId = Guid.NewGuid();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, attachments: [], activities: []);

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
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, attachments: [], activities: []);

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

    // todo: create test and implement for when activity type is DialogClosed/Completed etc?
}
