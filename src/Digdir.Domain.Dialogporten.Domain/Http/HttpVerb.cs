using Digdir.Library.Entity.Abstractions.Features.Lookup;
// ReSharper disable InconsistentNaming

namespace Digdir.Domain.Dialogporten.Domain.Http;

public class HttpVerb : AbstractLookupEntity<HttpVerb, HttpVerb.Enum>
{
    public HttpVerb(Enum id) : base(id) { }
    public override HttpVerb MapValue(Enum id) => new(id);

    public enum Enum
    {
        GET = 1,
        POST = 2,
        PUT = 3,
        PATCH = 4,
        DELETE = 5,
        HEAD = 6,
        OPTIONS = 7,
        TRACE = 8,
        CONNECT = 9
    }
}
