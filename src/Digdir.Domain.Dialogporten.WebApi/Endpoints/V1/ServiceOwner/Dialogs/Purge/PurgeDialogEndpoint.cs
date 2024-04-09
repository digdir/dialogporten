using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Purge;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Purge;

public sealed class PurgeDialogEndpoint : Endpoint<PurgeDialogRequest>
{
    private readonly ISender _sender;

    public PurgeDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Post("dialogs/{dialogId}/actions/purge");
        RequestBinder(new PurgeDialogRequestBinder());
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => PurgeDialogSwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(PurgeDialogRequest req, CancellationToken ct)
    {
        var command = new PurgeDialogCommand { DialogId = req.DialogId, IfMatchDialogRevision = req.IfMatchDialogRevision };
        var result = await _sender.Send(command, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            concurrencyError => this.PreconditionFailed(ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}

public sealed class PurgeDialogRequest
{
    public Guid DialogId { get; init; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? IfMatchDialogRevision { get; init; }
}



// Custom request binder to avoid attempted automatic deserialization of the Request body if the content type is application/json
public class PurgeDialogRequestBinder : IRequestBinder<PurgeDialogRequest>
{
    public ValueTask<PurgeDialogRequest> BindAsync(BinderContext ctx, CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.HttpContext.Request.RouteValues["dialogId"]?.ToString()!, out var dialogId))
            return ValueTask.FromResult(new PurgeDialogRequest());

        ctx.HttpContext.Request.Headers.TryGetValue(Constants.IfMatch, out var revisionHeader);
        var revisionFound = Guid.TryParse(revisionHeader, out var revision);

        return ValueTask.FromResult(new PurgeDialogRequest
        {
            DialogId = dialogId,
            IfMatchDialogRevision = revisionFound ? revision : null
        });
    }
}
