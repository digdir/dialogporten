//using Digdir.Domain.Dialogporten.Domain.Common;
//using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
//using MediatR;

//namespace Digdir.Domain.Dialogporten.Application.Common.Behaviours;

//internal sealed class DeleteOutboxBehaviour<TResponse> : IPipelineBehavior<IDomainEvent, TResponse>
//{
//    private readonly DialogDbContext _db;

//    public DeleteOutboxBehaviour(DialogDbContext db)
//    {
//        _db = db;
//    }

//    public async Task<TResponse> Handle(IDomainEvent request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//    {
//        await next();

//        var outbox = await _db.OutboxMessages.FindAsync(request.EventId, cancellationToken);
//        if (outbox is null)
//        {
//            return;
//        }
//        _db.OutboxMessages.Remove(outbox);
//        await _db.SaveChangesAsync(cancellationToken);
//        // TODO: Delete outbox message
//    }
//}