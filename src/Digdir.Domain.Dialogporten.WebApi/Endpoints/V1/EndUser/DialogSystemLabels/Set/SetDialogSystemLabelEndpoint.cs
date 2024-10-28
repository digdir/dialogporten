using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogSystemLabels.Set;

public sealed class SetDialogSystemLabelEndpoint(ISender sender) : Endpoint<DialogSystemLabelCommand>
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    public override void Configure()
    {
        Put("dialogs/{dialogId}/systemlabels");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(b => SetDialogSystemLabelSwaggerConfig.SetDescription(b, GetType()));
    }
    public override async Task HandleAsync(DialogSystemLabelCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            _ => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            concurrencyError => this.PreconditionFailed(ct));
    }
}
