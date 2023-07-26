namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerable;

internal readonly struct UpdateSet<TDestination, TSource> 
{
    public TDestination Destination { get; init; }
    public TSource Source { get; init; }

    public UpdateSet(TDestination destination, TSource source)
    {
        Destination = destination;
        Source = source;
    }

    public void Deconstruct(out TSource source, out TDestination destination)
    {
        source = Source;
        destination = Destination;
    }
}
