namespace Digdir.Tool.Dialogporten.SubjectResourceSync.External.Altinn;

interface IResourceRegistry 
{
    public Task<List<UpdatedSubjectResourceDto>> GetUpdatedSubjectResources(DateTimeOffset since);
}