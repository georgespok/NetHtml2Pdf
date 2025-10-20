using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using Xunit;

namespace NetHtml2Pdf.Test.Layout.Pagination;

public class PaginationServiceBehaviorTests
{
    [Fact]
    public void SingleFragmentFitsOnOnePage_ReturnsSinglePageWithFragment()
    {
        var fragment = CreateBlockFragment(width: 400, height: 200, nodePath: "Paragraph:0");
        var fragments = new[] { fragment };
        var constraints = new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.Empty,
            headerBand: 0f,
            footerBand: 0f);

        var paginationOptions = PaginationOptions.FromRendererOptions(new RendererOptions
        {
            EnablePaginationDiagnostics = false
        });

        var service = new PaginationService();

        var result = service.Paginate(fragments, constraints, paginationOptions, NullLogger.Instance);

        var page = Assert.Single(result.Pages);
        Assert.Equal(1, page.PageNumber);

        var slice = Assert.Single(page.Fragments);
        Assert.Equal(fragment, slice.SourceFragment);
        Assert.Equal(FragmentSliceKind.Full, slice.SliceKind);
    }

    [Fact]
    public void FragmentTallerThanContentHeight_IsSplitAcrossPages()
    {
        var tallFragment = CreateBlockFragment(width: 400, height: 900, nodePath: "Paragraph:0");
        var fragments = new[] { tallFragment };

        var constraints = new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.FromAll(40),
            headerBand: 60f,
            footerBand: 60f);

        var paginationOptions = PaginationOptions.FromRendererOptions(new RendererOptions
        {
            EnablePaginationDiagnostics = false
        });

        var service = new PaginationService();

        var result = service.Paginate(fragments, constraints, paginationOptions, NullLogger.Instance);

        Assert.Equal(2, result.Pages.Count);

        var firstPage = result.Pages[0];
        var secondPage = result.Pages[1];

        var firstSlice = Assert.Single(firstPage.Fragments);
        var secondSlice = Assert.Single(secondPage.Fragments);

        Assert.Equal(FragmentSliceKind.Start, firstSlice.SliceKind);
        Assert.Equal(FragmentSliceKind.End, secondSlice.SliceKind);

        Assert.Equal(tallFragment, firstSlice.SourceFragment);
        Assert.Equal(tallFragment, secondSlice.SourceFragment);
    }

    [Fact]
    public void SplitPages_ShouldExposeCarryLinks()
    {
        var tallFragment = CreateBlockFragment(width: 400, height: 900, nodePath: "Paragraph:1");
        var fragments = new[] { tallFragment };

        var constraints = new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.FromAll(20),
            headerBand: 40f,
            footerBand: 40f);

        var service = new PaginationService();
        var options = PaginationOptions.FromRendererOptions(new RendererOptions());

        var document = service.Paginate(fragments, constraints, options, NullLogger.Instance);

        Assert.Equal(2, document.Pages.Count);

        var firstCarry = document.Pages[0].CarryLink;
        var secondCarry = document.Pages[1].CarryLink;

        Assert.NotNull(firstCarry);
        Assert.Equal(2, firstCarry!.ContinuesToPage);
        Assert.True(firstCarry.RemainingBlockSize > 0);

        Assert.NotNull(secondCarry);
        Assert.Equal(1, secondCarry!.ContinuesFromPage);
    }

    [Fact]
    public void ContentBoundsRespectHeaderFooterAndMargins()
    {
        var fragment = CreateBlockFragment(width: 400, height: 200, nodePath: "Paragraph:2");
        var constraints = new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.FromSpecific(20, 30, 40, 10),
            headerBand: 60f,
            footerBand: 80f);

        var service = new PaginationService();
        var options = PaginationOptions.FromRendererOptions(new RendererOptions());

        var document = service.Paginate([fragment], constraints, options, NullLogger.Instance);
        var page = Assert.Single(document.Pages);

        Assert.Equal(constraints.ContentHeight, page.ContentBounds.Height);
        Assert.Equal(constraints.ContentWidth, page.ContentBounds.Width);
    }

    [Fact]
    public void KeepTogetherFragmentExceedingContentHeight_Throws()
    {
        var fragment = CreateBlockFragment(
            width: 400,
            height: 800,
            nodePath: "KeepTogether:0",
            metadata: new Dictionary<string, string> { [KeepTogetherKey] = bool.TrueString });
        Assert.True(fragment.Diagnostics.Metadata.TryGetValue(KeepTogetherKey, out var keepTogetherValue) && bool.Parse(keepTogetherValue));

        var constraints = new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.FromAll(20),
            headerBand: 40f,
            footerBand: 40f);

        var service = new PaginationService();
        var options = PaginationOptions.FromRendererOptions(new RendererOptions());

        Assert.Throws<PaginationException>(() => service.Paginate([fragment], constraints, options, NullLogger.Instance));
    }

    [Fact]
    public void KeepWithNextMovesFragmentWhenNextDoesNotFit()
    {
        var intro = CreateBlockFragment(width: 400, height: 400, nodePath: "Intro:0");
        var keepWithNext = CreateBlockFragment(
            width: 400,
            height: 200,
            nodePath: "Keep:1",
            metadata: new Dictionary<string, string> { [KeepWithNextKey] = bool.TrueString });
        Assert.True(keepWithNext.Diagnostics.Metadata.TryGetValue(KeepWithNextKey, out var keepWithNextValue) && bool.Parse(keepWithNextValue));
        var next = CreateBlockFragment(width: 400, height: 260, nodePath: "Next:2");

        var constraints = new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.FromAll(20),
            headerBand: 40f,
            footerBand: 40f);

        var service = new PaginationService();
        var options = PaginationOptions.FromRendererOptions(new RendererOptions());

        var document = service.Paginate([intro, keepWithNext, next], constraints, options, NullLogger.Instance);

        Assert.Equal(2, document.Pages.Count);

        var firstPageFragments = document.Pages[0].Fragments;
        Assert.Single(firstPageFragments);
        Assert.Equal("Intro:0", firstPageFragments[0].SourceFragment.NodePath);

        var secondPageFragments = document.Pages[1].Fragments;
        Assert.Equal(2, secondPageFragments.Count);
        Assert.Equal("Keep:1", secondPageFragments[0].SourceFragment.NodePath);
        Assert.Equal(FragmentSliceKind.Full, secondPageFragments[0].SliceKind);
        Assert.Equal("Next:2", secondPageFragments[1].SourceFragment.NodePath);
        Assert.Equal(FragmentSliceKind.Full, secondPageFragments[1].SliceKind);
    }

    private const string KeepTogetherKey = "pagination:keepTogether";
    private const string KeepWithNextKey = "pagination:keepWithNext";

    private static LayoutFragment CreateBlockFragment(
        float width,
        float height,
        string nodePath,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var node = new DocumentNode(DocumentNodeType.Paragraph);
        var box = new LayoutBox(
            node,
            DisplayClass.Block,
            CssStyleMap.Empty,
            new LayoutSpacing(BoxSpacing.Empty, BoxSpacing.Empty, BorderInfo.Empty),
            nodePath,
            []);

        var constraints = new LayoutConstraints(
            inlineMin: width,
            inlineMax: width,
            blockMin: height,
            blockMax: height,
            pageRemainingBlockSize: height,
            allowBreaks: false);

        var diagnostics = new LayoutDiagnostics("Test", constraints, width, height, metadata);

        return LayoutFragment.CreateBlock(box, width, height, [], diagnostics);
    }
}
