using Digdir.Library.Entity.Abstractions.Features.Lookup;
// ReSharper disable InconsistentNaming

namespace Digdir.Domain.Dialogporten.Domain.Http;

public class HttpVerb : AbstractLookupEntity<HttpVerb, HttpVerb.Values>
{
    public HttpVerb(Values id) : base(id) { }
    public override HttpVerb MapValue(Values id) => new(id);

    public enum Values
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
