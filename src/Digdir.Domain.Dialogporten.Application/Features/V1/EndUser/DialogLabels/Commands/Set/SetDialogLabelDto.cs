namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;

public class SetDialogLabelDto
{
    public Guid DialogId { get; set; }
    public string Label { get; set; } = null!;
}
