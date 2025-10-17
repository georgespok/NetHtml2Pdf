using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Test.Layout;

internal static class LayoutTestHelper
{
    private static readonly Assembly ProductAssembly = typeof(DocumentNode).Assembly;

    public static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false, ignoreCase: false);
        return type ?? throw new InvalidOperationException($"Type '{fullName}' is not available.");
    }

    public static Type MakeReadOnlyListType(Type elementType)
    {
        return typeof(IReadOnlyList<>).MakeGenericType(elementType);
    }

    public static PropertyInfo RequireProperty(Type declaringType, string propertyName)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return property ?? throw new InvalidOperationException($"Property '{declaringType.FullName}.{propertyName}' is not available.");
    }

    public static MethodInfo RequireMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        return method ?? throw new InvalidOperationException($"Method '{declaringType.FullName}.{methodName}' with expected signature is not available.");
    }

    public static ConstructorInfo RequireConstructor(Type declaringType, params Type[] parameterTypes)
    {
        var ctor = declaringType.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        return ctor ?? throw new InvalidOperationException($"Constructor '{declaringType.FullName}({string.Join(", ", parameterTypes.Select(t => t.Name))})' is not available.");
    }

    public static object CreateLayoutBox(DocumentNode node, DisplayClass display, string nodePath)
    {
        var layoutBoxType = RequireType("NetHtml2Pdf.Layout.Model.LayoutBox");
        var readOnlyListType = MakeReadOnlyListType(layoutBoxType);
        var spacingType = RequireType("NetHtml2Pdf.Layout.Model.LayoutSpacing");

        var ctor = RequireConstructor(
            layoutBoxType,
            typeof(DocumentNode),
            typeof(DisplayClass),
            typeof(CssStyleMap),
            spacingType,
            typeof(string),
            readOnlyListType);

        var spacingInstance = Activator.CreateInstance(spacingType, CssStyleMap.Empty.Margin, CssStyleMap.Empty.Padding, CssStyleMap.Empty.Border)
            ?? throw new InvalidOperationException("Failed to create LayoutSpacing instance.");

        var emptyChildren = CreateTypedList(layoutBoxType);
        return ctor.Invoke(new object?[] { node, display, CssStyleMap.Empty, spacingInstance, nodePath, emptyChildren });
    }

    public static object CreateLayoutFragment(float width, float height, float? baseline, string nodePath)
    {
        var fragmentType = RequireType("NetHtml2Pdf.Layout.Model.LayoutFragment");
        var kindType = RequireType("NetHtml2Pdf.Layout.Model.LayoutFragmentKind");
        var layoutBoxType = RequireType("NetHtml2Pdf.Layout.Model.LayoutBox");
        var diagnosticsType = RequireType("NetHtml2Pdf.Layout.Model.LayoutDiagnostics");
        var readOnlyListType = MakeReadOnlyListType(fragmentType);

        var ctor = RequireConstructor(fragmentType,
            kindType,
            layoutBoxType,
            typeof(float),
            typeof(float),
            typeof(float?),
            readOnlyListType,
            diagnosticsType);

        var emptyChildren = CreateTypedList(fragmentType);
        var layoutBox = CreateLayoutBox(new DocumentNode(DocumentNodeType.Paragraph), DisplayClass.Block, nodePath);
        var constraints = CreateLayoutConstraints(width, width, 0, height, height, allowBreaks: true);
        var metadata = new Dictionary<string, string>();
        var diagnostics = Activator.CreateInstance(diagnosticsType, "Test", constraints, width, height, metadata)
            ?? throw new InvalidOperationException($"Type '{diagnosticsType.FullName}' must expose the expected constructor.");

        var kindValue = Enum.Parse(kindType, "Block");

        return ctor.Invoke(new object?[] { kindValue, layoutBox, width, height, baseline, emptyChildren, diagnostics });
    }

    public static object CreateLayoutConstraints(float inlineMin, float inlineMax, float blockMin, float blockMax, float pageRemainingBlockSize, bool allowBreaks)
    {
        var constraintsType = RequireType("NetHtml2Pdf.Layout.Model.LayoutConstraints");
        var ctor = RequireConstructor(constraintsType,
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(bool));

        return ctor.Invoke(new object?[] { inlineMin, inlineMax, blockMin, blockMax, pageRemainingBlockSize, allowBreaks });
    }

    public static object CreateTypedList(Type elementType, params object[] items)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var item in items)
        {
            list.Add(item);
        }

        var readOnlyInterface = MakeReadOnlyListType(elementType);
        if (readOnlyInterface.IsInstanceOfType(list))
        {
            return list;
        }

        var array = Array.CreateInstance(elementType, list.Count);
        list.CopyTo(array, 0);
        return array;
    }
}
