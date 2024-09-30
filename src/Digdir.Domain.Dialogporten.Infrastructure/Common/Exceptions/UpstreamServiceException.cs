namespace Digdir.Domain.Dialogporten.Infrastructure.Common.Exceptions;

public interface IUpstreamServiceError;
internal sealed class UpstreamServiceException(Exception innerException)
    : Exception(innerException.Message, innerException), IUpstreamServiceError;
