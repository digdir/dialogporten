using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogs.Content;
internal class DialogContentConfiguration : IEntityTypeConfiguration<DialogContent>
{
    public void Configure(EntityTypeBuilder<DialogContent> builder)
    {
        builder.HasIndex(x => new { x.DialogId, x.TypeId }).IsUnique();
    }
}
