using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test;

/// <summary>
/// Tests to verify that PdfBuilder maintains backward compatibility after refactoring to use composition root.
/// These tests ensure that all existing public APIs work exactly as before.
/// </summary>
public class PdfBuilderCompatibilityTests
{
    [Theory]
    [InlineData("Default")]
    [InlineData("Logger")]
    [InlineData("OptionsAndLogger")]
    public void Constructors_ShouldWorkAsBefore(string constructorType)
    {
        // Arrange & Act - use constructors exactly as consumers would
        var builder = constructorType switch
        {
            "Default" => new PdfBuilder(),
            "Logger" => new PdfBuilder(NullLogger<PdfBuilderCompatibilityTests>.Instance),
            "OptionsAndLogger" => new PdfBuilder(RendererOptions.CreateDefault(), NullLogger<PdfBuilderCompatibilityTests>.Instance),
            _ => throw new ArgumentException($"Unknown constructor type: {constructorType}")
        };

        // Assert - verify builder is created successfully and can perform basic operations
        builder.ShouldNotBeNull();
        
        // Test basic functionality that consumers would use
        var result = builder.AddPage("<html><body><p>Test</p></body></html>").Build();
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void FluentInterface_ShouldWorkAsBefore()
    {
        // Arrange
        var builder = new PdfBuilder();

        // Act - use fluent interface exactly as consumers would
        var result = builder
            .SetHeader("<html><body><h1>Header</h1></body></html>")
            .SetFooter("<html><body><p>Footer</p></body></html>")
            .AddPage("<html><body><p>Page 1</p></body></html>")
            .AddPage("<html><body><p>Page 2</p></body></html>")
            .Build();

        // Assert - verify fluent interface works correctly
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("Reset")]
    [InlineData("MultiplePages")]
    public void BasicFunctionality_ShouldWorkAsBefore(string functionalityType)
    {
        // Arrange
        var builder = new PdfBuilder();

        // Act & Assert - test different basic functionality scenarios
        if (functionalityType == "Reset")
        {
            builder.AddPage("<html><body><p>First Page</p></body></html>");
            var result = builder.Reset().AddPage("<html><body><p>Second Page</p></body></html>").Build();
            
            result.ShouldNotBeNull();
            result.Length.ShouldBeGreaterThan(0);
        }
        else if (functionalityType == "MultiplePages")
        {
            builder.AddPage("<html><body><p>Page 1</p></body></html>");
            builder.AddPage("<html><body><p>Page 2</p></body></html>");
            builder.AddPage("<html><body><p>Page 3</p></body></html>");
            
            var result = builder.Build();
            result.ShouldNotBeNull();
            result.Length.ShouldBeGreaterThan(0);
        }
        else
        {
            throw new ArgumentException($"Unknown functionality type: {functionalityType}");
        }
    }

    [Fact]
    public void CustomOptions_ShouldWorkAsBefore()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };
        var logger = NullLogger<PdfBuilderCompatibilityTests>.Instance;

        // Act - use custom options exactly as consumers would
        var builder = new PdfBuilder(options, logger);
        var result = builder.AddPage("<html><body><p>Test with custom options</p></body></html>").Build();

        // Assert - verify custom options work correctly
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ComplexHtml_ShouldWorkAsBefore()
    {
        // Arrange
        var builder = new PdfBuilder();
        var complexHtml = @"
<html>
<head>
    <style>
        .header { font-size: 18px; font-weight: bold; }
        .content { margin: 10px 0; }
        .footer { font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='header'>Document Title</div>
    <div class='content'>
        <p>This is a complex HTML document with multiple elements.</p>
        <ul>
            <li>Item 1</li>
            <li>Item 2</li>
            <li>Item 3</li>
        </ul>
        <table border='1'>
            <tr><th>Header 1</th><th>Header 2</th></tr>
            <tr><td>Data 1</td><td>Data 2</td></tr>
        </table>
    </div>
    <div class='footer'>Document Footer</div>
</body>
</html>";

        // Act - use complex HTML exactly as consumers would
        var result = builder.AddPage(complexHtml).Build();

        // Assert - verify complex HTML works correctly
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void HeaderAndFooter_ShouldWorkAsBefore()
    {
        // Arrange
        var builder = new PdfBuilder();
        var headerHtml = "<html><body><h1>Document Header</h1></body></html>";
        var footerHtml = "<html><body><p>Page <span class='page'></span> of <span class='total'></span></p></body></html>";
        var contentHtml = "<html><body><p>Main content here</p></body></html>";

        // Act - use header and footer exactly as consumers would
        var result = builder
            .SetHeader(headerHtml)
            .SetFooter(footerHtml)
            .AddPage(contentHtml)
            .Build();

        // Assert - verify header and footer work correctly
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void BuildWithOptions_ShouldWorkAsBefore()
    {
        // Arrange
        var builder = new PdfBuilder();
        var converterOptions = new ConverterOptions();

        // Act - use Build with options exactly as consumers would
        var result = builder
            .AddPage("<html><body><p>Test with converter options</p></body></html>")
            .Build(converterOptions);

        // Assert - verify Build with options works correctly
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }
}
