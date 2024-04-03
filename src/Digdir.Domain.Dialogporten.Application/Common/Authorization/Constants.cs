namespace Digdir.Domain.Dialogporten.Application.Common.Authorization;

public static class Constants
{
    public const string MainResource = "main";
    public const string ReadAction = "read";
    public const string ElementReadAction = "elementread";
    public static readonly Uri UnauthorizedUri = new("urn:dialogporten:unauthorized");
}
