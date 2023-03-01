using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using MediatR;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

public sealed class CreateDialogueCommand : CreateDialogueDto, IRequest<OneOf<Guid, ValidationError>> { }

internal sealed class CreateDialogueCommandHandler : IRequestHandler<CreateDialogueCommand, OneOf<Guid, ValidationError>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDialogueCommandHandler(IDialogueDbContext db, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<OneOf<Guid, ValidationError>> Handle(CreateDialogueCommand request, CancellationToken cancellationToken)
    {
        var dialogue = _mapper.Map<DialogueEntity>(request);
        await _db.Dialogues.AddAsync(dialogue, cancellationToken);
        // TODO: Publish event
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dialogue.Id;
    }
}
