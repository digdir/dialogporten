using Medo;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests;

public static class UuiDv7Utils
{
    public static Guid GenerateBigEndianUuidV7(DateTimeOffset? timeStamp = null) => timeStamp is null ?
        Uuid7.NewUuid7().ToGuid()
        : Uuid7.NewUuid7(timeStamp.Value).ToGuid();
}
