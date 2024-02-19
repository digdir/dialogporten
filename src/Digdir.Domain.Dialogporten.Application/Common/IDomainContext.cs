using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Common.Exceptions;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IDomainContext
{
    bool IsValid { get; }
    IReadOnlyCollection<DomainFailure> Errors { get; }
    IReadOnlyCollection<DomainFailure> Pop();
    void AddError(DomainFailure error);
    void AddError(string propertyName, string error);
    void AddErrors(IEnumerable<DomainFailure> errors);
    void EnsureValidState();
}

internal class DomainContext : IDomainContext
{
    private readonly List<DomainFailure> _errors = [];

    public IReadOnlyCollection<DomainFailure> Errors => _errors.ToList();

    public bool IsValid => _errors.Count == 0;

    public void AddError(string propertyName, string error)
        => AddError(new DomainFailure(propertyName, error));

    public void AddError(DomainFailure error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _errors.Add(error);
    }

    public void AddErrors(IEnumerable<DomainFailure> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        _errors.AddRange(errors);
    }

    public IReadOnlyCollection<DomainFailure> Pop()
    {
        var errors = _errors.ToList();
        _errors.Clear();
        return errors;
    }

    public void EnsureValidState()
    {
        if (!IsValid)
        {
            throw new DomainException(Pop());
        }
    }
}
