using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;

public class SystemLabelDto
{
    public Guid DialogId { get; set; }
    public SystemLabel.Values Label { get; set; }
}
