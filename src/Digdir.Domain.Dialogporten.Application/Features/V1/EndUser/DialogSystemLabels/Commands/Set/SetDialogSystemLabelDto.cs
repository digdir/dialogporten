using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;

public class SetDialogSystemLabelDto
{
    public Guid DialogId { get; set; }
    public SystemLabel.Values Label { get; set; }
}
