using Digdir.Tool.Dialogporten.SlackNotifier.Common;

namespace Digdir.Tool.Dialogporten.SlackNotifier.Tests;

public class AsciiTableFormatterTests
{
    [Fact]
    public void AddLineBreaks_TextLongerThanMaxColumnWidth_ShouldWrapText()
    {
        // Arrange
        var rows = new List<List<object>>
        {
            new() { "Header1" },
            new() { "This is a very long text that exceeds the max column width" }
        };
        var maxColumnWidth = 20;

        // Act
        var result = AsciiTableFormatter.ToAsciiTable(rows, maxColumnWidth);

        // Assert
        var expected =
            """
            o----------------------o
            | Header1              |
            o----------------------o
            | This is a very long  |
            | text that exceeds    |
            | the max column width |
            o----------------------o

            """;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToAsciiTable_WordLongerThanMaxColumnWidth_ShouldNotBreakWord()
    {
        // Arrange
        var rows = new List<List<object>>
        {
            new() { "Header1" },
            new() { "Supercalifragilisticexpialidocious" }
        };
        var maxColumnWidth = 10;

        // Act
        var result = AsciiTableFormatter.ToAsciiTable(rows, maxColumnWidth);

        // Assert
        var expected =
            """
            o------------------------------------o
            | Header1                            |
            o------------------------------------o
            | Supercalifragilisticexpialidocious |
            o------------------------------------o

            """;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToAsciiTable_CellWithNull_ShouldHandleNullGracefully()
    {
        // Arrange
        var rows = new List<List<object>>
        {
            new() { "Header1" },
            new() { null! }
        };
        var maxColumnWidth = 10;

        // Act
        var result = AsciiTableFormatter.ToAsciiTable(rows, maxColumnWidth);

        // Assert
        var expected =
            """
            o---------o
            | Header1 |
            o---------o
            |         |
            o---------o

            """;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToAsciiTable_CellWithEmptyString_ShouldHandleEmptyString()
    {
        // Arrange
        var rows = new List<List<object>>
        {
            new() { "Header1" },
            new() { "" }
        };
        var maxColumnWidth = 10;

        // Act
        var result = AsciiTableFormatter.ToAsciiTable(rows, maxColumnWidth);

        // Assert
        var expected =
            """
            o---------o
            | Header1 |
            o---------o
            |         |
            o---------o

            """;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToAsciiTable_CellWithNumeric_ShouldRightAlign()
    {
        // Arrange
        var rows = new List<List<object>>
        {
            new() { "Header1" },
            new() { 12345 }
        };
        var maxColumnWidth = 5;

        // Act
        var result = AsciiTableFormatter.ToAsciiTable(rows, maxColumnWidth);

        // Assert
        var expected =
            """
            o---------o
            | Header1 |
            o---------o
            |   12345 |
            o---------o

            """;
        Assert.Equal(expected, result);
    }


    [Fact]
    public void ToAsciiTable_CellWithNonStringObject_ShouldConvertAndWrap()
    {
        // Arrange
        var rows = new List<List<object>>
        {
            new() { "Header1" },
            new() { 1234567890 }
        };
        var maxColumnWidth = 5;

        // Act
        var result = AsciiTableFormatter.ToAsciiTable(rows, maxColumnWidth);

        // Assert
        var expected =
            """
            o---------o
            | Header1 |
            o---------o
            | 1234567 |
            |     890 |
            o---------o

            """;
        Assert.Equal(expected, result);
    }
}
