using Microsoft.EntityFrameworkCore;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.EventPublisher;

internal static class EventPublisherExtensions
{
    internal static ModelBuilder AddEventPublisher(this ModelBuilder modelBuilder) => modelBuilder;
}
