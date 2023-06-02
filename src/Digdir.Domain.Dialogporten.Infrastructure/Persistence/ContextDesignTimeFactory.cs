using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class ContextDesignTimeFactory : IDesignTimeDbContextFactory<DialogueDbContext>
{
    private const string ConnectionStringConfigName = "Infrastructure:DialogueDbConnectionString";

    public DialogueDbContext CreateDbContext(string[] args)
    {
        var localPostgresConnectionString = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build()[ConnectionStringConfigName];
        return new(new DbContextOptionsBuilder<DialogueDbContext>()
            .UseNpgsql(localPostgresConnectionString)
            .Options);
    }
}
