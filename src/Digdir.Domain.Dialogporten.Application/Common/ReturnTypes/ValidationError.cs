﻿using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public record ValidationError(IEnumerable<ValidationFailure> Errors)
{
    public ValidationError(ValidationFailure error) : this(new[] { error }) { }
}
