# Quickstart: CSS Display (Block, Inline-Block, None)

## Sample HTML

```html
<style>
  .box { margin: 8px; padding: 8px; border: 1px solid #444; }
  .inline { display: inline-block; width: 120; height: 40; background-color: #eee; }
  .hidden { display: none; }
</style>
<section>
  <div class="box" style="display: block;">Block box A</div>
  <span class="box inline">Inline-block A</span>
  <span class="box inline">Inline-block B</span>
  <div class="box hidden">Should not appear</div>
</section>
```

## Expected Behavior
- "Block box A" starts on a new line.
- Inline-block A/B appear on the same line if space allows; wrap as whole boxes.
- "Should not appear" is omitted from output.

## Generate PDF
```csharp
var html = File.ReadAllText("sample.html");
var bytes = new PdfBuilder()
    .AddPage(html)
    .Build();
await File.WriteAllBytesAsync("display-sample.pdf", bytes);
```
