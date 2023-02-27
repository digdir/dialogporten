using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;

internal sealed class UpdateDialogueCommandValidator : AbstractValidator<UpdateDialogueCommand>
{
    public UpdateDialogueCommandValidator()
    {
        // TODO: Add validation rules.
        RuleFor(x => x.Dto).NotEmpty();
        RuleFor(x => x.Dto.Org).NotEmpty();
    }
}
