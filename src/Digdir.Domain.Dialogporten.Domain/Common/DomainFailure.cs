namespace Digdir.Domain.Dialogporten.Domain.Common;

public class DomainFailure 
{
    public string PropertyName { get; init; }
    public string ErrorMessage { get; init; }

    public DomainFailure(string propertyName, string error)
    {
        PropertyName = propertyName;
        ErrorMessage = error;
    }
}
