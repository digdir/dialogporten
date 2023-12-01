using System.Text;

namespace Digdir.Tool.Dialogporten.SlackNotifier.Common;

public static class AsciiTableFormatter
{
    public static string ToAsciiTable(this IEnumerable<IEnumerable<object>> rows) =>
        rows.Select(x => x.ToList())
            .ToList()
            .ToAsciiTable();

    private static string ToAsciiTable(this List<List<object>> rows)
    {
        var builder = new StringBuilder();

        var sizes = MaxLengthInEachColumn(rows);
        var types = GetColumnTypes(rows);

        for (var rowNum = 0; rowNum < rows.Count; rowNum++)
        {
            if (rowNum == 0)
            {
                // Top border
                AppendLine(builder, sizes);
                if (rows[0][0] == null)
                {
                    continue;
                }
            }

            var row = rows[rowNum];
            for (var i = 0; i < row.Count; i++)
            {
                var item = row[i]!;
                var size = sizes[i];
                builder.Append("| ");
                if (item == null)
                {
                    builder.Append("".PadLeft(size));
                }
                else if (types[i] == ColumnType.Numeric)
                {
                    builder.Append(item.ToString()!.PadLeft(size));
                }
                else if (types[i] == ColumnType.Text)
                {
                    builder.Append(item.ToString()!.PadRight(size));
                }
                else
                {
                    throw new InvalidOperationException("Unexpected state");
                }

                builder.Append(' ');

                if (i == row.Count - 1)
                {
                    // Add right border for last column
                    builder.Append('|');
                }
            }
            builder.Append('\n');
            if (rowNum == 0)
            {
                AppendLine(builder, sizes);
            }
        }

        AppendLine(builder, sizes);

        return builder.ToString();
    }

    private static void AppendLine(StringBuilder builder, IReadOnlyList<int> sizes)
    {
        builder.Append('o');

        foreach (var i in sizes)
        {
            builder.Append(new string('-', i + 2));
            builder.Append('o');
        }
        builder.Append('\n');
    }

    private static List<int> MaxLengthInEachColumn(IReadOnlyList<List<object>> rows)
    {
        var sizes = new List<int>();
        //Start from second row to skip the header
        for (var i = 0; i < rows[1].Count; i++)
        {
            var max = rows.Max(row => row[i]?.ToString()?.Length ?? 0);
            sizes.Insert(i, max);
        }
        return sizes;
    }

    private static List<ColumnType> GetColumnTypes(List<List<object>> rows)
    {
        var types = new List<ColumnType>();
        for (var i = 0; i < rows[1].Count; i++)
        {
            var isNumeric = rows.Skip(1).All(row => row[i]?.GetType()?.IsNumericType() ?? false);
            var columnType = isNumeric ? ColumnType.Numeric : ColumnType.Text;
            types.Insert(i, columnType);
        }
        return types;
    }

    /// <summary>
    /// https://stackoverflow.com/a/5182747/2513761
    /// </summary>
    private static bool IsNumericType(this Type type)
    {
        if (type == null)
        {
            return false;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
            case TypeCode.Object:
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return Nullable.GetUnderlyingType(type)!.IsNumericType();
                }
                return false;
            case TypeCode.Empty:
            case TypeCode.DBNull:
            case TypeCode.Boolean:
            case TypeCode.Char:
            case TypeCode.DateTime:
            case TypeCode.String:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    private enum ColumnType
    {
        Numeric,
        Text
    }
}
