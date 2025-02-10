using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Get;

public sealed class GetTransmissionQuery : IRequest<GetTransmissionResult>
{
    public Guid DialogId { get; set; }
    public Guid TransmissionId { get; set; }
}

[GenerateOneOf]
public sealed partial class GetTransmissionResult : OneOfBase<TransmissionDto, EntityNotFound, EntityDeleted>;

internal sealed class GetTransmissionQueryHandler : IRequestHandler<GetTransmissionQuery, GetTransmissionResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public GetTransmissionQueryHandler(IMapper mapper, IDialogDbContext dbContext, IAltinnAuthorization altinnAuthorization)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<GetTransmissionResult> Handle(GetTransmissionQuery request,
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
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Sender)
                .ThenInclude(x => x.ActorNameEntity)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken: cancellationToken);

        // If we cannot access the dialog at all, we don't allow access to any of the dialog transmissions.
        if (!authorizationResult.HasAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        var transmission = dialog.Transmissions.FirstOrDefault();
        if (transmission is null)
        {
            return new EntityNotFound<DialogTransmission>(request.TransmissionId);
        }

        var dto = _mapper.Map<TransmissionDto>(transmission);
        dto.IsAuthorized = authorizationResult.HasReadAccessToDialogTransmission(transmission.AuthorizationAttribute);

        if (dto.IsAuthorized) return dto;

        var urls = transmission.Attachments.SelectMany(a => a.Urls).ToList();
        foreach (var url in urls)
        {
            url.Url = Constants.UnauthorizedUri;
        }

        return dto;
    }
}
