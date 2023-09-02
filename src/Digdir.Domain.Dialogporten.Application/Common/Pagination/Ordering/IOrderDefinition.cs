namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public interface IOrderDefinition<TTarget>
{
    public static abstract IOrderOptions<TTarget> Configure(IOrderOptionsBuilder<TTarget> options);
}
