using System.Reflection;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.ChangeDataCapture;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.GraphQL;
using Digdir.Domain.Dialogporten.Janitor;
using Digdir.Domain.Dialogporten.Service.Consumers.OutboxMessages;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Architecture.Tests;

public static class DialogportenAssemblies
{
    private static readonly Assembly Application = typeof(GetDialogDto).Assembly;
    private static readonly Assembly Cdc = typeof(ICdcSink<>).Assembly;
    private static readonly Assembly Domain = typeof(DialogEntity).Assembly;
    private static readonly Assembly GraphQl = typeof(ApplicationInsightEventListener).Assembly;
    private static readonly Assembly Infrastructure = typeof(IUnitOfWork).Assembly;
    private static readonly Assembly Janitor = typeof(ConsoleUser).Assembly;
    private static readonly Assembly Service = typeof(OutboxMessageConsumer).Assembly;
    private static readonly Assembly WebApi = typeof(MetadataGroup).Assembly;
    private static readonly Assembly EntityAbstractions = typeof(IEntity).Assembly;
    private static readonly Assembly EntityFrameworkCore = typeof(EntityLibraryEfCoreExtensions).Assembly;

    internal static readonly List<Assembly> All =
    [
        Application,
        Cdc,
        Domain,
        GraphQl,
        Infrastructure,
        Janitor,
        Service,
        WebApi,
        EntityAbstractions,
        EntityFrameworkCore
    ];
}
