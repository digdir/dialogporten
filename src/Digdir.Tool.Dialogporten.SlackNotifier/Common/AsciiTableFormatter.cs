using System.Text;

namespace Digdir.Tool.Dialogporten.SlackNotifier.Common;

public static class AsciiTableFormatter
{
    public static string ToAsciiTable(this IEnumerable<IEnumerable<object>> rows, int maxColumnWidth = 90) =>
        rows.Select(x => x.ToList())
            .ToList()
            .ToAsciiTable(maxColumnWidth);

    private static readonly string[] StringArray = [""];

    private static string ToAsciiTable(this List<List<object>> rows, int maxColumnWidth)
    {
        var builder = new StringBuilder();

        // Determine column types before modifying cell contents
        var types = GetColumnTypes(rows);

        AddLineBreaks(rows, maxColumnWidth);
        var sizes = MaxLengthInEachColumn(rows);

        // Top border
        AppendLine(builder, sizes);

        // For each row
        for (var rowNum = 0; rowNum < rows.Count; rowNum++)
        {
            var row = rows[rowNum];

            // For each cell, split the content into lines
            var cellLines = row.Select(cell => cell?.ToString()?.Split('\n') ?? StringArray).ToList();

            // Determine the maximum number of lines in this row
            var maxLines = cellLines.Max(lines => lines.Length);

            // For each line index
            for (var lineIndex = 0; lineIndex < maxLines; lineIndex++)
            {
                // For each cell
                for (var i = 0; i < cellLines.Count; i++)
                {
                    var lines = cellLines[i];
                    var size = sizes[i];
                    var type = types[i];

                    builder.Append("| ");

                    var line = lineIndex < lines.Length ? lines[lineIndex] : "";

                    builder.Append(type == ColumnType.Numeric ? line.PadLeft(size) : line.PadRight(size));

                    builder.Append(' ');

                    if (i == cellLines.Count - 1)
                    {
                        // Add right border for last column
                        builder.Append('|');
                    }
                }
                builder.Append('\n');
            }

            // Add separator between rows
            AppendLine(builder, sizes);
        }

        return builder.ToString();
    }

    private static void AddLineBreaks(List<List<object>> rows, int maxColumnWidth)
    {
        foreach (var row in rows)
        {
            for (var colIndex = 0; colIndex < row.Count; colIndex++)
            {
                if (row[colIndex] is string str)
                {
                    var words = str.Split(' ');
                    var lines = new List<string>();
                    var currentLine = string.Empty;

                    foreach (var word in words)
                    {
                        if ((currentLine + word).Length > maxColumnWidth)
                        {
                            lines.Add(currentLine.TrimEnd());

                            if (word.Length > maxColumnWidth)
                            {
                                maxColumnWidth = word.Length;
                            }

                            currentLine = "";
                        }
                        currentLine += word + " ";
                    }
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.TrimEnd());
                    }

                    row[colIndex] = string.Join("\n", lines).Trim();
                }
                else if (row[colIndex]?.ToString()?.Length > maxColumnWidth)
                {
                    var strValue = row[colIndex].ToString();
                    var brokenLines = new List<string>();
                    for (var i = 0; i < strValue?.Length; i += maxColumnWidth)
                    {
                        brokenLines.Add(strValue.Substring(i, Math.Min(maxColumnWidth, strValue.Length - i)));
                        if (brokenLines.Last().Length > maxColumnWidth)
                        {
                            maxColumnWidth = brokenLines.Last().Length;
                        }
                    }
                    row[colIndex] = string.Join("\n", brokenLines);
                }
            }
        }
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
        for (var i = 0; i < rows[0].Count; i++)
        {
            var max = rows.Max(row => row[i]?.ToString()?.Split('\n').Max(line => line.Length) ?? 0);
            sizes.Add(max);
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
