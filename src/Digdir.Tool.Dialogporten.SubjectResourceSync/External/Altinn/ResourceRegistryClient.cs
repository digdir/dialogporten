namespace Digdir.Tool.Dialogporten.SubjectResourceSync.External.Altinn;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private readonly HttpClient _client;

    public ResourceRegistryClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task<List<UpdatedSubjectResourceDto>> GetUpdatedSubjectResources(DateTimeOffset since)
    {

    }
}
