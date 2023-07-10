namespace Digdir.Domain.Dialogporten.Domain.Common.Exceptions;

public class DomainException : ApplicationException
{
    private readonly IEnumerable<DomainFailure> _errors;

    /// <summary>
	/// Validation errors
	/// </summary>
	public IReadOnlyCollection<DomainFailure> Errors => _errors.ToList().AsReadOnly();

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message"></param>
    public DomainException(string message) : this(message, Enumerable.Empty<DomainFailure>())
    {

    }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errors"></param>
    public DomainException(string message, IEnumerable<DomainFailure> errors) : base(message)
    {
        _errors = errors;
    }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errors"></param>
    /// <param name="appendDefaultMessage">appends default validation error message to message</param>
    public DomainException(string message, IEnumerable<DomainFailure> errors, bool appendDefaultMessage)
        : base(appendDefaultMessage ? $"{message} {BuildErrorMessage(errors)}" : message)
    {
        _errors = errors;
    }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="errors"></param>
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
