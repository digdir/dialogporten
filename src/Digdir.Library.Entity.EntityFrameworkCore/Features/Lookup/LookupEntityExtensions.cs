using Digdir.Library.Entity.Abstractions.Features.Lookup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Lookup;

internal static class LookupEntityExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo?> LookupEntityMethodCache = new();

    public static ModelBuilder AddLookupEntities(this ModelBuilder modelBuilder)
    {
        var lookupTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.TryGetLookupValueMethodInfo(out _))
            .ToList();

        foreach (var lookupType in lookupTypes)
        {
            var lookupEntity = modelBuilder.Entity(lookupType.Name);
            var values = GetLookupEntityValues(lookupType.ClrType);
            lookupEntity.Property(nameof(MockLookupEntity.Id)).ValueGeneratedNever();
            lookupEntity.Property(nameof(MockLookupEntity.Name)).IsRequired();
            lookupEntity.HasData(values);

            foreach (var reference in lookupType.GetReferencingForeignKeys())
            {
                reference.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        return modelBuilder;
    }

    public static ChangeTracker HandleLookupEntities(this ChangeTracker changeTracker)
    {
        foreach (var entity in changeTracker.Entries<ILookupEntity>())
        {
            entity.State = EntityState.Unchanged;
        }

        return changeTracker;
    }

    private static IEnumerable<object> GetLookupEntityValues(Type type)
    {
        return type.TryGetLookupValueMethodInfo(out var method)
            ? (IEnumerable<object>)method.Invoke(null, null)!
            : Enumerable.Empty<object>();
    }

    private static bool TryGetLookupValueMethodInfo(this Type type, [NotNullWhen(true)] out MethodInfo? methodInfo)
    {
        methodInfo = LookupEntityMethodCache.GetOrAdd(type, GetLookupEntityMethod);
        return methodInfo is not null;
    }

    private static MethodInfo? GetLookupEntityMethod(Type type)
    {
        if (type.IsAbstract || type.IsInterface || !type.IsPublic || !type.IsClass)
        {
            return null;
        }

        if (!type.IsSubclassOfRawGeneric(typeof(AbstractLookupEntity<,>)))
        {
            return null;
        }

        return type.GetMethod(nameof(MockLookupEntity.GetValues), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    private static bool IsSubclassOfRawGeneric(this Type? toCheck, Type generic)
    {
        while (toCheck is not null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType
                ? toCheck.GetGenericTypeDefinition()
                : toCheck;

            if (generic == cur)
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    private abstract class MockLookupEntity : AbstractLookupEntity<MockLookupEntity, MockLookupEntity.Enum>
    {
        protected MockLookupEntity(Enum id, string name) : base(id, name)
        {
        }

        public enum Enum;
    }
}
