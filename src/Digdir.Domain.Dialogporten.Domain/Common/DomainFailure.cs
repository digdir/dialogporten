namespace Digdir.Domain.Dialogporten.Domain.Common;

public class DomainFailure 
{
    public string PropertyName { get; init; }
    public string ErrorMessage { get; init; }

    public DomainFailure(string propertyName, string error)
    {
        PropertyName = propertyName;
        ErrorMessage = error;
    }

    public static DomainFailure EntiryExists(string propertyName, string entityName, IEnumerable<Guid> keys)
        => new(propertyName, $"Entity '{entityName}' with the following key(s) allready exists: ({string.Join(", ", keys)}).");

    public static DomainFailure EntiryExists<T>(IEnumerable<Guid> keys)
    {
        var entityName = typeof(T).Name;
        return EntiryExists(entityName, entityName, keys);
    }
}
