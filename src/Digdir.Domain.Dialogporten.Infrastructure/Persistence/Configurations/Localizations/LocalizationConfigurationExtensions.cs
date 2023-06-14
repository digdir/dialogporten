using Digdir.Domain.Dialogporten.Domain.Localizations;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Localizations;

internal static class LocalizationConfigurationExtensions
{
    public static ModelBuilder ApplyLocalizationSetRestrictDeleteBehaviour(this ModelBuilder modelBuilder)
    {
        var referencingForeignKeys = modelBuilder
            .Entity<LocalizationSet>()
            .Metadata
            .GetReferencingForeignKeys();

        foreach (var referencingForeignKey in referencingForeignKeys)
        {
            referencingForeignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        return modelBuilder;
    }
}
