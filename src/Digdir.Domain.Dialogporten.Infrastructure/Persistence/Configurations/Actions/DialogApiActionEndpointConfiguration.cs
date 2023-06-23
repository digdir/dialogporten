using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Actions;

internal sealed class DialogApiActionEndpointConfiguration : IEntityTypeConfiguration<DialogApiActionEndpoint>
{
    public void Configure(EntityTypeBuilder<DialogApiActionEndpoint> builder)
    {

    }
}
