# Quickstart: Iteration 1 - Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-01-27 | **Phase**: 1 Complete

## Overview

This quickstart guide provides step-by-step instructions for validating the Iteration 1 HTML parsing and rendering functionality. It covers local development setup, testing procedures, and cross-platform validation.

## Prerequisites

- .NET 8.0 SDK installed
- Windows or Linux development environment
- Git repository cloned locally
- PowerShell (Windows) or Bash (Linux) terminal

## Local Development Setup

### 1. Restore Dependencies

```bash
# Navigate to project root
cd C:/Projects/Html2Pdf

# Restore solution dependencies
dotnet restore src/NetHtml2Pdf.sln
```

### 2. Build Solution

```bash
# Build the solution
dotnet build src/NetHtml2Pdf.sln

# Verify no build errors
echo "Build completed successfully"
```

### 3. Run Unit Tests

```bash
# Run all tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj

# Run specific test categories
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Core
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Parser
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Renderer
```

## Contract Testing

### 1. Core Paragraphs Contract

```bash
# Run core paragraphs contract tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=CoreParagraphs

# Expected output: All tests pass, PDF generated with proper paragraph formatting
```

**Validation Checklist**:
- [ ] Paragraphs render with proper spacing
- [ ] Bold and italic text render correctly
- [ ] CSS classes apply background colors
- [ ] Nested elements maintain structure

### 2. Table Borders and Alignment Contract

```bash
# Run table contract tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=TableBordersAlignment

# Expected output: All tests pass, PDF generated with proper table formatting
```

**Validation Checklist**:
- [ ] Table borders render correctly
- [ ] Cell alignment (left, center, right) works
- [ ] Vertical alignment (top, middle, bottom) works
- [ ] Background colors apply to cells
- [ ] Border collapse behavior works

### 3. Fallback Unsupported Tag Contract

```bash
# Run fallback contract tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=FallbackUnsupportedTag

# Expected output: All tests pass, PDF generated with fallback text
```

**Validation Checklist**:
- [ ] Unsupported elements render as plain text
- [ ] Warning logs generated for each fallback
- [ ] Document structure preserved
- [ ] Supported elements render normally

## Integration Testing

### 1. Run Integration Test Console

```bash
# Navigate to test console project
cd src/NetHtml2Pdf.TestConsole

# Run integration test console
dotnet run --scenario ultimate-html

# Expected output: PDF generated successfully with comprehensive HTML content
```

### 2. Validate Integration Output

**Check generated PDF**:
- [ ] PDF file created in output directory
- [ ] All supported HTML elements render correctly
- [ ] CSS styling applied properly
- [ ] Tables render with borders and alignment
- [ ] Fallback elements render as plain text

## Cross-Platform Validation

### Windows Validation

```powershell
# Run regression test suite on Windows
dotnet test src/NetHtml2Pdf.sln --filter Iteration1

# Run integration test console on Windows
dotnet run --project src/NetHtml2Pdf.TestConsole/NetHtml2Pdf.TestConsole.csproj --scenario ultimate-html

# Capture Windows output
$windowsOutput = Get-ChildItem -Path "output" -Filter "*.pdf" | Select-Object -First 1
Write-Host "Windows PDF: $($windowsOutput.FullName)"
```

### Linux Validation

```bash
# Run regression test suite on Linux
dotnet test src/NetHtml2Pdf.sln --filter Iteration1

# Run integration test console on Linux
dotnet run --project src/NetHtml2Pdf.TestConsole/NetHtml2Pdf.TestConsole.csproj --scenario ultimate-html

# Capture Linux output
linux_output=$(find output -name "*.pdf" | head -1)
echo "Linux PDF: $linux_output"
```

### Cross-Platform Comparison

```bash
# Compare PDF outputs (if both platforms available)
# Note: Visual/functional equivalence expected, not byte-level identity

# Check file sizes are similar
echo "Windows PDF size: $(stat -c%s windows_output.pdf) bytes"
echo "Linux PDF size: $(stat -c%s linux_output.pdf) bytes"

# Visual comparison criteria:
# - Content should be identical
# - Layout should be identical
# - Styling should be identical
# - Metadata differences are acceptable
```

## Performance Validation

### 1. Timing Validation

```bash
# Run performance tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Performance

# Expected results:
# - Parse time: < 200ms for typical documents
# - Render time: < 800ms for typical documents
# - Total time: < 1.5 seconds end-to-end
```

### 2. Memory Validation

```bash
# Monitor memory usage during testing
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Performance --logger "console;verbosity=detailed"

# Expected results:
# - Memory usage: < 20MB for typical documents
# - Peak memory: < 40MB during processing
# - No memory leaks detected
```

## Dependency Validation

### 1. Managed Dependencies Audit

```bash
# Run dependency audit
dotnet list src/NetHtml2Pdf/NetHtml2Pdf.csproj package --include-transitive

# Expected results:
# - All dependencies are managed .NET libraries
# - No GDI+ or native rendering libraries
# - QuestPDF and AngleSharp are primary dependencies
```

### 2. Cross-Platform Build Validation

```bash
# Verify solution builds on both platforms
dotnet build src/NetHtml2Pdf.sln --configuration Release

# Expected results:
# - Build succeeds without errors
# - No platform-specific dependencies
# - All assemblies generated successfully
```

## Troubleshooting

### Common Issues

**Build Errors**:
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` before building
- Check for missing package references

**Test Failures**:
- Verify all dependencies are restored
- Check test data files are present
- Ensure output directories are writable

**Cross-Platform Differences**:
- Accept metadata differences (creation dates, producer strings)
- Focus on visual/functional equivalence
- Document any significant differences

### Debug Mode

```bash
# Run tests in debug mode for detailed output
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --configuration Debug --logger "console;verbosity=detailed"

# Run integration console in debug mode
dotnet run --project src/NetHtml2Pdf.TestConsole/NetHtml2Pdf.TestConsole.csproj --configuration Debug --scenario ultimate-html
```

## Validation Checklist

### Pre-Release Validation

- [ ] All unit tests pass
- [ ] All contract tests pass
- [ ] Integration test console runs successfully
- [ ] Cross-platform validation completed
- [ ] Performance requirements met
- [ ] Memory usage within limits
- [ ] Dependency audit passed
- [ ] No build warnings or errors

### Documentation Validation

- [ ] Research.md completed
- [ ] Data model documented
- [ ] Contracts defined
- [ ] Quickstart guide validated
- [ ] All clarifications resolved

## Next Steps

After completing this quickstart validation:

1. **Phase 2**: Run `/speckit.tasks` to generate implementation tasks
2. **Phase 3**: Execute tasks in order (T001 → T050)
3. **Phase 4**: Complete implementation and testing
4. **Phase 5**: Final validation and release

## Support

For issues or questions:
- Check the research.md for technology decisions
- Review contracts for expected behavior
- Consult the data model for entity specifications
- Refer to the constitution for development standards

---

**Quickstart Complete**: All validation procedures documented and ready for execution.