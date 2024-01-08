//using BenchmarkDotNet.Running;
//using Digdir.Tool.Dialogporten.Benchmarks;

//BenchmarkRunner.Run<QueryableExtensionsBenchmark>();


using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Http;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;
using Digdir.Library.Entity.EntityFrameworkCore.Features.Aggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

var apiEndpointGuid = Guid.NewGuid();
Guid dialogId = default;

try
{
    dialogId = await CreateDialog(apiEndpointGuid);
    await EditApiEndpoint(apiEndpointGuid);
}
finally
{
    await DeleteDialog(dialogId);
}

//using var dbContext = new DialogDbContext(new DbContextOptionsBuilder<DialogDbContext>()
//    .UseNpgsql("Server=localhost;Port=5432;Database=Dialogporten;User ID=postgres;Password=postgres;")
//    .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
//    .Options);

//using var lala = new CancellationTokenSource();

////await dbContext.Database.MigrateAsync();

//var dialog = await dbContext.Dialogs
//    //.Include(x => x.Content.Where(x => x.TypeId == DialogContentType.Values.Title))
//    //.IgnoreAutoIncludes()
//    .FirstAsync(x => x.Content.Count > 1, lala.Token);

//foreach (var content in dialog.Content)
//{
//    Console.WriteLine(content.Id);
//}

//dialog.Content.First().UpdatedAt = DateTimeOffset.UtcNow;

//var entityEntry = (EntityEntry)dbContext.Entry(dialog);

//dbContext.Dialogs.Remove(dialog);

static async Task<Guid> CreateDialog(Guid apiEndpointId)
{
    using var dbContext = CreateDbContext();

    var dialog = new DialogEntity
    {
        ServiceResource = "foo:bar",
        Party = "Testing",
        StatusId = DialogStatus.Values.Unspecified,
        ApiActions =
        {
            new ()
            {
                Action = "Testing",
                Endpoints =
                {
                    new ()
                    {
                        Id = apiEndpointId,
                        HttpMethodId = HttpVerb.Values.GET,
                        Url = new("https://www.google.com")
                    }
                }
            }
        }
    };

    dbContext.Dialogs.Add(dialog);
    await dbContext.SaveChangesAsync();

    return dialog.Id;
}

static async Task EditApiEndpoint(Guid apiEndpointId)
{
    using var dbContext = CreateDbContext();

    var endpoint = await dbContext.DialogApiActionEndpoints
        .FirstAsync(x => x.Id == apiEndpointId);

    endpoint.Url = new("https://www.digdir.no");

    await dbContext.ChangeTracker.HandleAuditableEntities(DateTimeOffset.UtcNow);

    await dbContext.SaveChangesAsync();
}

static async Task DeleteDialog(Guid dialogId)
{
    using var dbContext = CreateDbContext();

    var dialog = await dbContext.Dialogs
        .IgnoreAutoIncludes()
        .FirstAsync(x => x.Id == dialogId);

    dbContext.Dialogs.Remove(dialog);

    await dbContext.SaveChangesAsync();
}

static DialogDbContext CreateDbContext()
{
    return new DialogDbContext(new DbContextOptionsBuilder<DialogDbContext>()
            .UseNpgsql("Server=localhost;Port=5432;Database=Dialogporten;User ID=postgres;Password=postgres;")
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .Options);
}
