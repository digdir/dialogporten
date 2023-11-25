using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.List;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Common.Authorization;

internal interface IDialogSearchAuthorizationService
{
    public Task<AuthorizedResources> GetAuthorizedResources(ListDialogQuery request, IUserService userService, CancellationToken cancellationToken = default);
}

internal class MockDialogSearchAuthorizationService : IDialogSearchAuthorizationService
{

    public async Task<AuthorizedResources> GetAuthorizedResources(ListDialogQuery request, IUserService userService, CancellationToken cancellationToken = default)
    {
        // TODO
        // - Implement as per https://github.com/digdir/dialogporten/issues/249
        // - Note that either ServiceResource or Party is always supplied in the request.
        // - The user is also always authorized for its own dialogs, which might be an optimization


        var authorizedResources = new AuthorizedResources
        {
            ResourcesForParties = new Dictionary<string, List<string>>
            {
                ["/org/991825827"] = new List<string> { "urn:altinn:resource:super-simple-service" },
                ["/person/07874299582"] = new List<string> { "urn:altinn:resource:super-simple-service2", "urn:altinn:resource:ttd-altinn-events-automated-tests" },
            },
            DialogIds = new List<Guid>() { Guid.Parse("0ab48b01-74d4-3770-b91d-79f99fb16a5a"), Guid.Parse("0ab48b01-bbca-8873-a3fe-518ce47532ce") }

        };

        return authorizedResources;

    }
}

internal sealed class AuthorizedResources
{
    public Dictionary<string, List<string>> ResourcesForParties { get; set; } = new();
    public Dictionary<string, List<string>> PartiesForResources { get; set; } = new();
    public List<Guid> DialogIds { get; set; } = new();
}
