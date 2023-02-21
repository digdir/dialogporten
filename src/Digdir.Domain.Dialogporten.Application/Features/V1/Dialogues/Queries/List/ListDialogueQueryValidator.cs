using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.List;

public class ListDialogueQueryValidator : AbstractValidator<ListDialogueQuery>
{
    public ListDialogueQueryValidator()
    {
        Include(new PaginationParameterValidator());
    }
}
