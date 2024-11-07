using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Common;

/// <summary>
/// Attribute to specify which endpoint name MassTransit will use when wrapping <see cref="INotificationHandler{T}.Handle(T, CancellationToken)"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///     <item>Will default to "{handlerType.Name}_{eventType.Name}" if not specified.</item>
///     <item>MassTransit will only wrap <see cref="INotificationHandler{TNotification}"/> where TNotification implements <see cref="IDomainEvent"/>.</item>
/// </list>
/// </remarks>
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

    public static string DefaultName(Type handlerType, Type eventType)
        => $"{handlerType.Name}_{eventType.Name}";
}
