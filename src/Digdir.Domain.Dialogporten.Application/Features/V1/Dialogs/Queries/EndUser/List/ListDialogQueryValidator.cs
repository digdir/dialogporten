using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerable;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.List;

internal sealed class ListDialogQueryValidator : AbstractValidator<ListDialogQuery>
{
    public ListDialogQueryValidator()
    {
        Include(new SortablePaginationParameterValidator<ListDialogQueryOrderDefinition, ListDialogDto>());
        RuleFor(x => x.Search)
            .MinimumLength(3)
            .When(x => x.Search is not null);

        RuleFor(x => x.SearchCultureCode)
            .Must(x => x is null || Localization.IsValidCultureCode(x))
            .WithMessage("'{PropertyName}' must be a valid culture code.");

        RuleFor(x => x)
            .Must(x => !x.ServiceResource.IsNullOrEmpty() || !x.Party.IsNullOrEmpty())
            .WithMessage($"Ether {nameof(ListDialogQuery.ServiceResource)} or {nameof(ListDialogQuery.Party)} must be specified.");

        RuleFor(x => x.Org!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.Org is not null);
        RuleFor(x => x.ServiceResource!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.ServiceResource is not null);
        RuleFor(x => x.Party!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.Party is not null);
        RuleFor(x => x.ExtendedStatus!.Count)
            .LessThanOrEqualTo(20)
            .When(x => x.ExtendedStatus is not null);

        RuleFor(x => x.Status).IsInEnum();
        //RuleFor(x => x.OrderBy).IsInEnum();
    }
}
