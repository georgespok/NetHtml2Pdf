# Quickstart: Phase 2 Layout Model (Text Blocks)

## Goal
Enable the new layout pipeline for paragraphs/headings/spans behind `EnableNewLayoutForTextBlocks`, validate parity, and inspect diagnostics.

## Prerequisites
1. Ensure Phase 1 seams (`DisplayClassifier`, `WrapWithSpacing`, `InlineFlowLayoutEngine`) are merged.
2. Restore dependencies and build once: `dotnet build`.
3. Baseline verification: `dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj`.

## Enable the Feature Flag
```csharp
var options = new ConverterOptions
{
    EnableNewLayoutForTextBlocks = true
};
```
- Flag defaults to `false`; enable per invocation when running parity experiments.
- Optional diagnostics:
```csharp
options.EnableLayoutDiagnostics = true;
```

## Test the New Pipeline
1. Run unit + contract tests for layout contexts:
   ```
   dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Layout
   ```
2. Execute parity suite comparing flag on/off outputs:
   ```
   dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter BlockComposerFragmentParity
   ```
3. Inspect structured logs for `LayoutEngine.FragmentMeasured` events to confirm fragments are emitted.
4. Validate full-suite parity and capture the runtime delta (keep <5%):
   ```
   # Flag off (baseline)
   dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj

   # Flag on
   $env:ENABLE_NEW_LAYOUT_FOR_TEXT_BLOCKS = "true"
   dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj
   ```
   Record the durations printed by the test runner; aim for less than 5% slowdown with the flag enabled.

## Revert / Troubleshoot
- Disable the flag to fall back to the legacy pipeline.
- If diagnostics flood logs, turn off tracing or reduce sampling.
- On exceptions thrown from layout contexts, capture node path from log payload and file a regression bug (no auto fallback expected).

## Next Steps
- After parity is confirmed, keep flag default off for production builds.
- Use fragment diagnostics to evaluate pagination and additional formatting contexts in upcoming phases.
