using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabels.Set;

public sealed class SetDialogLabelEndpoint(ISender sender) : Endpoint<SetDialogLabelCommand>
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    public override void Configure()
    {
        // Spørsmål: Hvordan ha namespaced label i dto men ha ikke namespaced enum
        Post("dialogs/{dialogId}/labels");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(b => SetDialogLabelSwaggerConfig.SetDescription(b));
    }
    public override async Task HandleAsync(SetDialogLabelCommand req, CancellationToken ct)
    {

        var result = await _sender.Send(req, ct);
        await result.Match(
            _ => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            forbidden => this.ForbiddenAsync(forbidden, ct),
            deleted => this.GoneAsync(deleted, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            concurrencyError => this.PreconditionFailed(ct));
    }
}
