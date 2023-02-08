using Digdir.Domain.Dialogporten.Domain;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Interfaces;

public interface IDialogueDbContext
{
    DbSet<DialogueEntity> Dialogues { get; }
}
