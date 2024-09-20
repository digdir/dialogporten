using Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;
using HotChocolate.Authorization;
using Constants = Digdir.Domain.Dialogporten.Infrastructure.GraphQl.GraphQlSubscriptionConstants;

// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

[Authorize(Policy = AuthorizationPolicy.EndUser)]
public sealed class Subscriptions
{
    [Subscribe]
    [Topic($"{Constants.DialogEventsTopic}{{{nameof(dialogId)}}}")]
    public DialogEventPayload DialogEvents(Guid dialogId,
        [EventMessage] DialogEventPayload eventMessage)
    {
        ArgumentNullException.ThrowIfNull(dialogId);
        ArgumentNullException.ThrowIfNull(eventMessage);
        return eventMessage;
    }
}
