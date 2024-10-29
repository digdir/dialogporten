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
using Digdir.Domain.Dialogporten.Domain.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;
using MassTransit.Internals;
using MassTransit.Testing;
using static Digdir.Domain.Dialogporten.Application.Integration.Tests.UuiDv7Utils;

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
        var harness = await Application.ConfigureServicesWithMassTransitTestHarness();

        var allActivityTypes = Enum.GetValues<ActivityType.Values>().ToList();
        var activities = allActivityTypes
            .Select(activityType => DialogGenerator.GenerateFakeDialogActivity(activityType))
            .ToList();
        var transmissionOpenedActivity = activities
            .Single(x => x.Type == ActivityType.Values.TransmissionOpened);
        var transmission = DialogGenerator.GenerateFakeDialogTransmissions(1).First();
        transmissionOpenedActivity.TransmissionId = transmission.Id;
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            activities: activities,
            attachments: DialogGenerator.GenerateFakeDialogAttachments(3));
        createDialogCommand.Transmissions.Add(transmission);

        // Act
        await Application.Send(createDialogCommand);
        await harness.Consumed
            .SelectAsync<DialogCreatedDomainEvent>(x => x.Context.Message.DialogId == createDialogCommand.Id)
            .FirstOrDefault();
        await harness.Consumed
            .SelectAsync<DialogActivityCreatedDomainEvent>(x => x.Context.Message.DialogId == createDialogCommand.Id)
            .Take(activities.Count)
            .ToListAsync();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == createDialogCommand.Id.ToString());
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
        var harness = await Application.ConfigureServicesWithMassTransitTestHarness();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            activities: [],
            progress: 0,
            attachments: []);

        await Application.Send(createDialogCommand);

        var getDialogResult = await Application.Send(new GetDialogQuery { DialogId = createDialogCommand.Id!.Value });
        getDialogResult.TryPickT0(out var getDialogDto, out _);

        var updateDialogDto = Mapper.Map<UpdateDialogDto>(getDialogDto);

        // Act
        updateDialogDto.Progress = 1;

        var updateDialogCommand = new UpdateDialogCommand
        {
            Id = createDialogCommand.Id!.Value,
            Dto = updateDialogDto
        };

        await Application.Send(updateDialogCommand);
        await harness.Consumed
            .SelectAsync<DialogUpdatedDomainEvent>(x => x.Context.Message.DialogId == createDialogCommand.Id)
            .FirstOrDefault();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == createDialogCommand.Id!.Value.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogUpdatedDomainEvent)));
    }

    [Fact]
    public async Task Creates_CloudEvent_When_Attachments_Updates()
    {
        // Arrange
        var harness = await Application.ConfigureServicesWithMassTransitTestHarness();
        var dialogId = GenerateBigEndianUuidV7();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(
            id: dialogId,
            attachments: []);

        await Application.Send(createDialogCommand);

        var getDialogResult = await Application.Send(new GetDialogQuery { DialogId = dialogId });
        getDialogResult.TryPickT0(out var getDialogDto, out _);

        var updateDialogDto = Mapper.Map<UpdateDialogDto>(getDialogDto);

        // Act
        updateDialogDto.Attachments = [new AttachmentDto
        {
            DisplayName = DialogGenerator.GenerateFakeLocalizations(3),
            Urls = [new()
            {
                ConsumerType = AttachmentUrlConsumerType.Values.Gui,
                Url = new Uri("https://example.com")
            }]
        }];

        var updateDialogCommand = new UpdateDialogCommand
        {
            Id = dialogId,
            Dto = updateDialogDto
        };

        await Application.Send(updateDialogCommand);
        await harness.Consumed
            .SelectAsync<DialogUpdatedDomainEvent>(x => x.Context.Message.DialogId == dialogId)
            .FirstOrDefault();
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
        var harness = await Application.ConfigureServicesWithMassTransitTestHarness();
        var dialogId = GenerateBigEndianUuidV7();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, attachments: [], activities: []);

        await Application.Send(createDialogCommand);

        // Act
        var deleteDialogCommand = new DeleteDialogCommand
        {
            Id = dialogId
        };
        await Application.Send(deleteDialogCommand);
        await harness.Consumed
            .SelectAsync<DialogDeletedDomainEvent>(x => x.Context.Message.DialogId == dialogId)
            .FirstOrDefault();
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
        var harness = await Application.ConfigureServicesWithMassTransitTestHarness();
        var dialogId = GenerateBigEndianUuidV7();
        var createDialogCommand = DialogGenerator.GenerateFakeDialog(id: dialogId, attachments: [], activities: []);

        await Application.Send(createDialogCommand);

        // Act
        var purgeCommand = new PurgeDialogCommand
        {
            DialogId = dialogId
        };

        await Application.Send(purgeCommand);
        await harness.Consumed
            .SelectAsync<DialogDeletedDomainEvent>(x => x.Context.Message.DialogId == dialogId)
            .FirstOrDefault();
        var cloudEvents = Application.PopPublishedCloudEvents();

        // Assert
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.ResourceInstance == dialogId.ToString());
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Resource == createDialogCommand.ServiceResource);
        cloudEvents.Should().OnlyContain(cloudEvent => cloudEvent.Subject == createDialogCommand.Party);

        cloudEvents.Should().ContainSingle(cloudEvent =>
            cloudEvent.Type == CloudEventTypes.Get(nameof(DialogDeletedDomainEvent)));
    }
}
