using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public record EntityDeleted<T>(IEnumerable<object> Keys) : EntityDeleted(typeof(T).Name, Keys)
{
    public EntityDeleted(IEnumerable<Guid> keys) : this(keys.Cast<object>()) { }
    public EntityDeleted(Guid keys) : this(new object[] { keys }) { }
}

public record EntityDeleted(string Name, IEnumerable<object> Keys)
{
    public string Message => $"Entity '{Name}' with the following key(s) is removed: ({string.Join(", ", Keys)}).";
    public EntityDeleted(string name, IEnumerable<Guid> keys) : this(name, keys.Cast<object>()) { }
    public EntityDeleted(string name, Guid key) : this(name, new object[] { key }) { }

    public List<ValidationFailure> ToValidationResults() => [new ValidationFailure(Name, Message)];
}