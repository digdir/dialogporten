using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal class DomainEventToAltinnForwarderBase
{
    protected readonly ICloudEventBus CloudEventBus;
    protected readonly IDialogDbContext Db;
    private readonly IConfiguration _configuration;

    protected DomainEventToAltinnForwarderBase(ICloudEventBus cloudEventBus, IDialogDbContext db, IConfiguration configuration)
    {
        CloudEventBus = cloudEventBus ?? throw new ArgumentNullException(nameof(cloudEventBus));
        Db = db ?? throw new ArgumentNullException(nameof(db));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected string? DialogportenBaseUrl() => _configuration["WebApi:DialogPortenBaseUri"];

    protected async Task<DialogEntity> GetDialog(Guid dialogId, CancellationToken cancellationToken)
    {
        var dialog = await Db.Dialogs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dialogId, cancellationToken)
            ?? throw new KeyNotFoundException($"Dialog with id {dialogId} not found");

        return dialog;
    }
}
