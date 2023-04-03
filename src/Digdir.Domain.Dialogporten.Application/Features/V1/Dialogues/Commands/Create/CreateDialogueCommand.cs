using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

public sealed class CreateDialogueCommand : CreateDialogueDto, IRequest<OneOf<Guid, EntityExists, ValidationError>> { }

internal sealed class CreateDialogueCommandHandler : IRequestHandler<CreateDialogueCommand, OneOf<Guid, EntityExists, ValidationError>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _eventPublisher;

    public CreateDialogueCommandHandler(IDialogueDbContext db, IMapper mapper, IUnitOfWork unitOfWork, IDomainEventPublisher eventPublisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<OneOf<Guid, EntityExists, ValidationError>> Handle(CreateDialogueCommand request, CancellationToken cancellationToken)
    {
        if (await IsExistingDialogueId(request.Id, cancellationToken))
        {
            return new EntityExists<DialogueEntity>(request.Id!.Value);
        }

        var dialogue = _mapper.Map<DialogueEntity>(request);
        dialogue.CreateId();
        await _db.Dialogues.AddAsync(dialogue, cancellationToken);
        _eventPublisher.Publish(new DialogueCreatedDomainEvent(dialogue.Id));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dialogue.Id;
    }

    private async Task<bool> IsExistingDialogueId(Guid? id, CancellationToken cancellationToken) => 
        id.HasValue && await _db.Dialogues.AnyAsync(x => x.Id == id, cancellationToken);
}
