namespace Digdir.Library.Dialogporten.WebApiClient.Interfaces;

public interface IIdentifiable
{
    Guid Id { get; }
    Guid RevisionId { get; }
}
