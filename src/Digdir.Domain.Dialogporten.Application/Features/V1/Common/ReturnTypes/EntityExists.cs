using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record EntityExists<T>(IEnumerable<object> Keys) : EntityExists(typeof(T).Name, Keys)
{
    public EntityExists(IEnumerable<Guid> keys) : this(keys.Cast<object>()) { }
    public EntityExists(IEnumerable<int> keys) : this(keys.Cast<object>()) { }
    public EntityExists(IEnumerable<long> keys) : this(keys.Cast<object>()) { }
    public EntityExists(Guid keys) : this(new object[] { keys }) { }
    public EntityExists(int keys) : this(new object[] { keys }) { }
    public EntityExists(long keys) : this(new object[] { keys }) { }
}

public record EntityExists(string Name, IEnumerable<object> Keys)
{
    public string Message => $"Entity '{Name}' with the following key(s) allready exists: ({string.Join(", ", Keys)}).";
    public EntityExists(string name, IEnumerable<Guid> keys) : this(name, keys.Cast<object>()) { }
    public EntityExists(string name, IEnumerable<int> keys) : this(name, keys.Cast<object>()) { }
    public EntityExists(string name, IEnumerable<long> keys) : this(name, keys.Cast<object>()) { }
    public EntityExists(string name, Guid key) : this(name, new object[] { key }) { }
    public EntityExists(string name, int key) : this(name, new object[] { key }) { }
    public EntityExists(string name, long key) : this(name, new object[] { key }) { }

    public List<ValidationFailure> ToValidationResults() => new() { new ValidationFailure(Name, Message) };
}