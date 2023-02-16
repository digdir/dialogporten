using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

public sealed class DialogueDbContext : DbContext, IDialogueDbContext
{
    internal DialogueDbContext(DbContextOptions<DialogueDbContext> options) : base(options) { }

    public DbSet<DialogueEntity> Dialogues => Set<DialogueEntity>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>(x => x.HaveMaxLength(255));
        configurationBuilder.Properties<Uri>(x => x.HaveMaxLength(1023));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.RemovePluralizingTableNameConvention()
            .SetCrossCuttingTimeSpanToStringConverter()
            .AddAuditableEntities()
            .ApplyConfigurationsFromAssembly(typeof(DialogueDbContext).Assembly);
}