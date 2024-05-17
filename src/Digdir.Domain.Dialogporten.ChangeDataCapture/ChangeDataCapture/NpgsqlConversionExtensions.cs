using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

internal static class NpgsqlConversionExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfoConverter>> _propInfoConvertersByType = new();

    public static async Task<T> To<T>(this InsertMessage insertMessage, CancellationToken cancellationToken)
        where T : class
    {
        var type = typeof(T);
        var result = (T)Activator.CreateInstance(type)!;
        var infoConverterByPropName = _propInfoConvertersByType.GetOrAdd(type, GeneratePropInfoConvertionsByPropName);
        var columnNumber = 0;
        await foreach (var value in insertMessage.NewRow)
        {
            var propName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            var propValue = await value.Get<string?>(cancellationToken);
            infoConverterByPropName.SetValue(result, propName, propValue);
        }

        return result;
    }

    public static T To<T>(this NpgsqlDataReader reader)
        where T : class
    {
        var type = typeof(T);
        var result = (T)Activator.CreateInstance(type)!;
        var infoConverterByPropName = _propInfoConvertersByType.GetOrAdd(type, GeneratePropInfoConvertionsByPropName);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var propName = reader.GetName(i);
            var propValue = reader.GetValue(i)?.ToString();
            infoConverterByPropName.SetValue(result, propName, propValue);
        }

        return result;
    }

    private static void SetValue<T>(this Dictionary<string, PropertyInfoConverter> infoConverterByPropName, T value, string propName, string? propValue)
        where T : class
    {
        if (!infoConverterByPropName.TryGetValue(propName, out var propInfoConverter))
        {
            throw new InvalidOperationException($"Property {propName} not found on type {typeof(T).Name}.");
        }

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

    public static async Task<Dictionary<string, string>> ToDictionary(this InsertMessage insertMessage, CancellationToken cancellationToken)
    {
        var columnNumber = 0;
        var result = new Dictionary<string, string>();
        await foreach (var value in insertMessage.NewRow)
        {
            var columnName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            result[columnName] = await value.Get<string>(cancellationToken);
        }

        return result;
    }

    public static Dictionary<string, string> ToDictionary(this NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            result[columnName] = reader.GetValue(i).ToString()!;
        }
        return result;
    }
}
