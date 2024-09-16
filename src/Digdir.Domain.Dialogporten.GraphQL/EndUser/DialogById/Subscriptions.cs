using Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;
using HotChocolate.Authorization;
using Constants = Digdir.Domain.Dialogporten.Infrastructure.GraphQl.GraphQlSubscriptionConstants;

// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

public sealed class Subscriptions
{
    [Subscribe]
    [Authorize(AuthorizationPolicy.EndUserSubscription, ApplyPolicy.Validation)]
    [Topic($"{Constants.DialogUpdatedTopic}{{{nameof(dialogId)}}}")]
    public DialogUpdatedPayload DialogUpdated(Guid dialogId,
        [EventMessage] Guid eventMessage)
    {
        ArgumentNullException.ThrowIfNull(eventMessage);
        return new DialogUpdatedPayload { Id = dialogId };
    }
}
