using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Create;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabels.Set;

public sealed class SetDialogSystemLabelEndpoint(ISender sender) : Endpoint<SetDialogSystemLabelCommand>
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    public override void Configure()
    {
        // Spørsmål: Kan jeg lage parameter for {trash} samme som med dialogId?
        // 
        Post("dialogs/{dialogId}/actions/setsystemlabel/{label}");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        // Description(b => CreateDialogSwaggerConfig.SetDescription(b));
    }
    public override async Task HandleAsync(SetDialogSystemLabelCommand req, CancellationToken ct)
    {

        var result = await _sender.Send(req, ct);
        await result.Match(
            _ => SendOkAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            forbidden => this.ForbiddenAsync(forbidden, ct),
            deleted => this.GoneAsync(deleted, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            concurrencyError => this.PreconditionFailed(ct));
    }
}
