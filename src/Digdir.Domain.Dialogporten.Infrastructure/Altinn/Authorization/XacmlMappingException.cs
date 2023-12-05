namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public class XacmlMappingException : Exception
{
    public XacmlMappingException(string message) : base(message) { }
    public XacmlMappingException(string message, Exception innerException) : base(message, innerException) { }
}
