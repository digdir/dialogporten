using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Janitor;

public class ConsoleUser : IUser
{
    public ClaimsPrincipal GetPrincipal() => throw new NotImplementedException();
}
