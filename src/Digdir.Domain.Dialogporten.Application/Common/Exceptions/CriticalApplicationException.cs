using System.Runtime.Serialization;

namespace Digdir.Domain.Dialogporten.Application.Common.Exceptions;

public sealed class CriticalApplicationException : ApplicationException
{
    public CriticalApplicationException()
    {
    }

    public CriticalApplicationException(string? message) : base(message)
    {
    }

    public CriticalApplicationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CriticalApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
