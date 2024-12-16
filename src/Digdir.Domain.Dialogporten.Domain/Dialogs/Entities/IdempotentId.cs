namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class IdempotentId(string org, string idempotent)
{
    public string Org { set; get; } = org;
    public string Idempotent { set; get; } = idempotent;
}
