﻿using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.List;

internal sealed class ListDialogQueryValidator : AbstractValidator<ListDialogQuery>
{
    public ListDialogQueryValidator()
    {
        Include(new PaginationParameterValidator<ListDialogQueryOrderDefinition, ListDialogDto>());
    }
}
