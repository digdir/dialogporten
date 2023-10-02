using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.EventPublisher;

internal static class EventPublisherExtensions
{
    internal static ModelBuilder AddEventPublisher(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<IEventPublisher>(builder =>
        {
            builder.Ignore(nameof(IEventPublisher.DomainEvents));
        });
    }
}