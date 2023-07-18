using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using MediatR;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;

public sealed class CreateDialogCommand : CreateDialogDto, IRequest<OneOf<Guid, DomainError, ValidationError>> { }

internal sealed class CreateDialogCommandHandler : IRequestHandler<CreateDialogCommand, OneOf<Guid, DomainError, ValidationError>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDomainContext _domainContext;

    public CreateDialogCommandHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher eventPublisher,
        IDomainContext domainContext)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
    }

    public async Task<OneOf<Guid, DomainError, ValidationError>> Handle(CreateDialogCommand request, CancellationToken cancellationToken)
    {
        var dialog = _mapper.Map<DialogEntity>(request);

        var existingDialogIds = await _db.GetExistingIds(new[] { dialog }, cancellationToken);
        if (existingDialogIds.Any())
        {
            _domainContext.AddError(DomainFailure.EntiryExists<DialogEntity>(existingDialogIds));
        }

        var existingActivityIds = await _db.GetExistingIds(dialog.Activities, cancellationToken);
        if (existingDialogIds.Any())
        {
            _domainContext.AddError(DomainFailure.EntiryExists<DialogActivity>(existingActivityIds));
        }

        var existingElementIds = await _db.GetExistingIds(dialog.Elements, cancellationToken);
        if (existingElementIds.Any())
        {
            _domainContext.AddError(DomainFailure.EntiryExists<DialogElement>(existingElementIds));
        }

        await _db.Dialogs.AddAsync(dialog, cancellationToken);
        _eventPublisher.Publish(new DialogCreatedDomainEvent(dialog.CreateId()));
        _eventPublisher.Publish(dialog.Activities.Select(x => new DialogActivityCreatedDomainEvent(dialog.Id, x.CreateId())));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dialog.Id;
    }
}
