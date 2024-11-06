namespace Digdir.Domain.Dialogporten.Application.Common;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class EndpointNameAttribute : Attribute
{
    public string Name { get; }

    public EndpointNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        Name = name;
    }
}
