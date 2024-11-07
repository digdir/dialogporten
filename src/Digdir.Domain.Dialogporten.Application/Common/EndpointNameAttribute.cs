using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Common;

/// <summary>
/// Attribute to specify which endpoint name MassTransit will use when wrapping <see cref="INotificationHandler{T}"/>.<see cref="INotificationHandler{T}.Handle(T, CancellationToken)"/> methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
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
