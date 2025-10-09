# STRATEGY TO PARSE AND RENDER

Build “semantic normalization layer” between your parsed HTML nodes and QuestPDF’s strongly typed elements. Let’s formalize this layer as a node taxonomy and a property rule set.

🧱 Node Types (Core Abstractions)
Your normalized model doesn’t need to replicate all HTML tags — it only needs to represent rendering semantics.
Here’s a clean minimal set for HTML → PDF rendering:
Node Type	Purpose	Typical HTML Tags
BlockNode	Container that defines layout, box model, and block flow	<div>, <p>, <section>, <header>, <footer>, <li>, <td>, <th>
InlineNode	Container for inline elements that affect text style or flow	<span>, <strong>, <b>, <i>, <em>, <a>
TextRunNode	Holds raw text with applied inline styles	text nodes
ListNode	Represents ordered/unordered list as a structure	<ul>, <ol>
ListItemNode	Represents individual list item	<li>
TableNode	Represents table container	<table>
TableSectionNode	For <thead>, <tbody>, <tfoot>	
TableRowNode	For <tr>	
TableCellNode	For <td>, <th>	
LineBreakNode	Represents <br> line break	
DocumentNode	Root of the tree, normalizes <html> or <body>	

🎨 Property Rules by Node Type
Here’s a clear ruleset for what properties each node may have.
This ensures your rendering code doesn’t need to guess.
1. TextRunNode
Properties:
- Text: string
- FontFamily: string
- FontSize: float
- FontWeight: bold/normal
- FontStyle: normal/italic
- TextDecoration: underline/none
- Color: string (hex or named)
- BackgroundColor: string?
- LetterSpacing: float?
- LineHeight: float?
Allowed CSS: color, font-family, font-size, font-weight, font-style, text-decoration, background-color, letter-spacing, line-height.

2. InlineNode
Properties:
- Children: List<Node>
- InheritedTextStyle: merge(TextStyle)
Allowed CSS: same as TextRunNode + vertical-align.

3. BlockNode
Properties:
- Children: List<Node>
- Display: block | inline-block
- Margin: BoxSpacing
- Padding: BoxSpacing
- Border: BorderInfo (width, color, style)
- Width: float?
- Height: float?
- BackgroundColor: string?
- TextAlign: left/center/right/justify
Allowed CSS: margin, padding, border-*, width, height, background-color, text-align.

4. ListNode
Properties:
- ListType: ordered | unordered
- MarkerStyle: disc | circle | square | decimal | etc.
- Children: List<ListItemNode>
Allowed CSS: list-style-type, margin, padding.

5. ListItemNode
Properties:
- MarkerText: string (auto-generated if not overridden)
- Children: List<Node>
- Margin/Padding same as BlockNode

6. TableNode
Properties:
- BorderCollapse: collapse | separate
- Width: float?
- Children: List<TableSectionNode>
7. TableSectionNode
Properties:
- Children: List<TableRowNode>
- SectionType: thead | tbody | tfoot
8. TableRowNode
Properties:
- Children: List<TableCellNode>
- Height: float?
9. TableCellNode
Properties:
- ColSpan: int?
- RowSpan: int?
- Padding, Border, BackgroundColor
- TextAlign, VerticalAlign

10. LineBreakNode
No properties — just a semantic marker
11. DocumentNode
Properties:
- Metadata (title, author, etc.)
- Children: List<BlockNode>

⚙️ Normalization Rules
When building the normalized tree:
	1. Merge inline tags — <b><i>Text</i></b> → single TextRunNode with combined styles.
	2. Promote text styles upward — inherit styles down to text runs.
	3. Normalize block display — <p>, <div> → BlockNode(display=block).
	4. Normalize borders/margins/padding — use computed shorthand (CSSBox).
	5. Convert lists to structure — <ul> → ListNode, <li> → ListItemNode.
	6. Simplify table structure — <table> → TableNode, flatten nested <tbody> if only one.
	7. Ignore unknown tags — treat them as BlockNode or InlineNode based on default display semantics.

🧩 Example
Input HTML:
<div style="color:red; margin:10px">
  <p>Hello <b>World</b></p>
</div>
Normalized structure:
BlockNode (margin=10)
 └─ BlockNode (from <p>)
     └─ TextRunNode ("Hello ", Color=red)
     └─ TextRunNode ("World", Color=red, FontWeight=bold)

🗂️ Suggested .NET Class Structure
abstract class HtmlNode
{
    public List<HtmlNode> Children { get; } = new();
    public Dictionary<string, string> Attributes { get; } = new();
}
class BlockNode : HtmlNode
{
    public BoxSpacing Margin { get; set; }
    public BoxSpacing Padding { get; set; }
    public BorderInfo Border { get; set; }
    public string BackgroundColor { get; set; }
}
class TextRunNode : HtmlNode
{
    public string Text { get; set; }
    public string Color { get; set; }
    public string FontFamily { get; set; }
    public float? FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
}
class ListNode : HtmlNode { /* ... */ }
class TableNode : HtmlNode { /* ... */ }
class TableCellNode : HtmlNode { /* ... */ }
// etc.

Below is a complete normalization rule table — a concise, structured reference you can use when mapping parsed HTML tags and styles into your normalized node tree for QuestPDF rendering.
This table assumes the node model we defined earlier (BlockNode, TextRunNode, etc.) and will guide you on how each HTML tag should be normalized, what node type to use, what properties are valid, and what behaviors should be applied (block vs inline, inheritance, etc.).

🧭 HTML → Node Normalization Rules
🔹 Legend
Symbol	Meaning
🧱	Block-level container
✏️	Inline or text-level container
🗒	Structural element (lists, tables)
⚙️	Special behavior or rule

🧱 Block-Level Tags
HTML Tag	Node Type	Allowed Properties	Inherits From Parent	Notes
<div>	BlockNode	margin, padding, border, background-color, width, height, text-align	Yes	Generic block container.
<p>	BlockNode	margin, padding, text-align, color, font-*	Yes	Usually add default bottom margin (e.g., 8pt).
<section>, <header>, <footer>	BlockNode	same as <div>	Yes	Semantic blocks.
<h1>–<h6>	BlockNode + implicit TextRunNode	color, font-weight, font-size, margin	Yes	Apply predefined font size/weight (H1 largest).
<blockquote>	BlockNode	margin, padding, border-left, color, font-style	Yes	Default left indentation and italic.
<pre>	BlockNode	font-family, white-space, background-color, padding	Yes	Preserve whitespace; use monospace font.
<hr>	BlockNode	border-top, margin	No	Represented as horizontal line separator.

✏️ Inline-Level Tags
HTML Tag	Node Type	Allowed Properties	Inherits From Parent	Notes
<span>	InlineNode	color, font-family, font-size, font-weight, font-style, text-decoration, background-color	Yes	Inline styling container.
<b>, <strong>	InlineNode	Adds font-weight=bold	Yes	Merge with parent TextRunNode.
<i>, <em>	InlineNode	Adds font-style=italic	Yes	Merge with parent TextRunNode.
<u>	InlineNode	Adds text-decoration=underline	Yes	Combine with other inline styles.
<small>	InlineNode	Sets relative font-size=0.8em	Yes	Inherited otherwise.
<mark>	InlineNode	Adds background-color=yellow	Yes	Highlighted text.
<a>	InlineNode	color, text-decoration, href	Yes	Optional hyperlink data (can be ignored or styled).
<br>	LineBreakNode	none	No	Forces line break.

🗒 List Tags
HTML Tag	Node Type	Allowed Properties	Inherits From Parent	Notes
<ul>	ListNode	list-style-type, margin, padding	Yes	Default marker: disc.
<ol>	ListNode	same as <ul>	Yes	Default marker: decimal.
<li>	ListItemNode	margin, padding, color, font-*	Yes	Contains block or inline children. Marker is auto-generated.

🗒 Table Tags
HTML Tag	Node Type	Allowed Properties	Inherits From Parent	Notes
<table>	TableNode	border, border-collapse, width, margin	Yes	Use border-collapse: collapse by default.
<thead>, <tbody>, <tfoot>	TableSectionNode	none	Yes	Structural grouping; may be ignored visually.
<tr>	TableRowNode	background-color, height	Yes	Contains cells.
<td>	TableCellNode	colspan, rowspan, padding, border, text-align, vertical-align, background-color	Yes	Default text-align: left.
<th>	TableCellNode	same as <td> + font-weight=bold	Yes	Header cells; center-align by default.

✏️ Text Content
HTML Tag	Node Type	Allowed Properties	Inherits From Parent	Notes
(Text node)	TextRunNode	text, color, font-family, font-size, font-style, font-weight, text-decoration, line-height	Yes	Leaf node; inherits inline and block styles.

⚙️ Special Rules
Rule	Description
Style Inheritance	Inline and text styles (color, font-*, text-decoration) always cascade down to child nodes. Block layout properties (margin, padding, border, width, height) do not cascade.
Display Normalization	Default display mode: block for <div>, <p>, <table>, <ul>, <ol>; inline for <span>, <b>, <i>, <em>, <a>, etc.
Auto-Wrapping	Adjacent inline nodes inside a block should be merged into a single text run group for rendering efficiency.
Whitespace Normalization	Trim excessive whitespace except inside <pre> tags. Convert <br> into explicit line breaks in the text flow.
Unknown Tags	Treat as <span> (inline) if inside text, or <div> (block) otherwise.
Implicit Paragraphs	Consecutive text or inline elements inside <body> without wrapping <p> can be wrapped in a virtual BlockNode.

🧩 Suggested Type Mapping Summary
Node Type	Possible Children	Typical Tags
DocumentNode	BlockNode	<html>, <body>
BlockNode	BlockNode, InlineNode, TextRunNode, ListNode, TableNode	<div>, <p>, <section>
InlineNode	InlineNode, TextRunNode	<span>, <b>, <i>, <a>
TextRunNode	none	text only
ListNode	ListItemNode	<ul>, <ol>
ListItemNode	BlockNode, InlineNode	<li>
TableNode	TableSectionNode	<table>
TableSectionNode	TableRowNode	<thead>, <tbody>, <tfoot>
TableRowNode	TableCellNode	<tr>
TableCellNode	BlockNode, InlineNode, TextRunNode	<td>, <th>
LineBreakNode	none	<br>

⚖️ Example Normalization
HTML:
<p style="color: navy">Hello <strong>World</strong><br>Next line</p>
Normalized:
BlockNode (text-align=left)
 ├─ TextRunNode ("Hello ", Color=navy)
 ├─ TextRunNode ("World", Color=navy, FontWeight=bold)
 ├─ LineBreakNode
 └─ TextRunNode ("Next line", Color=navy)

🧠 Implementation Tip
You can store the mapping in a static lookup table like:
public static readonly Dictionary<string, HtmlTagRule> HtmlRules = new()
{
    ["div"] = new HtmlTagRule(NodeType.BlockNode, DisplayType.Block, new[] { "margin", "padding", "border", "background-color" }),
    ["span"] = new HtmlTagRule(NodeType.InlineNode, DisplayType.Inline, new[] { "color", "font-size", "font-weight", "font-style" }),
    ["b"] = new HtmlTagRule(NodeType.InlineNode, DisplayType.Inline, new[] { "font-weight" }, defaults: new { FontWeight = "bold" }),
    ["ul"] = new HtmlTagRule(NodeType.ListNode, DisplayType.Block, new[] { "list-style-type" }),
    // ...
};
Then during normalization:
var rule = HtmlRules.TryGetValue(tagName, out var r) ? r : HtmlRules["div"];
node = Activator.CreateInstance(r.NodeType);
ApplyDefaults(node, r.Defaults);
ApplyAllowedStyles(node, r.AllowedProperties, styleDict);


