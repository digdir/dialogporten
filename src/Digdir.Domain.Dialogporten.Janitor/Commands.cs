using Cocona;
using Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;
using MediatR;

namespace Digdir.Domain.Dialogporten.Janitor;

internal static class Commands
{
    internal static CoconaApp AddJanitorCommands(this CoconaApp app)
    {
        app.AddCommand("UpdateSubjectResources", (
                [FromService] CoconaAppContext ctx,
                [FromService] ISender application,
                [Argument] SynchronizeResourceRegistryCommand command)
            => application.Send(command, ctx.CancellationToken));

        // app.AddCommand("test", ([Argument] MahInputs inputs) => Console.WriteLine("Hello, World!"));

        return app;
    }
}
