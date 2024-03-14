using MediatR;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Metadata.WellKnown.OauthAuthorizationServer.Queries.Get;

public sealed class GetOauthAuthorizationServerQuery : IRequest<GetOauthAuthorizationServerDto>;

internal sealed class GetOauthAuthorizationServerQueryHandler : IRequestHandler<GetOauthAuthorizationServerQuery, GetOauthAuthorizationServerDto>
{
    private readonly ApplicationSettings _applicationSettings;

    public GetOauthAuthorizationServerQueryHandler(
        IOptions<ApplicationSettings> applicationSettings)
    {
        _applicationSettings = applicationSettings.Value;
    }

    public async Task<GetOauthAuthorizationServerDto> Handle(GetOauthAuthorizationServerQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new GetOauthAuthorizationServerDto
        {
            Issuer = _applicationSettings.Dialogporten.BaseUri.ToString(),
            // FIXME! Don't hardcode the jwks_uri
            JwksUri = _applicationSettings.Dialogporten.BaseUri + "api/v1/metadata/.well-known/jwks.json"
        });
    }
}
