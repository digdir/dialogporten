using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common;

public record ValidationFailed(IEnumerable<ValidationFailure> Errors)
{
    public ValidationFailed(ValidationFailure error) : this(new[] { error }) { }
}

public record EntityNotFound(string Name, object Key)
{
    public string Message => $"Entity '{Name}' ({Key}) was not found.";
}

public record EntityNotFound<T>(object Key) : EntityNotFound(typeof(T).Name, Key);