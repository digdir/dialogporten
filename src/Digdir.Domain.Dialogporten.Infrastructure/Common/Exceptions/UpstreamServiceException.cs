namespace Digdir.Domain.Dialogporten.Infrastructure.Common.Exceptions;

public interface IUpstreamServiceError;
internal sealed class UpstreamServiceException : Exception, IUpstreamServiceError
{
    public UpstreamServiceException(Exception innerException) : base(innerException.Message, innerException) { }
    public UpstreamServiceException(string message) : base(message) { }
}
