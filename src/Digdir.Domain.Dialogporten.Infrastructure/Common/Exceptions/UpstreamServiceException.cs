namespace Digdir.Domain.Dialogporten.Infrastructure.Common.Exceptions;

public interface IUpstreamServiceError;
public class UpstreamServiceException(Exception innerException)
    : Exception(innerException.Message, innerException), IUpstreamServiceError;
