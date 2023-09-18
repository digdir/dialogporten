using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.List;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public class ListDialogEndpoint : Endpoint<ListDialogQuery>
{
    private readonly ISender _sender;
    private readonly IResourceRegistry _resourceRegistry;

    public ListDialogEndpoint(ISender sender, IResourceRegistry resourceRegistry)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _resourceRegistry = resourceRegistry;
    }

    public override void Configure()
    {
        Get("dialogs");
        // TODO: Sjekk om policies har `OR` eller `AND` relasjon
        Policies(Common.Authorization.Policy.ServiceproviderSearch);
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(ListDialogQuery req, CancellationToken ct)
    {
        // I et rent maskinporten token har vi verken urn:altinn:org eller urn:altinn:orgNumber
        // så vi må ta utgangspunkt i consumer.ID. 
        // 1. Hvordan henter vi ut ogranisasjonsnavnet for å lagre i DialogEntity.Org?
        // 2. Kunne vi lagret orgnummeret i consumer.ID i tillegg til orgnavnet for å slippe å slå opp i resource registry?

        var consumerClaim = User.Claims.First(x => x.Type == "urn:altinn:org").Value;
        //var consumerClaim = User.Claims.First(x => x.Type == "consumer").Value;
        //var lala = JsonConvert.DeserializeObject<ConsumerClaim>(consumerClaim);
        //var orgNumber = lala.Id.Split(":").Last();
        //var ownerOrg = await _resourceRegistry.GetOrgOwner("super-simple-service", ct);

        var result = await _sender.Send(req, ct);
        await result.Match(
            paginatedDto => SendOkAsync(paginatedDto, ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}

public class ConsumerClaim
{
    public string Id { get; set; }
}