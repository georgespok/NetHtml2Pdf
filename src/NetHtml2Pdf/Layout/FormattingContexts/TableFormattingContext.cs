using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.FormattingContexts;

/// <summary>
///     Produces layout fragments for table nodes while attaching metadata used by pagination.
///     This initial implementation focuses on structure/diagnostics and delegates pagination
///     behaviour to later tasks.
/// </summary>
internal sealed class TableFormattingContext
{
    private const float DefaultRowHeight = 24f;
    private const float DefaultCaptionHeight = 20f;

    public static LayoutFragment Layout(LayoutBox tableBox, LayoutConstraints constraints,
        FormattingContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(tableBox);

        var columnCount = Math.Max(1, DetermineColumnCount(tableBox));
        var columnWidth = constraints.InlineMax > 0
            ? constraints.InlineMax / columnCount
            : columnCount;

        var fragments = new List<LayoutFragment>();
        var metadata = new Dictionary<string, string>
        {
            ["table:columnCount"] = columnCount.ToString()
        };

        var captionBox = tableBox.Children.FirstOrDefault(IsCaptionCandidate);
        if (captionBox is not null)
        {
            fragments.Add(LayoutCaption(captionBox, constraints));
            metadata["caption"] = "true";
        }

        foreach (var section in tableBox.Children.Where(IsTableSection))
            fragments.AddRange(LayoutSection(section, constraints, columnWidth, options));

        var height = fragments.Sum(fragment => fragment.Height);
        var diagnostics = new LayoutDiagnostics(
            "TableFormattingContext",
            constraints,
            constraints.InlineMax,
            height,
            metadata);

        return LayoutFragment.CreateBlock(tableBox, constraints.InlineMax, height, fragments, diagnostics);
    }

    private static IEnumerable<LayoutFragment> LayoutSection(
        LayoutBox sectionBox,
        LayoutConstraints tableConstraints,
        float columnWidth,
        FormattingContextOptions options)
    {
        var result = new List<LayoutFragment>();
        var sectionMetadata = new Dictionary<string, string>
        {
            ["table:section"] = sectionBox.Node.NodeType.ToString()
        };

        foreach (var rowBox in sectionBox.Children.Where(child => child.Node.NodeType == DocumentNodeType.TableRow))
        {
            var rowDiagnostics = new LayoutDiagnostics(
                "TableRow",
                tableConstraints,
                tableConstraints.InlineMax,
                DefaultRowHeight,
                sectionMetadata);

            var cellFragments = LayoutCells(rowBox, columnWidth, options);
            var rowHeight = cellFragments.Count != 0 ? cellFragments.Max(cell => cell.Height) : DefaultRowHeight;

            result.Add(LayoutFragment.CreateBlock(
                rowBox,
                tableConstraints.InlineMax,
                rowHeight,
                cellFragments,
                rowDiagnostics));
        }

        return result;
    }

    private static List<LayoutFragment> LayoutCells(LayoutBox rowBox, float columnWidth,
        FormattingContextOptions options)
    {
        var fragments = new List<LayoutFragment>();
        var cellConstraints = new LayoutConstraints(0, columnWidth, 0, columnWidth, columnWidth, false);

        var metadata = new Dictionary<string, string>
        {
            ["table:borderCollapse"] = options.EnableTableBorderCollapse ? "collapse" : "separate"
        };

        foreach (var cellBox in rowBox.Children.Where(child => IsTableCell(child.Node.NodeType)))
        {
            var diagnostics = new LayoutDiagnostics(
                "TableCell",
                cellConstraints,
                columnWidth,
                DefaultRowHeight,
                metadata);

            fragments.Add(LayoutFragment.CreateBlock(
                cellBox,
                columnWidth,
                DefaultRowHeight,
                [],
                diagnostics));
        }

        return fragments;
    }

    private static LayoutFragment LayoutCaption(LayoutBox captionBox, LayoutConstraints tableConstraints)
    {
        var captionConstraints = new LayoutConstraints(
            tableConstraints.InlineMin,
            tableConstraints.InlineMax,
            0,
            DefaultCaptionHeight,
            DefaultCaptionHeight,
            false);

        var diagnostics = new LayoutDiagnostics(
            "TableCaption",
            captionConstraints,
            tableConstraints.InlineMax,
            DefaultCaptionHeight,
            new Dictionary<string, string> { ["table:caption"] = "true" });

        return LayoutFragment.CreateBlock(
            captionBox,
            tableConstraints.InlineMax,
            DefaultCaptionHeight,
            [],
            diagnostics);
    }

    private static bool IsTableCell(DocumentNodeType nodeType)
    {
        return nodeType == DocumentNodeType.TableHeaderCell || nodeType == DocumentNodeType.TableCell;
    }

    private static bool IsTableSection(LayoutBox box)
    {
        return box.Node.NodeType is
            DocumentNodeType.TableHead or
            DocumentNodeType.TableBody or
            DocumentNodeType.TableSection;
    }

    private static bool IsCaptionCandidate(LayoutBox box)
    {
        if (IsTableSection(box)) return false;

        return box.Node.NodeType is DocumentNodeType.Section or DocumentNodeType.Paragraph or DocumentNodeType.Div;
    }

    private static int DetermineColumnCount(LayoutBox tableBox)
    {
        foreach (var section in tableBox.Children.Where(IsTableSection))
        {
            var row = section.Children.FirstOrDefault(child => child.Node.NodeType == DocumentNodeType.TableRow);
            if (row is not null) return Math.Max(1, row.Children.Count(child => IsTableCell(child.Node.NodeType)));
        }

        return 1;
    }
}