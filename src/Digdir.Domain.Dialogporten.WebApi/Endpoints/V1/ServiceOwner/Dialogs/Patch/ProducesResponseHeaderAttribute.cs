namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Patch;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ProducesResponseHeaderAttribute : Attribute
{
    public ProducesResponseHeaderAttribute(int statusCode, string headerName, string description)
    {
        HeaderName = headerName;
        StatusCode = statusCode;
        Description = description;
    }

    public string HeaderName { get; }
    public int StatusCode { get; }
    public string Description { get; }
}
