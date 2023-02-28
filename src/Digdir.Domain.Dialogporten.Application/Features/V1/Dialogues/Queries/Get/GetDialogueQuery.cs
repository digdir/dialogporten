using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;

public class GetDialogueQuery : IRequest<OneOf<GetDialogueDto, NotFound>>
{
    public Guid Id { get; set; }
}

internal sealed class GetDialogueQueryHandler : IRequestHandler<GetDialogueQuery, OneOf<GetDialogueDto, NotFound>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;

    public GetDialogueQueryHandler(IDialogueDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OneOf<GetDialogueDto, NotFound>> Handle(GetDialogueQuery request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .AsNoTracking()
            .ProjectTo<GetDialogueDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialogue is null)
        {
            // TODO: Handle with specific exception OR result object
            //throw new Exception($"Dialogue with id {request.Id} not found.");
            return new NotFound();
        }

        return dialogue;
    }
}
