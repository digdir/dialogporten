using System.Text.Json.Serialization;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.WellKnown.OauthAuthorizationServer.Queries.Get;

public sealed class GetOauthAuthorizationServerDto
{
    public string Issuer { get; set; } = null!;

    [JsonPropertyName("jwks_uri")]
    public string JwksUri { get; set; } = null!;
}
