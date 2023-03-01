namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record EntityExists<T>(object Key) : EntityExists(typeof(T).Name, Key);

public record EntityExists(string Name, object Key)
{
    public string Message => $"Entity '{Name}' ({Key}) allready exists.";
}