using Digdir.Domain.Dialogporten.Application.Common;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IServiceOwnerNameRegistry
{
    Task<string?> GetServiceOwnerShortName(string orgNumber, CancellationToken cancellationToken);
    Task<List<LocalizedName>> GetServiceOwnerLongNames(string orgNumber, CancellationToken cancellationToken);
}
