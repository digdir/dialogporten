using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

internal sealed class UpdateDialogCommandValidator : AbstractValidator<UpdateDialogCommand>
{
    public UpdateDialogCommandValidator()
    {
        // TODO: Add validation rules.
        RuleFor(x => x.Dto).NotEmpty();
        //RuleFor(x => x.Dto.Org).NotEmpty();
    }
}
