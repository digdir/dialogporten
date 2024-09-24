using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using HotChocolate.Subscriptions;
using MassTransit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Constants = Digdir.Domain.Dialogporten.Infrastructure.GraphQl.GraphQlSubscriptionConstants;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;

internal sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private readonly ITransactionTime _transactionTime;
    private readonly ITopicEventSender _topicEventSender;
    private readonly ILogger<ConvertDomainEventsToOutboxMessagesInterceptor> _logger;
    private readonly IPublishEndpoint? _publishEndpoint;

    private List<IDomainEvent> _domainEvents = [];

    public ConvertDomainEventsToOutboxMessagesInterceptor(
        ITransactionTime transactionTime,
        ITopicEventSender topicEventSender,
        ILogger<ConvertDomainEventsToOutboxMessagesInterceptor> logger,
        IPublishEndpoint? publishEndpoint = null)
    {
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _topicEventSender = topicEventSender ?? throw new ArgumentNullException(nameof(topicEventSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint;// ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
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

        if (_publishEndpoint is not null)
        {
            await Task.WhenAll(_domainEvents
                .Select(x => _publishEndpoint
                    .Publish(x, x.GetType(), cancellationToken)));
        }

        // var outboxMessages = _domainEvents
        //     .Select(OutboxMessage.Create)
        //     .ToList();
        //
        // dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in _domainEvents)
        {
            try
            {
                // If you are adding any additional cases to this switch,
                // please consider making the running of the tasks parallel
                var task = domainEvent switch
                {
                    DialogUpdatedDomainEvent dialogUpdatedDomainEvent => _topicEventSender.SendAsync(
                        $"{Constants.DialogUpdatedTopic}{dialogUpdatedDomainEvent.DialogId}",
                        dialogUpdatedDomainEvent.DialogId,
                        cancellationToken),
                    _ => ValueTask.CompletedTask
                };

                await task;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send domain event to graphQL subscription");
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
