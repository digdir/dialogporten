using Digdir.Tool.Dialogporten.SlackNotifier.Common;

namespace Digdir.Tool.Dialogporten.SlackNotifier.Tests;

public class AsciiTableFormatterTests
{
    [Fact]
    public void AddLineBreaks_TextLongerThanMaxColumnWidth_ShouldWrapText()
    {
        // Arrange
        List<List<object>> rows =
        [
            ["Header1"],
            ["This is a very long text that exceeds the max column width"]
        ];

        const int maxColumnWidth = 20;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
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
        List<List<object>> rows =
        [
            ["Header1"],
            ["Supercalifragilisticexpialidocious"]
        ];

        const int maxColumnWidth = 10;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
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
        List<List<object>> rows =
        [
            ["Header1"],
            [null!]
        ];

        const int maxColumnWidth = 10;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
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
        List<List<object>> rows =
        [
            ["Header1"],
            [""]
        ];

        const int maxColumnWidth = 10;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
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
        List<List<object>> rows =
        [
            ["Header1"],
            [12345]
        ];

        const int maxColumnWidth = 5;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
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
        List<List<object>> rows =
        [
            ["Header1"],
            [1234567890]
        ];

        const int maxColumnWidth = 5;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
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

    [Fact]
    public void ToAsciiTable_JaggedTable_ShouldPadWithEmptyCells()
    {
        // Arrange
        List<List<object>> rows =
        [
            ["Header1", "Header2"],
            ["a", "b", "c"],
            ["a", "b"]
        ];

        const int maxColumnWidth = 5;

        // Act
        var result = rows.ToAsciiTable(maxColumnWidth);

        // Assert
        const string expected =
            """
            o---------o---------o---o
            | Header1 | Header2 |   |
            o---------o---------o---o
            | a       | b       | c |
            o---------o---------o---o
            | a       | b       |   |
            o---------o---------o---o

            """;

        Assert.Equal(expected, result);
    }
}
