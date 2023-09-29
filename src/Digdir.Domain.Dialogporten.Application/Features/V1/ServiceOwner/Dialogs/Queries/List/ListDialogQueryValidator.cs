using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerable;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.List;

internal sealed class ListDialogQueryValidator : AbstractValidator<ListDialogQuery>
{
    public ListDialogQueryValidator()
    {
        Include(new PaginationParameterValidator<ListDialogQueryOrderDefinition, ListDialogDto>());
        RuleFor(x => x.Search)
            .MinimumLength(3)
            .When(x => x.Search is not null);

        RuleFor(x => x.SearchCultureCode)
            .Must(x => x is null || Localization.IsValidCultureCode(x))
            .WithMessage("'{PropertyName}' must be a valid culture code.");
        
        RuleFor(x => x.ServiceResource!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.ServiceResource is not null);
        
        RuleFor(x => x.Party!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.Party is not null);
        
        RuleFor(x => x.ExtendedStatus!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.ExtendedStatus is not null);

        RuleForEach(x => x.Status).IsInEnum();
    }
}