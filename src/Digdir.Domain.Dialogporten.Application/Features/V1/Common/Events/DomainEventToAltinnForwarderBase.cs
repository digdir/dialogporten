using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal class DomainEventToAltinnForwarderBase
{
    protected readonly ICloudEventBus CloudEventBus;
    protected readonly IDialogDbContext Db;

    internal DomainEventToAltinnForwarderBase(ICloudEventBus cloudEventBus, IDialogDbContext db)
    {
        CloudEventBus = cloudEventBus ?? throw new ArgumentNullException(nameof(cloudEventBus));
        Db = db ?? throw new ArgumentNullException(nameof(db));
    }

    protected async Task<DialogEntity> GetDialog(Guid dialogId, CancellationToken cancellationToken)
    {
        var dialog = await Db.Dialogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dialogId, cancellationToken);

        if (dialog is null)
        {
            throw new ApplicationException($"Dialog with id {dialogId} not found");
        }

        return dialog;
    }
}