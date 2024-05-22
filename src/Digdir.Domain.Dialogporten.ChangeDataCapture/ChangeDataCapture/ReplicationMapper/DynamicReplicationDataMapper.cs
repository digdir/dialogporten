using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.ReplicationMapper;

internal sealed class DynamicReplicationDataMapper<T> : IReplicationDataMapper<T>
    where T : class
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfoConverter>> _propInfoConvertersByType = new();

    public async Task<T> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct)
    {
        var type = typeof(T);
        var result = (T)Activator.CreateInstance(type)!;
        var infoConverterByPropName = _propInfoConvertersByType.GetOrAdd(type, GeneratePropInfoConvertionsByPropName);
        var columnNumber = 0;
        await foreach (var value in insertMessage.NewRow)
        {
            var propName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            var propValue = await value.Get<string?>(ct);
            SetValue(infoConverterByPropName, result, propName, propValue);
        }

        return result;
    }

    public Task<T> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct)
    {
        var type = typeof(T);
        var result = (T)Activator.CreateInstance(type)!;
        var infoConverterByPropName = _propInfoConvertersByType.GetOrAdd(type, GeneratePropInfoConvertionsByPropName);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var propName = reader.GetName(i);
            var propValue = reader.GetValue(i)?.ToString();
            SetValue(infoConverterByPropName, result, propName, propValue);
        }

        return Task.FromResult(result);
    }


    private static void SetValue(Dictionary<string, PropertyInfoConverter> infoConverterByPropName, T value, string propName, string? propValue)
    {
        if (!infoConverterByPropName.TryGetValue(propName, out var propInfoConverter))
            throw new InvalidOperationException($"Property {propName} not found on type {typeof(T).Name}.");

        propInfoConverter.ConvertAndSetValue(value, propValue);
    }

    private static Dictionary<string, PropertyInfoConverter> GeneratePropInfoConvertionsByPropName(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new PropertyInfoConverter(x))
            .ToDictionary(x => x.PropertyName);

    private record PropertyInfoConverter
    {
        private static readonly NullableStringConverter _nullableStringConverter = new();
        private readonly PropertyInfo _info;
        private readonly TypeConverter _converter;

        public string PropertyName => _info.Name;

        public PropertyInfoConverter(PropertyInfo info)
        {
            _info = info;
            _converter = info.PropertyType == typeof(string)
                ? _nullableStringConverter
                : TypeDescriptor.GetConverter(info.PropertyType);
        }

        public void ConvertAndSetValue(object obj, string? value) => _info.SetValue(obj, _converter.ConvertFromInvariantString(value!));

        private class NullableStringConverter : StringConverter
        {
            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
                value is not null
                    ? base.ConvertFrom(context, culture, value)
                    : null;
        }
    }
}
