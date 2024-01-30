using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

internal sealed class SearchDialogQueryValidator : AbstractValidator<SearchDialogQuery>
{
    public SearchDialogQueryValidator()
    {
        Include(new PaginationParameterValidator<SearchDialogQueryOrderDefinition, SearchDialogDto>());
        RuleFor(x => x.Search)
            .MinimumLength(3)
            .When(x => x.Search is not null);

        RuleFor(x => x.SearchCultureCode)
            .Must(x => x is null || Localization.IsValidCultureCode(x))
            .WithMessage("'{PropertyName}' must be a valid culture code.");

        RuleFor(x => x)
            .Must(x => !x.ServiceResource.IsNullOrEmpty() || !x.Party.IsNullOrEmpty())
            .WithMessage($"Either {nameof(SearchDialogQuery.ServiceResource)} or {nameof(SearchDialogQuery.Party)} must be specified.");

        RuleForEach(x => x.Party)
            .IsValidPartyIdentifier();

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

        RuleForEach(x => x.Status).IsInEnum();
    }
}
