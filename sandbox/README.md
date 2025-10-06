# NetHtml2Pdf Sandbox

This console app exists solely to smoke-test a packed `NetHtml2Pdf` NuGet before you publish it.

## How to Run

1. Update `SandboxNetHtml2Pdf.csproj` so the `NetHtml2Pdf` package reference points to the `.nupkg` version you want to verify.
2. Restore and execute the project:
   ```bash
   dotnet restore
   dotnet run
   ```
3. The app renders a sample document and writes `sandbox-test.pdf` to your temp directory. Open the file to confirm fonts, layout, and metadata look correct.

Use this project whenever you need a quick sanity check on a freshly built package.
