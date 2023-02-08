using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class ContextDesignTimeFactory : IDesignTimeDbContextFactory<DialogueDbContext>
{
    private const string LocalDbConnectionString = 
        @"Server=(localdb)\MSSQLLocalDB;Database=digdir-dialogportalen;Trusted_Connection=True;MultipleActiveResultSets=true;";

    public DialogueDbContext CreateDbContext(string[] args) => 
        new (new DbContextOptionsBuilder<DialogueDbContext>()
            .UseSqlServer(LocalDbConnectionString)
            .Options);
}
