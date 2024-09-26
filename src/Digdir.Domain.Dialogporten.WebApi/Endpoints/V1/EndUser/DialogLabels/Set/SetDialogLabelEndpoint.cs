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
        Post("dialogs/{dialogId}/labels");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();
        // Amund: Vil dette funke på alle samme? deler alle samme rate limit?
        // Amund: Ser ut som dette er en enkel ting å lure seg unna med minimal innsats, vil bare stoppe vanlige personer fra å bytte tag veldig fort.
        // Amund: Om hensikt er for å stoppe db skriving burdce jeg heller ta en manuel sjekk på db call innen en tid og ikke skrive på hyppig bruk?
        // men burde ikke ratelimit være her av DDD grunner?
        // Men kan ha egen return type for ratelimit (429)? så den kan fint leve i commanden og den blir ikke å direkte bryte ddd?
        // trenger da en billig måte å sjekke hvor mange updates det har vært innen en tidsperiode
        Options(x => x.RequireRateLimiting("limiterPolicy"));

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
