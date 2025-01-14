using FluentValidation;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;

// ReSharper disable once ClassNeverInstantiated.Global
public class SearchDialogInputValidator : AbstractValidator<SearchDialogInput>
{
    public SearchDialogInputValidator()
    {
        RuleFor(x => x.OrderBy)
            .NotEmpty()
            .When(x => x.OrderBy != null);

        RuleForEach(x => x.OrderBy)
            .Must(order => order.CreatedAt.HasValue ^ order.UpdatedAt.HasValue ^ order.DueAt.HasValue)
            .WithMessage("Exactly one property must be set on each OrderBy object.");
    }
}
