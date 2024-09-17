using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Janitor;

public sealed class ConsoleUser : IUser
{
    public ClaimsPrincipal GetPrincipal()
        => throw new NotImplementedException(
            "Claims for the console user has not been implemented. Consider " +
            "implementing ConsoleUser.GetPrincipal() if this is needed.");
}
