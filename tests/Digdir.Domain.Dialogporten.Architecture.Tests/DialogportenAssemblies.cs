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

namespace Digdir.Domain.Dialogporten.Architecture.Tests;

public static class DialogportenAssemblies
{
    private static readonly Assembly ApplicationAssembly = typeof(GetDialogDto).Assembly;
    private static readonly Assembly CdcAssembly = typeof(ICdcSink<>).Assembly;
    private static readonly Assembly DomainAssembly = typeof(DialogEntity).Assembly;
    private static readonly Assembly GraphQlAssembly = typeof(ApplicationInsightEventListener).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(IUnitOfWork).Assembly;
    private static readonly Assembly JanitorAssembly = typeof(ConsoleUser).Assembly;
    private static readonly Assembly ServiceAssembly = typeof(OutboxMessageConsumer).Assembly;
    private static readonly Assembly WebApiAssembly = typeof(MetadataGroup).Assembly;
    private static readonly Assembly EntityAbstractionsAssembly = typeof(IEntity).Assembly;

    internal static readonly List<Assembly> All =
    [
        ApplicationAssembly,
        CdcAssembly,
        DomainAssembly,
        GraphQlAssembly,
        InfrastructureAssembly,
        JanitorAssembly,
        ServiceAssembly,
        WebApiAssembly,
        EntityAbstractionsAssembly
    ];
}
