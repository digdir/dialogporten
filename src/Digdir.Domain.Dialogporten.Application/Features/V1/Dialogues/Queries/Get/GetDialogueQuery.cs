using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;

public sealed class GetDialogueQuery : IRequest<OneOf<GetDialogueDto, EntityNotFound>>
{
    public Guid Id { get; set; }
}

internal sealed class GetDialogueQueryHandler : IRequestHandler<GetDialogueQuery, OneOf<GetDialogueDto, EntityNotFound>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;

    public GetDialogueQueryHandler(IDialogueDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OneOf<GetDialogueDto, EntityNotFound>> Handle(GetDialogueQuery request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .AsNoTracking()
            .ProjectTo<GetDialogueDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialogue is null)
        {
            return new EntityNotFound<DialogueEntity>(request.Id);
        }

        return dialogue;
    }
}
