﻿using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;

public sealed record DialogActivityCreatedDomainEvent(
    Guid DialogId,
    Guid ActivityId,
    DialogActivityType.Values TypeId,
    string Party,
    string ServiceResource,
    string? Process,
    string? PrecedingProcess,
    Uri? ExtendedType) : DomainEvent;
