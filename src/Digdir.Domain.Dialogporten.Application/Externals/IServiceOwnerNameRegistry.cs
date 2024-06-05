namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IServiceOwnerNameRegistry
{
    Task<ServiceOwnerInfo?> GetServiceOwnerInfo(string orgNumber, CancellationToken cancellationToken);
}

public sealed class ServiceOwnerLongName
{
    public required string LongName { get; init; }
    public required string Language { get; init; }
}

public sealed class ServiceOwnerInfo
{
    public required string OrgNumber { get; init; }
    public required string ShortName { get; init; }
    public required IList<ServiceOwnerLongName> LongNames { get; init; }
}
