namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record EntityNotFound<T>(object Key) : EntityNotFound(typeof(T).Name, Key);

public record EntityNotFound(string Name, object Key)
{
    public string Message => $"Entity '{Name}' ({Key}) was not found.";
}
