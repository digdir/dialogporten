using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure.GraphQl;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Constants = Digdir.Domain.Dialogporten.Infrastructure.GraphQl.GraphQlSubscriptionConstants;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;

internal sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private readonly ITransactionTime _transactionTime;
    private readonly ITopicEventSender _topicEventSender;
    private readonly ILogger<ConvertDomainEventsToOutboxMessagesInterceptor> _logger;

    private List<IDomainEvent> _domainEvents = [];

    public ConvertDomainEventsToOutboxMessagesInterceptor(
        ITransactionTime transactionTime,
        ITopicEventSender topicEventSender,
        ILogger<ConvertDomainEventsToOutboxMessagesInterceptor> logger)
    {
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _topicEventSender = topicEventSender ?? throw new ArgumentNullException(nameof(topicEventSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        _domainEvents = dbContext.ChangeTracker.Entries()
            .SelectMany(x =>
                x.Entity is IEventPublisher publisher
                    ? publisher.PopDomainEvents()
                    : [])
            .ToList();

        foreach (var domainEvent in _domainEvents)
        {
            domainEvent.OccuredAt = _transactionTime.Value;
        }

        var outboxMessages = _domainEvents
            .Select(OutboxMessage.Create)
            .ToList();

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = _domainEvents
                .Select(x => x switch
                {
                    DialogUpdatedDomainEvent dialogUpdatedDomainEvent => new DialogEventPayload
                    {
                        Id = dialogUpdatedDomainEvent.DialogId,
                        Type = DialogEventType.DialogUpdated
                    },
                    DialogDeletedDomainEvent dialogDeletedDomainEvent => new DialogEventPayload
                    {
                        Id = dialogDeletedDomainEvent.DialogId,
                        Type = DialogEventType.DialogDeleted
                    },
                    _ => (DialogEventPayload?)null
                })
                .Where(x => x is not null)
                .Cast<DialogEventPayload>()
                .Select(x => _topicEventSender.SendAsync(
                        $"{Constants.DialogEventsTopic}{x.Id}",
                        x,
                        cancellationToken)
                    .AsTask());
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send domain events to graphQL subscription");
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
