using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Get;

public sealed class GetDialogTransmissionQuery : IRequest<GetDialogTransmissionResult>
{
    public Guid DialogId { get; set; }
    public Guid TransmissionId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogTransmissionResult : OneOfBase<GetDialogTransmissionDto, EntityNotFound, EntityDeleted>;

internal sealed class GetDialogTransmissionQueryHandler : IRequestHandler<GetDialogTransmissionQuery, GetDialogTransmissionResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;
    private readonly IUserResourceRegistry _userResourceRegistry;

    public GetDialogTransmissionQueryHandler(IMapper mapper, IDialogDbContext dbContext, IUserResourceRegistry userResourceRegistry)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    }

    public async Task<GetDialogTransmissionResult> Handle(GetDialogTransmissionQuery request,
        CancellationToken cancellationToken)
    {
        var dialog = await _dbContext.Dialogs
            .Include(x => x.Transmissions.Where(x => x.Id == request.TransmissionId))
                .ThenInclude(x => x.Content)
                .ThenInclude(x => x.Value.Localizations)
            .Include(x => x.Transmissions.Where(x => x.Id == request.TransmissionId))
                .ThenInclude(x => x.Attachments)
                .ThenInclude(x => x.DisplayName!.Localizations)
            .Include(x => x.Transmissions.Where(x => x.Id == request.TransmissionId))
                .ThenInclude(x => x.Attachments.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        var transmission = dialog.Transmissions.FirstOrDefault();

        // TODO: Check auth
        return transmission is null
            ? (GetDialogTransmissionResult)new EntityNotFound<DialogTransmission>(request.TransmissionId)
            : _mapper.Map<GetDialogTransmissionDto>(transmission);
    }
}
