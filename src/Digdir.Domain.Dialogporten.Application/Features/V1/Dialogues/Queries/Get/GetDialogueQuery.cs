using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;

public class GetDialogueQuery : IRequest<GetDialogueDto>
{
    public Guid Id { get; set; }
}

internal sealed class GetDialogueQueryHandler : IRequestHandler<GetDialogueQuery, GetDialogueDto>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;

    public GetDialogueQueryHandler(IDialogueDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GetDialogueDto> Handle(GetDialogueQuery request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .AsNoTracking()
            .ProjectTo<GetDialogueDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id== request.Id);

        if (dialogue is null)
        {
            // TODO: Handle with specific exception OR result object
            throw new Exception($"Dialogue with id {request.Id} not found.");
        }

        return dialogue;
    }
}
