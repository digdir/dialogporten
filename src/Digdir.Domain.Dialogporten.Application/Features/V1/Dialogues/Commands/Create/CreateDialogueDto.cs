namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

public class CreateDialogueDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
}