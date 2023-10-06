using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal class DomainEventToAltinnForwarderBase
{
    protected readonly ICloudEventBus CloudEventBus;
    protected readonly IDialogDbContext Db;
    private readonly DialogportenSettings _dialogportenSettings;

    protected DomainEventToAltinnForwarderBase(ICloudEventBus cloudEventBus, IDialogDbContext db, IOptions<ApplicationSettings> settings)
    {
        CloudEventBus = cloudEventBus ?? throw new ArgumentNullException(nameof(cloudEventBus));
        Db = db ?? throw new ArgumentNullException(nameof(db));
        _dialogportenSettings = settings.Value.Dialogporten ?? throw new ArgumentNullException(nameof(settings));
    }

    protected string DialogportenBaseUrl() => _dialogportenSettings.BaseUri.ToString();
    
    protected async Task<DialogEntity> GetDialog(Guid dialogId, CancellationToken cancellationToken)
    {
        var dialog = await Db.Dialogs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dialogId, cancellationToken);

        if (dialog is null)
        {
            throw new ApplicationException($"Dialog with id {dialogId} not found");
        }

        return dialog;
    }
}