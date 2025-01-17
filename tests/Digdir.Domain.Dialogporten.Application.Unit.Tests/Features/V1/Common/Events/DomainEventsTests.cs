using System.Reflection;
using Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.Common.Events;

public class DomainEventTests
{
    // [Fact]
    // // This test ensures that all Altinn forwarders check for the event opt-out flag.
    // public void All_Altinn_Forwarders_Must_Check_For_Event_Opt_Out()
    // {
    //     // Arrange
    //     var altinnForwarderAssembly = Assembly.GetAssembly(
    //         typeof(Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events.AltinnForwarders.
    //             DomainEventToAltinnForwarderBase));
    //
    //     var altinnHandlers = altinnForwarderAssembly
    //         ?.GetTypes()
    //         .Where(t => t.GetInterfaces().Any(i =>
    //             i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
    //         .ToList();
    //
    //     // Act & Assert
    //     foreach (var handler in altinnHandlers!)
    //     {
    //         var handleMethods = handler
    //             .GetMethods()
    //             .Where(m => m.Name == "Handle" && m.GetParameters().Any(p => typeof(IDomainEvent).IsAssignableFrom(p.ParameterType)));
    //
    //         foreach (var method in handleMethods)
    //         {
    //             var domainEventType = method
    //                 .GetParameters()
    //                 .FirstOrDefault(p => typeof(IDomainEvent).IsAssignableFrom(p.ParameterType))?.ParameterType;
    //
    //             // Find a suitable constructor
    //             var constructor = domainEventType!.GetConstructors().FirstOrDefault();
    //             var constructorParameters = constructor?.GetParameters();
    //             var parameterValues = constructorParameters?.Select(p => GetDefaultValue(p.ParameterType)).ToArray();
    //
    //             var domainEventInstance = constructor?.Invoke(parameterValues);
    //             var metadataProperty = domainEventType.GetProperty(nameof(IDomainEvent.Metadata));
    //
    //             // Set the metadata property to disable Altinn events
    //             metadataProperty?.SetValue(domainEventInstance,
    //                 new Dictionary<string, string> { { Constants.DisableAltinnEvents, bool.TrueString } });
    //
    //             var handlerInstance = Activator.CreateInstance(handler);
    //             // var cancellationToken = CancellationToken.None;
    //             //
    //             // var shouldNotBeSentToAltinnMethod = domainEventType.GetMethod("ShouldNotBeSentToAltinn");
    //             // var shouldNotBeSentToAltinnResult =
    //             //     (bool)shouldNotBeSentToAltinnMethod?.Invoke(domainEventInstance, null)!;
    //             //
    //             // Assert.True(shouldNotBeSentToAltinnResult,
    //             //     $"The method {method.Name} in {handler.Name} does not check ShouldNotBeSentToAltinn.");
    //         }
    //     }
    // }
    //
    // private static object? GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    //
    //
    // [Fact]
    // public void All_EventHandlers_Should_Call_SpecificMethod()
    // {
    //     // Arrange
    //     var altinnForwarderAssembly = Assembly.GetAssembly(
    //         typeof(Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events.AltinnForwarders.
    //             DomainEventToAltinnForwarderBase));
    //
    //     var altinnHandlers = altinnForwarderAssembly
    //         ?.GetTypes()
    //         .Where(t => t.GetInterfaces().Any(i =>
    //             i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
    //         .ToList();
    //
    //     foreach (var handlerType in altinnHandlers!)
    //     {
    //         var handleMethods = handlerType
    //             .GetMethods()
    //             .Where(m => m.Name == "Handle" && m.GetParameters().Any(p => typeof(IDomainEvent).IsAssignableFrom(p.ParameterType)))
    //             .ToList();
    //
    //         foreach (var method in handleMethods)
    //         {
    //             var methodBody = method.GetMethodBody();
    //             var callsSpecificMethod = methodBody?.GetILAsByteArray()!
    //             .Any(il => il == OpCodes.Call.Value || il == OpCodes.Callvirt.Value) ?? false;
    //
    //             // callsSpecificMethod.Should().BeTrue($"The method {method.Name} in {handlerType.Name} does not call the required method.");
    //         }
    //     }
    // }


    [Fact]
    public void All_EventHandlers_Should_Call_SpecificMethod2()
    {
        // Arrange
        var altinnForwarderAssembly = Assembly.GetAssembly(
            typeof(Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events.AltinnForwarders.
                DomainEventToAltinnForwarderBase));

        var assembly = AssemblyDefinition.ReadAssembly(altinnForwarderAssembly!.Location);
        var method = assembly.MainModule.Types
            .First(t => t.Name == "TestClass")
            .Methods.First(m => m.Name == "MethodToAnalyze");

        // foreach (var instruction in method.Body.Instructions)
        // {
        //     if (instruction.OpCode.Code == Mono.Cecil.Cil.Code.Call ||
        //         instruction.OpCode.Code == Mono.Cecil.Cil.Code.Callvirt)
        //     {
        //         Console.WriteLine($"Method call: {instruction.Operand}");
        //     }
        // }
        // var altinnHandlers = altinnForwarderAssembly
        //     ?.GetTypes()
        //     .Where(t => t.GetInterfaces().Any(i =>
        //         i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
        //     .ToList();
        //
        // var requiredMethod = typeof(DomainEventExtensions)
        //     .GetMethod(nameof(DomainEventExtensions.ShouldNotBeSentToAltinn));
        //
        // foreach (var handlerType in altinnHandlers!)
        // {
        //     var handleMethods = handlerType
        //         .GetMethods()
        //         .Where(m => m.Name == "Handle" &&
        //                     m.GetParameters().Any(p => typeof(IDomainEvent).IsAssignableFrom(p.ParameterType)))
        //         .ToList();
        //
        //     foreach (var method in handleMethods)
        //     {
        //         var methodBody = method.GetMethodBody();
        //         var ilBytes = methodBody?.GetILAsByteArray();
        //         var modCtx = ModuleDef.CreateModuleContext();
        //         var module = ModuleDefMD.Load(ilBytes, modCtx);
        //         Console.WriteLine(module);
        //
        //         var requiredMethodIsCalled = ilBytes != null && ilBytes
        //             .Select((b, i) => new { Byte = b, Index = i })
        //             .Any(x => x.Byte == OpCodes.Callvirt.Value &&
        //                       BitConverter.ToInt32(ilBytes, x.Index + 1) == requiredMethod!.MetadataToken);
        //
        //         requiredMethodIsCalled.Should().BeTrue(
        //             $"The method {method.Name} in {handlerType.Name} does " +
        //             $"not call {nameof(DomainEventExtensions.ShouldNotBeSentToAltinn)}.");
        //     }
        // }
    }

    [Fact]
    public void All_EventHandlers_Should_Call_SpecificMethod()
    {
        // Arrange
        var altinnForwarderAssembly = Assembly.GetAssembly(
            typeof(Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events.AltinnForwarders.
                DomainEventToAltinnForwarderBase));

        var assembly = AssemblyDefinition.ReadAssembly(altinnForwarderAssembly!.Location);
        var domainEventExtensionsType = typeof(DomainEventExtensions);
        var shouldNotBeSentToAltinnMethod = domainEventExtensionsType.GetMethod("ShouldNotBeSentToAltinn");

        var altinnHandlers = assembly.MainModule.Types
            .Where(t => t.Namespace ==
                        "Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events.AltinnForwarders" &&
                        t.Interfaces.Any(i =>
                            i.InterfaceType.Name.StartsWith("INotificationHandler", StringComparison.Ordinal)))
            .ToList();

        foreach (var handlerType in altinnHandlers)
        {
            var handleMethods = handlerType.Methods
                .Where(m => m.Name == "Handle" &&
                            m.Parameters.Any(p =>
                                p.ParameterType.Name.EndsWith("IDomainEvent", StringComparison.Ordinal)))
                .ToList();

            foreach (var method in handleMethods)
            {
                var callsSpecificMethod = method.Body.Instructions
                    .Any(instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).FullName ==
                        shouldNotBeSentToAltinnMethod!.Name);

                Assert.True(callsSpecificMethod,
                    $"The method {method.Name} in {handlerType.Name} does not call ShouldNotBeSentToAltinn.");
            }
        }
    }
}
