using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Digdir.Domain.Dialogporten.Infrastructure.Unit.Tests;

public class DialogDbContextTests
{
    [Fact]
    public void MigrationsShouldBeInSyncWithEntities()
    {
        var builder = new DbContextOptionsBuilder<DialogDbContext>()
            .UseNpgsql("DataSource=:memory:");
        var context = new DialogDbContext(builder.Options);
        var isMissingMigrations = IsMissingMigrations(context);
        Assert.False(isMissingMigrations, "DialogDbContexts snapshot differs from its entities. Are you missing a migration?");
    }

    private static bool IsMissingMigrations(DbContext context)
    {
        var snapshotModel = context
            .GetService<IMigrationsAssembly>()
            .ModelSnapshot?
            .Model;

        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }

        if (snapshotModel != null)
        {
            snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(snapshotModel);
        }

        var hasDifferences = context.GetService<IMigrationsModelDiffer>().HasDifferences(
            snapshotModel?.GetRelationalModel(),
            context.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        return hasDifferences;
    }
}
