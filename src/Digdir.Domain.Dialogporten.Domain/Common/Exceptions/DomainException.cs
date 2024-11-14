namespace Digdir.Domain.Dialogporten.Domain.Common.Exceptions;

public sealed class DomainException : ApplicationException
{
    private readonly IEnumerable<DomainFailure> _errors;

    public IReadOnlyCollection<DomainFailure> Errors => _errors.ToList().AsReadOnly();

    public DomainException(string message) : this(message, []) { }

    public DomainException(string message, IEnumerable<DomainFailure> errors) : base(message)
    {
        _errors = errors;
    }

    public DomainException(string message, IEnumerable<DomainFailure> errors, bool appendDefaultMessage)
        : base(appendDefaultMessage ? $"{message} {BuildErrorMessage(errors)}" : message)
    {
        _errors = errors;
    }

    public DomainException(IEnumerable<DomainFailure> errors) : base(BuildErrorMessage(errors))
    {
        _errors = errors;
    }

    private static string BuildErrorMessage(IEnumerable<DomainFailure> errors)
    {
        var arr = errors.Select(x => $"{Environment.NewLine} -- {x.PropertyName}: {x.ErrorMessage}");
        return "Domain rule broken: " + string.Join(string.Empty, arr);
    }
}
