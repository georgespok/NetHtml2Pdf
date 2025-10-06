using NetHtml2Pdf.Core;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Core;

public class DocumentNodeTypeTests
{
    
    #region Enum Count Tests

    [Fact]
    public void DocumentNodeType_ShouldHaveExpectedCount()
    {
        // Arrange & Act
        var enumValues = Enum.GetValues<DocumentNodeType>();

        // Assert
        enumValues.Length.ShouldBe(29); // Current count of all enum values
    }

    #endregion

    #region Enum Parsing Tests

    
    [Theory]
    [InlineData("INVALID")]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData("Heading7")]
    [InlineData("InvalidType")]
    public void DocumentNodeType_ParseInvalidValue_ShouldThrowArgumentException(string invalidValue)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => Enum.Parse<DocumentNodeType>(invalidValue));
    }

    #endregion

    #region Enum Flags Tests

    [Fact]
    public void DocumentNodeType_ShouldNotBeFlagsEnum()
    {
        // Arrange
        var enumType = typeof(DocumentNodeType);

        // Act & Assert
        enumType.GetCustomAttributes(typeof(FlagsAttribute), false).ShouldBeEmpty();
    }

    [Fact]
    public void DocumentNodeType_Values_ShouldBeSequential()
    {
        // Arrange
        var enumValues = Enum.GetValues<DocumentNodeType>().Cast<int>().ToArray();

        // Act & Assert
        for (int i = 1; i < enumValues.Length; i++)
        {
            enumValues[i].ShouldBe(enumValues[i - 1] + 1, 
                $"Enum values should be sequential. Expected {enumValues[i - 1] + 1}, got {enumValues[i]}");
        }
    }

    #endregion
}
