using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Queries.Set;

public sealed class SetDialogSystemLabelQuery : IRequest<SetDialogLabelResult>
{
    // public Guid DialogId { get; set; }
    // public SystemLabel.Values LabelName { get; set; }
}
