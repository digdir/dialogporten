namespace Digdir.Domain.Dialogporten.Application.Features.V1.WellKnown.Jwks.Queries.Get;

public sealed class GetJwksDto
{
    public List<Jwk> Keys { get; set; } = [];
}

public sealed class Jwk
{
    public string Kty { get; set; } = "OKP";
    public string Use { get; set; } = "sig";
    public string Kid { get; set; } = null!;
    public string Crv { get; set; } = "Ed25519";
    public string X { get; set; } = null!;
    public string Alg { get; set; } = "EdDSA";
}
