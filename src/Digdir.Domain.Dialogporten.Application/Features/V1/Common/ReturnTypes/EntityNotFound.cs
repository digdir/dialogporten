using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record EntityNotFound<T>(IEnumerable<object> Keys) : EntityNotFound(typeof(T).Name, Keys)
{
    public EntityNotFound(IEnumerable<Guid> keys) : this(keys.Cast<object>()) { }
    public EntityNotFound(IEnumerable<int> keys) : this(keys.Cast<object>()) { }
    public EntityNotFound(IEnumerable<long> keys) : this(keys.Cast<object>()) { }
    public EntityNotFound(Guid keys) : this(new object[] { keys }) { }
    public EntityNotFound(int keys) : this(new object[] { keys }) { }
    public EntityNotFound(long keys) : this(new object[] { keys }) { }
}

public record EntityNotFound(string Name, IEnumerable<object> Keys)
{
    public string Message => $"Entity '{Name}' with the following key(s) was not found: ({string.Join(", ", Keys)}).";
    public EntityNotFound(string name, IEnumerable<Guid> keys) : this(name, keys.Cast<object>()) { }
    public EntityNotFound(string name, IEnumerable<int> keys) : this(name, keys.Cast<object>()) { }
    public EntityNotFound(string name, IEnumerable<long> keys) : this(name, keys.Cast<object>()) { }
    public EntityNotFound(string name, Guid key) : this(name, new object[] { key }) { }
    public EntityNotFound(string name, int key) : this(name, new object[] { key }) { }
    public EntityNotFound(string name, long key) : this(name, new object[] { key }) { }

    public List<ValidationFailure> ToValidationResults() => new() { new ValidationFailure(Name, Message) };
}